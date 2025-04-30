using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using MapChooserSharp.API.Events.MapVote;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination.Interfaces;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapVote.Countdown;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.MapVote.Menus.Interfaces;
using MapChooserSharp.Modules.MapVote.Menus.SimpleHtml;
using MapChooserSharp.Modules.MapVote.Models;
using MapChooserSharp.Modules.Nomination;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace MapChooserSharp.Modules.MapVote;

internal sealed class McsMapVoteController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsMapVoteControllerApi
{
    public override string PluginModuleName => "McsMapVoteController";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
    
    private IMcsInternalEventManager _mcsEventManager = null!;
    private IMapConfigProvider _mapConfigProvider = null!;
    private McsMapCycleController _mapCycleController = null!;
    private McsMapNominationController _mapNominationController = null!;
    private ITimeLeftUtil _timeLeftUtil = null!;
    private IMcsMapVoteUiFactory _voteUiFactory = null!;
    
    private McsCountdownUiController _countdownUiController = null!;
    
    

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);

        // TODO() Toggle menu type from config
        services.AddTransient<IMcsMapVoteUiFactory, McsSimpleHtmlVoteUiFactory>();
    }

    protected override void OnAllPluginsLoaded()
    {
        _mapCycleController = ServiceProvider.GetRequiredService<McsMapCycleController>();
        _mapNominationController = ServiceProvider.GetRequiredService<McsMapNominationController>();
        _voteUiFactory = ServiceProvider.GetRequiredService<IMcsMapVoteUiFactory>();
        _countdownUiController = ServiceProvider.GetRequiredService<McsCountdownUiController>();
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        
        Plugin.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
        
        // For force resetting when map changed while voting
        Plugin.RegisterListener<Listeners.OnMapStart>(_ =>
        {
            CurrentVoteState = McsMapVoteState.NoActiveVote;
        });
    }


    protected override void OnUnloadModule()
    {
        Plugin.RemoveListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
    }
    

    private void OnClientDisconnect(int slot)
    {
        RemovePlayerVote(slot);
    }
    
    
    
    #region Vote Controll Area


    public FakeConVar<int> ConVarMaxVoteMenuElements = new("mcs_max_vote_menu_elements", "Max Vote Menu Elements", 6, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(2, 7));
    
    
    public McsMapVoteState CurrentVoteState { get; private set; } = McsMapVoteState.NoActiveVote;

    
    private int AllVotesCount {
        get
        {
            if (_mapVoteContent == null)
                return -1;

            return _mapVoteContent.GetVotingMaps().Sum(mapVoteData => mapVoteData.GetVoters().Count);
        }
    }
    
    private bool TEMP_SHOULD_VOTE_MENU_SHUFFLE = false;
    private bool TEMP_SHOW_ALIAS_NAME = true;
    private float TEMP_MAP_VOTE_END_TIME = 15.0F;
    private int TEMP_MAP_VOTE_COUNT_DOWN_TIME = 3;
    
    // If vote is not higher than TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD, then it will pick up maps, that higher than this percentage.
    private float TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD_WHEN_NO_WINNERS = 0.3F;
    
    // If vote is higher than this percent, it will end the vote.
    private float TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD = 0.7F;

    private const string IdExtendMap = "MapChooserSharp:ExtendMap";
    private const string IdDontChangeMap = "MapChooserSharp:DontChangeMap";

    internal readonly string PlaceHolderExtendMap = "%PLACE_HOLDER_EXTEND_MAP%";
    internal readonly string PlaceHolderDontChangeMap = "%PLACE_HOLDER_DONT_CHANGE_MAP%";
    
    private const int FallBackDefaultExtendTime = 15;
    private const int FallBackDefaultExtendRound = 15;

    private readonly Random _random = new();
    

    private IMapVoteContent? _mapVoteContent;

    private Timer? _mapVoteTimer;

    #region Vote Logic
    
    public McsMapVoteState InitiateVote(bool isActivatedByRtv = false)
    {
        if (CurrentVoteState != McsMapVoteState.NoActiveVote)
        {
            DebugLogger.LogWarning($"Vote initiation failed, Because we are another vote in progress! {CurrentVoteState}");
            return CurrentVoteState;
        }
        
        DebugLogger.LogInformation("Starting map vote");
        CurrentVoteState = McsMapVoteState.Initializing;
        
        DebugLogger.LogDebug($"This vote is initiated by RTV?: {isActivatedByRtv}");
        
        DebugLogger.LogTrace("Initializing MapVoteContent");
        
        // Respect menu's max elements
        int maxMenuElements = Math.Min(_voteUiFactory.MaxMenuElements, ConVarMaxVoteMenuElements.Value);
    
        Dictionary<string, IMapConfig> mapConfigs = _mapConfigProvider.GetMapConfigs();
        Dictionary<string, IMapConfig> unusedMapPool = new(mapConfigs);
        
        List<IMapVoteData> mapsToVote = new();
        List<IMcsVoteOption> voteOptions = new();

        
        
        void AddToVotingMaps(string mapName)
        {
            // If map is already added to voting maps
            if (!unusedMapPool.TryGetValue(mapName, out var mapConfig))
                return;

            string menuName = mapName;
            
            if (TEMP_SHOW_ALIAS_NAME && mapConfig.MapNameAlias != string.Empty)
            {
                menuName = mapConfig.MapNameAlias;
            }

            IMcsVoteOption voteOption = new McsVoteOption(menuName, CastPlayerVote);
            voteOptions.Add(voteOption);

            IMapVoteData voteData = new MapVoteData(mapConfig, mapName);

            mapsToVote.Add(voteData);
            unusedMapPool.Remove(mapName);
        }






        if (mapConfigs.Count < maxMenuElements)
        {
            DebugLogger.LogWarning("There is no enough maps to initiate vote!");
            CurrentVoteState = McsMapVoteState.NoActiveVote;
            
            PrintLocalizedChatToAll("MapVote.Broadcast.NotEnoughMapsToStartVote");
            return McsMapVoteState.Cancelling;
        }
        
        
        if (isActivatedByRtv && _mapCycleController.ExtendsLeft > 0)
        {
            DebugLogger.LogDebug("This vote is activated by RTV, first vote option is \"Don't change\"");
            McsVoteOption voteOption = new McsVoteOption(PlaceHolderDontChangeMap, CastPlayerVote);
            voteOptions.Add(voteOption);
            IMapVoteData voteData = new MapVoteData(null, IdDontChangeMap);
            mapsToVote.Add(voteData);
        }
        if (!isActivatedByRtv && _mapCycleController.ExtendsLeft > 0)
        {
            DebugLogger.LogDebug("This vote is not activated by RTV, first vote option is \"Extend current map\"");
            McsVoteOption voteOption = new McsVoteOption(PlaceHolderExtendMap, CastPlayerVote);
            voteOptions.Add(voteOption);
            IMapVoteData voteData = new MapVoteData(null, IdExtendMap);
            mapsToVote.Add(voteData);
        }
        
        
        Dictionary<string, IMcsNominationData> adminNominations = _mapNominationController
            .NominatedMaps
            .Where(v => v.Value.IsForceNominated)
            .ToDictionary();

        
        Dictionary<string, IMcsNominationData> sortedNominatedMaps = _mapNominationController
            .NominatedMaps
            .OrderByDescending(v => v.Value.NominationParticipants.Count)
            .ToDictionary();
        
        
        DebugLogger.LogDebug("Adding admin nominated maps to vote map list");
        foreach (var (key, value) in adminNominations)
        {
            AddToVotingMaps(key);
        }
        
        DebugLogger.LogDebug("Adding nominated maps to vote map list");
        foreach (var (key, value) in sortedNominatedMaps)
        {
            AddToVotingMaps(key);
        }



        DebugLogger.LogDebug("Collecting possible vote participates");
        HashSet<int> voteParticipants = Utilities.GetPlayers().Where(p => p is { IsHLTV: false, IsBot: false }).Select(p => p.Slot).ToHashSet();
        DebugLogger.LogDebug($"Possible participants count: {voteParticipants.Count}");
        

        
        
        // After processing the nominated maps, remove the current map from the unused map pool
        // so it won't be shown in randomly picked maps
        unusedMapPool.Remove(Server.MapName);

        int mapSlotsRemaining = maxMenuElements - mapsToVote.Count;
        
        if (mapSlotsRemaining > 0)
        {
            DebugLogger.LogDebug("We don't have enough nominated maps to fill map vote, picking up the random map...");
            
            var numToPick = Math.Min(mapSlotsRemaining, unusedMapPool.Count);
            DebugLogger.LogDebug($"{numToPick} maps will be chosen randomely");
            var unusedMapList = unusedMapPool.ToList();
            var pickedMaps = unusedMapList
                .OrderBy(_ => _random.Next())
                .Where(map => map.Value.MapCooldown.CurrentCooldown <= 0)
                .Where(map => !map.Value.OnlyNomination)
                .Where(map => !map.Value.NominationConfig.RestrictToAllowedUsersOnly)
                .Where(map => map.Value.NominationConfig.MinPlayers == 0 || map.Value.NominationConfig.MinPlayers <= Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false }))
                .Where(map => map.Value.NominationConfig.MaxPlayers == 0 || map.Value.NominationConfig.MaxPlayers > Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false }))
                .Where(map => !map.Value.NominationConfig.RequiredPermissions.Any())
                .Where(map => !map.Value.NominationConfig.DaysAllowed.Any() || map.Value.NominationConfig.DaysAllowed.Contains(DateTime.Today.DayOfWeek) )
                .Where(map => !map.Value.NominationConfig.AllowedTimeRanges.Any() || map.Value.NominationConfig.AllowedTimeRanges.Count(range => range.IsInRange(TimeOnly.FromDateTime(DateTime.Now))) < 1 )
                .Take(numToPick);

            foreach (var (key, value) in pickedMaps)
            {
                DebugLogger.LogTrace($"Adding random map: {key}");
                AddToVotingMaps(key);
            }
        }
        
        DebugLogger.LogTrace("Setting vote option");
        var voteUi = _voteUiFactory.Create();
        voteUi.SetVoteOptions(voteOptions);
        voteUi.SetRandomShuffle(TEMP_SHOULD_VOTE_MENU_SHUFFLE);
        
        
        
        DebugLogger.LogDebug("Creating a new MapVoteContent instance");
        _mapVoteContent = new MapVoteContent(voteParticipants, mapsToVote, voteUi, isActivatedByRtv);
        DebugLogger.LogTrace($"Obtained: {_mapVoteContent.GetType().FullName}");
        
        DebugLogger.LogInformation("Initialize successfully");

        int count = TEMP_MAP_VOTE_COUNT_DOWN_TIME;
        _mapVoteTimer = Plugin.AddTimer(1.0F, () =>
        {
            if (count <= 0)
            {
                _mapVoteTimer?.Kill();
                _countdownUiController.CloseCountdownUiAll();
                StartVote();
                return;
            }
            _countdownUiController.ShowCountdownToAll(count);
            count--;
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

        FireVoteInitiatedEvent();
        return McsMapVoteState.InitializeAccepted;
    }

    private void StartVote()
    {
        if (CurrentVoteState != McsMapVoteState.Initializing)
        {
            DebugLogger.LogWarning($"CurrentVoteState: {CurrentVoteState} is not initializing, so vote is cancelled");
            return;
        }

        if (_mapVoteContent == null)
        {
            DebugLogger.LogError($"MapVoteContent is null, vote cannot be started!");
            return;
        }
        
        CurrentVoteState = McsMapVoteState.Voting;
        
        var voteParticipants = _mapVoteContent.GetVoteParticipants();

        ShowVoteMenu(voteParticipants);

        _mapVoteTimer = Plugin.AddTimer(TEMP_MAP_VOTE_END_TIME, EndVote, TimerFlags.STOP_ON_MAPCHANGE);

        FireVoteStartedEvent();
    }
    
    private void EndVote()
    {
        if (_mapVoteContent == null)
        {
            CurrentVoteState = McsMapVoteState.NoActiveVote;
            return;
        }

        bool isActivatedByRtv = _mapVoteContent.IsRtvVote;
        
        DebugLogger.LogInformation("Finalizing vote...");
        
        CurrentVoteState = McsMapVoteState.Finalizing;
        
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (player.IsBot || player.IsHLTV)
                continue;
            
            MenuManager.CloseActiveMenu(player);
        }
        
        foreach (IMapVoteData voteData in _mapVoteContent.GetVotingMaps())
        {
            DebugLogger.LogTrace($"Vote data for map: {voteData.MapName}");
            DebugLogger.LogTrace($"Voters slots: {string.Join(", " ,voteData.GetVoters())}");
        }


        int totalVotes = AllVotesCount;

        if (totalVotes == 0)
        {
            DebugLogger.LogDebug("There is no votes picking random map...");

            // Remove nullable map config (extend map and don't change)
            _mapVoteContent.GetVotingMaps().RemoveAll(data => data.MapConfig == null);
            
            var pickedMaps = _mapVoteContent
                .GetVotingMaps()
                .OrderBy(_ => _random.Next())
                .Take(1);

            var mapCfg = pickedMaps.First().MapConfig!;
            
            PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.NoVotes", mapCfg.MapName);
            FireNextMapConfirmedEvent(mapCfg);
            EndVotePostInitialization();
            CurrentVoteState = McsMapVoteState.NextMapConfirmed;
            return;
        }
        
        float votePercentage = (float)_mapVoteContent.GetVoteParticipants().Count / totalVotes;
        PrintLocalizedChatToAll("MapVote.Broadcast.VoteFinished", totalVotes ,_mapVoteContent.GetVoteParticipants().Count , $"{votePercentage * 100:F2}");
        
        List<IMapVoteData> winners = PickWinningMaps(_mapVoteContent.GetVotingMaps());

        // If winners count is higher than 2, then we'll start run off vote
        if (winners.Count > 1)
        {
            PrintLocalizedChatToAll("MapVote.Broadcast.StartingRunoffVote", $"{TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD*100:F2}");
            
            _mapVoteTimer?.Kill();
            InitializeRunOffVote(winners);
            return;
        }

        var winMap = winners.First();
        
        // If MapConfig is null, then this is "extend map" or "don't change"
        if (winMap.MapConfig == null)
        {
            ProcessNonMapWinner(_mapVoteContent, winMap);
            return;
        }
        
        float mapVotePercentage = (float)winMap.GetVoters().Count / totalVotes * 100.0F;

        PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.NextMapConfirmed", winMap.MapConfig.MapName, $"{mapVotePercentage:F2}", totalVotes);

        FireVoteFinishedEvent();
        FireNextMapConfirmedEvent(winMap.MapConfig);
        EndVotePostInitialization();
        CurrentVoteState = McsMapVoteState.NextMapConfirmed;
    }
    
    #endregion
    
    #region Runoff vote logic

    private void InitializeRunOffVote(List<IMapVoteData> votes)
    {
        DebugLogger.LogInformation("Starting runoff vote");
        CurrentVoteState = McsMapVoteState.Initializing;
        
        List<IMapVoteData> mapsToVote = new();
        List<IMcsVoteOption> voteOptions = new();
        
        foreach (IMapVoteData vote in votes)
        {
            // To determine vote data is "extend map" or "don't change"
            if (vote.MapConfig == null)
            {
                string nonMapMenuName = "";
                if (!_mapVoteContent?.IsRtvVote ?? false)
                {
                    nonMapMenuName = PlaceHolderExtendMap;
                }
                else
                {
                    nonMapMenuName = PlaceHolderDontChangeMap;
                }
                voteOptions.Add(new McsVoteOption(nonMapMenuName, CastPlayerVote));
                mapsToVote.Add(new MapVoteData(null, vote.MapName));
                continue;
            }
            
            string menuName = vote.MapName;
            
            if (TEMP_SHOW_ALIAS_NAME && vote.MapConfig.MapNameAlias != string.Empty)
            {
                menuName = vote.MapConfig.MapNameAlias;
            }
            
            voteOptions.Add(new McsVoteOption(menuName, CastPlayerVote));

            mapsToVote.Add(new MapVoteData(vote.MapConfig, vote.MapName));
        }
        
        DebugLogger.LogDebug("Collecting possible vote participates");
        HashSet<int> voteParticipants = Utilities.GetPlayers().Where(p => p is { IsHLTV: false, IsBot: false }).Select(p => p.Slot).ToHashSet();
        DebugLogger.LogDebug($"Possible participants count: {voteParticipants.Count}");
        
        
        DebugLogger.LogDebug("Setting vote option");
        var voteUi = _voteUiFactory.Create();
        voteUi.SetVoteOptions(voteOptions);
        voteUi.SetRandomShuffle(TEMP_SHOULD_VOTE_MENU_SHUFFLE);
        
        var newVoteContent = new MapVoteContent(voteParticipants, mapsToVote, voteUi, _mapVoteContent?.IsRtvVote ?? false);
        _mapVoteContent = newVoteContent;
        
        DebugLogger.LogInformation("Initialize successfully");

        int count = TEMP_MAP_VOTE_COUNT_DOWN_TIME;
        _mapVoteTimer = Plugin.AddTimer(1.0F, () =>
        {
            if (count <= 0)
            {
                _mapVoteTimer?.Kill();
                _countdownUiController.CloseCountdownUiAll();
                StartRunOffVote();
                return;
            }
            
            _countdownUiController.ShowCountdownToAll(count);
            count--;
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

        FireVoteInitiatedEvent();
    }

    private void StartRunOffVote()
    {
        if (CurrentVoteState != McsMapVoteState.Initializing)
        {
            DebugLogger.LogWarning($"CurrentVoteState: {CurrentVoteState} is not initializing, so runoff vote is cancelled");
            return;
        }

        if (_mapVoteContent == null)
        {
            DebugLogger.LogError("MapVoteContent is null, runoff vote cannot be started!");
            return;
        }
        
        CurrentVoteState = McsMapVoteState.RunoffVoting;
        
        var voteParticipants = _mapVoteContent.GetVoteParticipants();
        
        DebugLogger.LogDebug($"Runoff vote participants: {voteParticipants.Count}");
        
        ShowVoteMenu(voteParticipants);

        _mapVoteTimer = Plugin.AddTimer(TEMP_MAP_VOTE_END_TIME, EndRunoffVote, TimerFlags.STOP_ON_MAPCHANGE);

        FireVoteStartedEvent();
    }

    private void EndRunoffVote()
    {
        if (_mapVoteContent == null)
        {
            CurrentVoteState = McsMapVoteState.NoActiveVote;
            return;
        }

        bool isActivatedByRtv = _mapVoteContent.IsRtvVote;
    
        DebugLogger.LogInformation("Finalizing vote...");
    
        CurrentVoteState = McsMapVoteState.Finalizing;
    
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (player.IsBot || player.IsHLTV)
                continue;
        
            MenuManager.CloseActiveMenu(player);
        }
    
        foreach (IMapVoteData voteData in _mapVoteContent.GetVotingMaps())
        {
            DebugLogger.LogTrace($"Vote data for map: {voteData.MapName}");
            DebugLogger.LogTrace($"Voters slots: {string.Join(", " ,voteData.GetVoters())}");
        }


        int totalVotes = AllVotesCount;

        if (totalVotes == 0)
        {
            DebugLogger.LogDebug("There is no votes picking random map...");

            // Remove nullable map config (extend map and don't change)
            _mapVoteContent.GetVotingMaps().RemoveAll(data => data.MapConfig == null);
            
            var pickedMaps = _mapVoteContent
                .GetVotingMaps()
                .OrderBy(_ => _random.Next())
                .Take(1);

            var mapCfg = pickedMaps.First().MapConfig!;
            
            PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.NoVotes", mapCfg.MapName);
            FireNextMapConfirmedEvent(mapCfg);
            EndVotePostInitialization();
            CurrentVoteState = McsMapVoteState.NextMapConfirmed;
            return;
        }

        List<IMapVoteData> winners = PickWinningMaps(_mapVoteContent.GetVotingMaps());

        var winMap = winners.First();

        float votePercentage = (float)_mapVoteContent.GetVoteParticipants().Count / totalVotes;
        
        
        PrintLocalizedChatToAll("MapVote.Broadcast.VoteFinished", totalVotes ,_mapVoteContent.GetVoteParticipants().Count , $"{votePercentage * 100:F2}");
        
        // If MapConfig is null, then this is "extend map" or "don't change"
        if (winMap.MapConfig == null)
        {
            ProcessNonMapWinner(_mapVoteContent, winMap);
            return;
        }
        
        float mapVotePercentage = (float)winMap.GetVoters().Count / totalVotes * 100.0F;

        PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.NextMapConfirmed", winMap.MapConfig.MapName, $"{mapVotePercentage:F2}", totalVotes);
        
        FireVoteFinishedEvent();
        FireNextMapConfirmedEvent(winMap.MapConfig);
        
        EndVotePostInitialization();
        CurrentVoteState = McsMapVoteState.NextMapConfirmed;
    }
    
    #endregion
    
    
    #region Cancel vote logic
    
    public McsMapVoteState CancelVote(CCSPlayerController? player = null)
    {
        if (CurrentVoteState is McsMapVoteState.Cancelling or McsMapVoteState.NoActiveVote
            or McsMapVoteState.NextMapConfirmed)
        {
            DebugLogger.LogWarning($"Vote cancellation failed, Because there are no active vote available or next map is confirmed! state: {CurrentVoteState}");
            return CurrentVoteState;
        }

        FireVoteCancelEvent();
        EndVotePostInitialization();

        string executorName = PlayerUtil.GetPlayerName(player);
        
        PrintLocalizedChatToAll("MapVote.Broadcast.Admin.CancelVote", executorName);
        return McsMapVoteState.Cancelling;
    }
    
    #endregion

    #region Utilities

    private void ProcessNonMapWinner(IMapVoteContent mapVoteContent, IMapVoteData winnerData)
    {
        float mapVotePercentage = (float)winnerData.GetVoters().Count / AllVotesCount * 100.0F;
        
        if (mapVoteContent.IsRtvVote)
        {
            PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.NotChanging", mapVotePercentage, AllVotesCount);
            FireMapNotChangedEvent();
        }
        else
        {
            PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.Extend", mapVotePercentage, AllVotesCount);
            McsMapExtendType extendType = _timeLeftUtil.ExtendType;
            DebugLogger.LogTrace($"Determined extend type to extend {extendType}");
            ExtendCurrentMap(extendType);
        }
        
        EndVotePostInitialization();
    }
    
    private void EndVotePostInitialization()
    {
        _mapVoteContent = null;
        _mapVoteTimer?.Kill();
        _mapVoteTimer = null;
        CurrentVoteState = McsMapVoteState.NoActiveVote;
    }

    private void FireNextMapConfirmedEvent(IMapConfig mapConfig)
    {
        var confirmedEvent = new McsNextMapConfirmedEvent(GetTextWithPluginPrefix(""), mapConfig);
        _mcsEventManager.FireEventNoResult(confirmedEvent);
    }

    private void FireMapNotChangedEvent()
    {
        var notChangedEvent = new McsMapNotChangedEvent(GetTextWithPluginPrefix(""));
        _mcsEventManager.FireEventNoResult(notChangedEvent);
    }

    private void FireMapExtendEvent(int extendTime, McsMapExtendType extendType)
    {
        var extendEvent = new McsMapExtendEvent(GetTextWithPluginPrefix(""), extendTime, extendType);
        _mcsEventManager.FireEventNoResult(extendEvent);
    }

    private void FireVoteInitiatedEvent()
    {
        var voteInitiatedEvent = new McsMapVoteInitiatedEvent(GetTextWithPluginPrefix(""));
        _mcsEventManager.FireEventNoResult(voteInitiatedEvent);
    }

    private void FireVoteStartedEvent()
    {
        var voteStartedEvent = new McsMapVoteStartedEvent(GetTextWithPluginPrefix(""));
        _mcsEventManager.FireEventNoResult(voteStartedEvent);
    }

    private void FireVoteFinishedEvent()
    {
        var voteFinishedEvent = new McsMapVoteFinishedEvent(GetTextWithPluginPrefix(""));
        _mcsEventManager.FireEventNoResult(voteFinishedEvent);
    }

    private void FireVoteCancelEvent()
    {
        var voteCancelledEvent = new McsMapVoteCancelledEvent(GetTextWithPluginPrefix(""));
        _mcsEventManager.FireEventNoResult(voteCancelledEvent);
    }
    
    

    private void ShowVoteMenu(HashSet<int> voteParticipants)
    {
        if (_mapVoteContent == null)
        {
            DebugLogger.LogWarning("Tried to open a vote menu, but there is no ongoing vote available!");
            return;
        }
        
        foreach (int participantSlot in voteParticipants)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(participantSlot);
            
            if (player == null)
                continue;
            
            DebugLogger.LogTrace($"Showing menu: {player.PlayerName}");
            _mapVoteContent?.VoteUi.OpenMenu(player);
        }
    }
    
    
    private List<IMapVoteData> PickWinningMaps(List<IMapVoteData> votingMaps)
    {
        DebugLogger.LogDebug("Picking winning map(s)");
        if (_mapVoteContent == null)
        {
            DebugLogger.LogError("There is no ongoing votes! returning empty list!");
            return [];
        }
        
        
        List<IMapVoteData> winners = new();

        List<IMapVoteData> sortedVotingMaps =
            votingMaps.OrderByDescending(v => v.GetVoters().Count).ToList();
        
        int topVotes = sortedVotingMaps.First().GetVoters().Count;
        DebugLogger.LogTrace($"Top voted map: {sortedVotingMaps.First().MapName}, total {topVotes} votes");
        

        int totalVotes = AllVotesCount;
        
        if (totalVotes == 0)
        {
            DebugLogger.LogWarning("Total vote is 0! Returning first element of list to avoid 0 division, and picking random map.");
            return [sortedVotingMaps.First()];
        }
        
        foreach (IMapVoteData map in sortedVotingMaps)
        {
            float votePercentage = (float)map.GetVoters().Count / totalVotes;
            
            DebugLogger.LogTrace($"{map.MapName} Vote percentage: {votePercentage*100:F1}% > Threshold {TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD*100:F0}%");
            if (votePercentage >= TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD)
            {
                winners.Add(map);
            }
        }
        
        if (!winners.Any())
        {
            DebugLogger.LogDebug($"No winning map found! Picking maps with over {TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD_WHEN_NO_WINNERS*100:F0}% of votes");
            foreach (IMapVoteData map in sortedVotingMaps)
            {
                float votePercentage = (float)map.GetVoters().Count / totalVotes;
            
                DebugLogger.LogTrace($"{map.MapName} Vote percentage: {votePercentage*100:F1}% > Threshold {TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD_WHEN_NO_WINNERS*100:F0}%");
                if (votePercentage >= TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD_WHEN_NO_WINNERS)
                {
                    winners.Add(map);
                }
            }

            // If only 1 map is higher than TEMP_MAP_VOTE_WINNER_PICK_UP_THRESHOLD_WHEN_NO_WINNERS
            // Then add 1 more maps
            if (winners.Count <= 1)
            {
                DebugLogger.LogDebug($"Not enough maps for starting vote, we'll pick up one more map for runoff vote.");
                foreach (IMapVoteData map in sortedVotingMaps)
                {
                    if (winners.Count > 1)
                        break;
                    
                    if (winners.Contains(map))
                        continue;
                    
                    winners.Add(map);
                }
            }
        }
        
        return winners;
    }

    public void PlayerReVote(CCSPlayerController player)
    {
        if (CurrentVoteState != McsMapVoteState.Voting)
            return;
        
        if (_mapVoteContent == null)
            return;

        if (!_mapVoteContent.IsPlayerInVoteParticipant(player.Slot))
        {
            DebugLogger.LogDebug($"Player {player.PlayerName} tried to revote the current vote. but they are not a participant of current vote!");
            return;
        }

        DebugLogger.LogDebug($"Player {player.PlayerName} is trying to revote");
        RemovePlayerVote(player.Slot);
        
        _mapVoteContent.VoteUi.OpenMenu(player);
    }



    /// <summary>
    /// Determines map is in group cooldown/map cooldown or not
    /// </summary>
    /// <param name="mapConfig">Map config data</param>
    /// <returns>True if in cooldown, otherwise false</returns>
    private bool IsMapInCooldown(IMapConfig mapConfig)
    {
        if (mapConfig.MapCooldown.CurrentCooldown > 0)
            return true;

        if (mapConfig.GroupSettings.Any(cd => cd.GroupCooldown.CurrentCooldown > 0))
            return true;

        return false;
    }


    private void CastPlayerVote(CCSPlayerController player, byte voteIndex)
    {
        if (_mapVoteContent == null)
            return;
        
        DebugLogger.LogDebug($"Player casted a vote! Player: {player.PlayerName}, VoteIndex: {voteIndex}");
        _mapVoteContent.GetVotingMaps()[voteIndex].AddVoter(player.Slot);
        _mapVoteContent.VoteUi.CloseMenu(player);

        if (AllVotesCount >= _mapVoteContent.GetVoteParticipants().Count)
        {
            if (CurrentVoteState == McsMapVoteState.Voting)
            {
                EndVote();
            }

            if (CurrentVoteState == McsMapVoteState.RunoffVoting)
            {
                EndRunoffVote();
            }
        }
    }

    public void RemovePlayerVote(int slot)
    {
        if (_mapVoteContent == null)
            return;

        DebugLogger.LogDebug($"Trying to remove player vote for slot: {slot}");
        var mapVoteData = GetPlayerVotedMap(slot);
        mapVoteData?.RemoveVoter(slot);
    }

    private IMapVoteData? GetPlayerVotedMap(int slot)
    {
        try
        {
            return _mapVoteContent?.GetVotingMaps().First(p => p.GetVoters().Contains(slot));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void ExtendCurrentMap(McsMapExtendType type)
    {
        int extendTime;

        switch (type)
        {
            case McsMapExtendType.TimeLimit:
                extendTime = _mapCycleController.CurrentMap?.ExtendTimePerExtends ?? FallBackDefaultExtendTime;

                PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.ExtendForTime", extendTime);
                FireMapExtendEvent(extendTime, type);
                break;
            case McsMapExtendType.RoundTime:
                extendTime = _mapCycleController.CurrentMap?.ExtendTimePerExtends ?? FallBackDefaultExtendTime;
                
                PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.ExtendForTime", extendTime);
                FireMapExtendEvent(extendTime, type);
                break;
            case McsMapExtendType.Rounds:
                var extendRound = _mapCycleController.CurrentMap?.ExtendRoundsPerExtends ?? FallBackDefaultExtendRound;
                
                PrintLocalizedChatToAll("MapVote.Broadcast.VoteResult.ExtendForRound", extendRound);
                FireMapExtendEvent(extendRound, type);
                break;
        }
    }
    #endregion
    
    #endregion
}