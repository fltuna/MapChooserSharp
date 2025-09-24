using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Timers;
using MapChooserSharp.API.Events.MapCycle;
using MapChooserSharp.API.Events.MapVote;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.MapCycle.Services;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using MapChooserSharp.Modules.RockTheVote.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Other;
using ZLinq;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace MapChooserSharp.Modules.MapCycle;

internal sealed class McsMapCycleController(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider), IMcsInternalMapCycleControllerApi
{
    public override string PluginModuleName => "McsMapCycleController";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private IMcsInternalEventManager _mcsEventManager = null!;
    private IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = null!;
    private IMcsPluginConfigProvider _mcsPluginConfigProvider = null!;
    private IMcsInternalMapVoteControllerApi _mcsMapVoteController = null!;
    private IMcsInternalRtvControllerApi _mcsRtvController = null!;
    private McsMapConfigExecutionService _mapConfigExecutionService = null!;
    private IMcsDatabaseProvider _mcsDatabaseProvider = null!;
    private ITimeLeftUtil _timeLeftUtil = null!;

    
    private IMapConfig? _nextMap = null;

    public IMapConfig? NextMap
    {
        get => _nextMap;
        private set => _nextMap = value;
    }
    
    public bool IsNextMapConfirmed => _nextMap != null;

    public bool ChangeMapOnNextRoundEnd { get; set; } = false;

    private IMapConfig? _currentMap = null;

    public IMapConfig? CurrentMap
    {
        get
        {
            return _currentMap ??= _mcsInternalMapConfigProviderApi.GetMapConfig(Server.MapName);
        }
        private set => _currentMap = value;
    }

    public int ExtendCount { get; private set; } = 0;
    private int ExtendLimit { get; set; } = 0;
    
    public int ExtendsLeft => ExtendLimit - ExtendCount;
    
    public bool SetNextMap(IMapConfig mapConfig)
    {
        NextMap = mapConfig;

        FireNextMapChangedEvent(NextMap);
        return true;
    }

    public bool SetNextMap(string mapName)
    {
        IMapConfig? mapConfig = _mcsInternalMapConfigProviderApi.GetMapConfig(mapName);

        if (mapConfig == null)
            return false;

        NextMap = mapConfig;

        FireNextMapChangedEvent(NextMap);
        return true;
    }

    public bool RemoveNextMap()
    {
        if (NextMap == null)
            return false;
        
        FireNextMapRemovedEvent(NextMap);
        
        _mapChangeTimer?.Kill();
        ChangeMapOnNextRoundEnd = false;
        NextMap = null;
        RecreateVoteTimer();
        return true;
    }

    
    private int DefaultMapExtends => _mcsPluginConfigProvider.PluginConfig.MapCycleConfig.FallbackDefaultMaxExtends;


    private bool _isMapStarted = false;

    private Timer? _voteStartTimer = null;

    private Timer? _mapChangeTimer = null;

    private const float VoteStartCheckInterval = 1.0F;
    
    private const float DefaultRoundRestartDelay = 7.0F;

    private const float DefaultMapChangeDelay = 10.0F;
    
    
    // Those variables are used for avoid unexpected cooldown reduction when server startup
    private bool IsFirstMapEnded { get; set; }
    private bool IsSecondMapIsPassed { get; set; }

    public readonly FakeConVar<int> VoteStartTimingTime = new("mcs_vote_start_timing_time", "When should vote started if map is based on mp_timelimit or mp_roundtime? (seconds)", 180,
        ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 600));
    
    public readonly FakeConVar<int> VoteStartTimingRound = new("mcs_vote_start_timing_round", "When should vote started if map is based on mp_maxrounds? (rounds)", 2,
        ConVarFlags.FCVAR_NONE, new RangeValidator<int>(2, 15));
    
    protected override void OnInitialize()
    {
        TrackConVar(VoteStartTimingTime);
        TrackConVar(VoteStartTimingRound);
    }
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMcsInternalMapCycleControllerApi>(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
        _mcsRtvController = ServiceProvider.GetRequiredService<IMcsInternalRtvControllerApi>();
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _mcsPluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        _mcsDatabaseProvider = ServiceProvider.GetRequiredService<IMcsDatabaseProvider>();
        _mapConfigExecutionService = ServiceProvider.GetRequiredService<McsMapConfigExecutionService>();
        
        _mcsEventManager.RegisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.RegisterEventHandler<McsMapExtendEvent>(OnMapExtended);
        _mcsEventManager.RegisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        Plugin.RegisterListener<Listeners.OnMapEnd>(() =>
        {
            _isMapStarted = false;
            ChangeMapOnNextRoundEnd = false;
            IsFirstMapEnded = true;
        });
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        Plugin.RegisterEventHandler<EventCsIntermission>(OnIntermission);
        
        // This is for late timer start
        // Since we cannot obtain McsMapExtendType before map is fully loaded
        // So we'll wait for first round started
        Plugin.RegisterEventHandler<EventRoundPoststart>((@event, info) =>
        {
            if (_isMapStarted)
                return HookResult.Continue;
            
            _timeLeftUtil.ReDetermineExtendType();
            _isMapStarted = true;
            RecreateVoteTimer();
            return HookResult.Continue;
        });

        if (hotReload)
        {
            OnMapStart(Server.MapName);
            _timeLeftUtil.ReDetermineExtendType();
            _isMapStarted = true;
            RecreateVoteTimer();
            IsFirstMapEnded = true;
            IsSecondMapIsPassed = true;
        }
    }

    protected override void OnUnloadModule()
    {
        _mcsEventManager.UnregisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.UnregisterEventHandler<McsMapExtendEvent>(OnMapExtended);
        _mcsEventManager.UnregisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        
        Plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
        Plugin.DeregisterEventHandler<EventCsIntermission>(OnIntermission);
    }


    public void ChangeToNextMap(float seconds)
    {
        if (seconds < 0.0F)
            seconds = DefaultMapChangeDelay;
        
        _mapChangeTimer = Plugin.AddTimer(seconds, ChangeToNextMapInternal, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ChangeToNextMapInternal()
    {
        
        if (NextMap == null)
        {
            Logger.LogError("Failed to change map: next map is null");
            return;
        }
        
        if (_mcsPluginConfigProvider.PluginConfig.MapCycleConfig.ShouldStopSourceTvRecording)
        {
            Logger.LogInformation("Executing tv_stoprecord before map change to prevent server crash.");
            Server.ExecuteCommand("tv_stoprecord");
        }

        DebugLogger.LogDebug("Changing to next map!");
        long workshopId = NextMap.WorkshopId;

        if (workshopId == 0)
        {
            DebugLogger.LogInformation($"No workshop ID defined! We will try change level with {NextMap.MapName}");
            // Use MapUtil.ChangeMap(string) instead of MapUtil.ChangeToWorkshopMap(string)
            // Because, This IMapConfig is no guarantee official map or not.
            MapUtil.ChangeMap(NextMap.MapName);
            return;
        }

        DebugLogger.LogInformation($"We will try to change map to {NextMap.MapName} with workshop ID: {workshopId}");
        MapUtil.ChangeToWorkshopMap(workshopId);
    }

    private void OnClientPutInServer(int slot)
    {
        // If current timelimit/round is lowerthan vote start time/round threshold
        // And first player is joined to server, then set nextmap randomly
        if (!GetCorrespondTimelimitCheck().Invoke() && Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false }) == 0)
        {
            NextMap = PickRandomMapForNextMap();
            Logger.LogWarning($"{NextMap.MapName} has picked for nextmap because not enough time to vote, but no nextmap has specified.");
            FireNextMapChangedEvent(NextMap);
        }
        
        // TODO if current map is official maps, then set IsSecondMapIsPassed to true.
        if (IsSecondMapIsPassed || !IsFirstMapEnded)
            return;
        
        IsSecondMapIsPassed = true;
    }

    private void OnMapStart(string mapName)
    {
        ExtendCount = 0;
        
        CurrentMap = NextMap;
        NextMap = null;
        
        DecrementAllMapCooldown(CurrentMap);
        ApplyCooldownToCurrentMap(CurrentMap);
        
        Server.NextFrame(ExecuteMapLimitConfig);

        // Wait for first people joined
        // TODO() Maybe we can use Server.NextWorldUpdate() to execute things?
        Plugin.AddTimer(0.1F, () =>
        {
            ObtainCurrentMap(mapName);
            _mapConfigExecutionService.ExecuteMapConfigs();
        });
    }

    private void ObtainCurrentMap(string mapName)
    {
        // This is extra check for server startup
        if (CurrentMap == null)
        {
            CurrentMap = _mcsInternalMapConfigProviderApi.GetMapConfig(mapName);
            
            // If map name isn't match with config then find with workshop ID
            if (CurrentMap == null)
            {
                if (long.TryParse(ForceFullUpdate.GetWorkshopId(), out long workshopId))
                {
                    // Find map config my workshopId
                    // But if not find with this way, CurrentMap become null.
                    CurrentMap = _mcsInternalMapConfigProviderApi.GetMapConfig(workshopId);
                }
            }
        }

        ExtendLimit = CurrentMap?.MaxExtends ?? DefaultMapExtends;
    }

    private void DecrementAllMapCooldown(IMapConfig? previousMap)
    {
        // To prevent unxpected cooldown reduction
        if (!IsFirstMapEnded || !IsSecondMapIsPassed)
            return;
        
        // Decrement all cooldowns
        _mcsDatabaseProvider.MapInfoRepository.DecrementAllCooldownsAsync().ConfigureAwait(false);
        _mcsDatabaseProvider.GroupInfoRepository.DecrementAllCooldownsAsync().ConfigureAwait(false);
        foreach (var (key, value) in _mcsInternalMapConfigProviderApi.GetMapConfigs())
        {
            foreach (IMapGroupSettings setting in value.GroupSettings)
            {
                if (setting.GroupCooldown.CurrentCooldown > 0)
                    setting.GroupCooldown.CurrentCooldown--;
            }

            if (value.MapCooldown.CurrentCooldown > 0)
                value.MapCooldown.CurrentCooldown--;
        }
    }
    
    private void ApplyCooldownToCurrentMap(IMapConfig? currentMap)
    {
        if (currentMap == null)
            return;
            
        _mcsDatabaseProvider.MapInfoRepository.UpsertMapCooldownAsync(currentMap.MapName, currentMap.MapCooldown.MapConfigCooldown).ConfigureAwait(false);
        currentMap.MapCooldown.CurrentCooldown = currentMap.MapCooldown.MapConfigCooldown;
        
        foreach (IMapGroupSettings setting in currentMap.GroupSettings)
        {
            _mcsDatabaseProvider.GroupInfoRepository.UpsertGroupCooldownAsync(setting.GroupName, setting.GroupCooldown.MapConfigCooldown).ConfigureAwait(false);
            setting.GroupCooldown.CurrentCooldown = setting.GroupCooldown.MapConfigCooldown;
        }
    }
        
    
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (!_isMapStarted)
            return HookResult.Continue;
        
        if (NextMap == null)
            return HookResult.Continue;

        if (ChangeMapOnNextRoundEnd)
        {
            ChangeToNextMap(1.0F);
            return HookResult.Continue;
        }
        
        return HookResult.Continue;
    }

    private HookResult OnIntermission(EventCsIntermission @event, GameEventInfo info)
    {
        if (!_isMapStarted)
            return HookResult.Continue;

        if (NextMap == null)
            return HookResult.Continue;

        if (ChangeMapOnNextRoundEnd)
            return HookResult.Continue;

        McsMapExtendType extendType = _timeLeftUtil.ExtendType;

        if (extendType == McsMapExtendType.TimeLimit && _timeLeftUtil.TimeLimit > 0)
            return HookResult.Continue;

        if (extendType == McsMapExtendType.Rounds && _timeLeftUtil.RoundsLeft > 0)
            return HookResult.Continue;

        if (extendType == McsMapExtendType.RoundTime && _timeLeftUtil.RoundTimeLeft > 0)
            return HookResult.Continue;
        
        // TODO() 将来的に、nativeなmap投票を使うようになる可能性もあるので、取得するConVarを柔軟に変更できるようにする
        ConVar? mp_competitive_endofmatch_extra_time = ConVar.Find("mp_competitive_endofmatch_extra_time");

        float delay = mp_competitive_endofmatch_extra_time?.GetPrimitiveValue<float>() ?? DefaultRoundRestartDelay;
        
        ChangeToNextMap(delay-1);
        
        return HookResult.Continue;
    }


    private void OnNextMapConfirmed(McsNextMapConfirmedEvent @event)
    {
        NextMap = @event.MapConfig;
    }

    private void OnMapExtended(McsMapExtendEvent @event)
    {
        ExtendCount++;
        
        switch (@event.MapExtendType)
        {
            case McsMapExtendType.TimeLimit:
                _timeLeftUtil.ExtendTimeLimit(@event.ExtendTime);
                break;
            case McsMapExtendType.Rounds:
                _timeLeftUtil.ExtendRounds(@event.ExtendTime);
                break;
            case McsMapExtendType.RoundTime:
                _timeLeftUtil.ExtendRoundTime(@event.ExtendTime);
                break;
        }

        RecreateVoteTimer();
    }

    // This method is redundant, but left for just in case.
    private void OnMapNotChanged(McsMapNotChangedEvent @event)
    {
        RecreateVoteTimer();
    }

    private void RecreateVoteTimer()
    {
        _voteStartTimer?.Kill();

        CreateVoteStartTimer();
    }
    
    private void CreateVoteStartTimer()
    {
        var shouldContinueCheck = GetCorrespondTimelimitCheck();
        _voteStartTimer = Plugin.AddTimer(VoteStartCheckInterval, () =>
        {
            if (!_isMapStarted)
                return;
            
            if (shouldContinueCheck())
                return;
        
            if (_mcsMapVoteController.CurrentVoteState == McsMapVoteState.NextMapConfirmed)
            {
                _voteStartTimer?.Kill();
                _voteStartTimer = null;
                return;
            }

            if (_mcsMapVoteController.CurrentVoteState == McsMapVoteState.NoActiveVote)
            {
                InitiateVote();
            }
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }

    private Func<bool> GetCorrespondTimelimitCheck()
    {
        switch (_timeLeftUtil.ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                return () => _timeLeftUtil.TimeLimit > VoteStartTimingTime.Value;
            
            case McsMapExtendType.RoundTime:
                return () => _timeLeftUtil.RoundTimeLeft > VoteStartTimingTime.Value;
            
            case McsMapExtendType.Rounds:
                return () => _timeLeftUtil.RoundsLeft > VoteStartTimingRound.Value;
            
            default:
                throw new InvalidOperationException($"This extend type {_timeLeftUtil.ExtendType} is not supported");
        }
    }

    // Timelimit, Round time limit, Round Limit
    private void ExecuteMapLimitConfig()
    {
        if (CurrentMap == null)
        {
            Logger.LogError("Failed to find current map config, we cannot execute map time/round limit config.");
            return;
        }

        var cvarName = _timeLeftUtil.ExtendType switch
        {
            McsMapExtendType.TimeLimit => "mp_timelimit",
            McsMapExtendType.RoundTime => "mp_roundtime",
            McsMapExtendType.Rounds => "mp_maxrounds",
            _ => throw new InvalidOperationException($"This extend type {_timeLeftUtil.ExtendType} is not supported")
        };

        var cvar = ConVar.Find(cvarName);
        if (cvar == null)
        {
            Logger.LogError($"Failed to find ConVar: {cvarName}");
            return;
        }

        switch (_timeLeftUtil.ExtendType)
        {
            case McsMapExtendType.TimeLimit:
            case McsMapExtendType.RoundTime:
                cvar.SetValue((float)CurrentMap.MapTime);
                break;
            case McsMapExtendType.Rounds:
                cvar.SetValue(CurrentMap.MapRounds);
                break;
        }
    }
    
    // tuna: I know same method exists in vote controller, but I don't have mutch time to refactor so I simply copy paste it.
    // TODO() Needs refactor
    private IMapConfig PickRandomMapForNextMap()
    {
        // This method will not use Linq to make debug logging easier
        var shuffledMaps = _mcsInternalMapConfigProviderApi.GetMapConfigs().Values.ToList()
            .OrderBy(_ => Random.Shared.Next()).ToList();

        var disabledMaps = shuffledMaps.Where(map => !map.IsDisabled).ToList();
        DebugLogger.LogTrace($"[Filter | Disabled Maps] {disabledMaps.Count} maps found.");
        
        var cooldownEndedMaps = disabledMaps.Where(map => map.MapCooldown.CurrentCooldown <= 0).ToList();
        DebugLogger.LogTrace($"[Filter | Map Cooldown] {cooldownEndedMaps.Count} maps found.");

        
        var alsoGroupCooldownEnded = cooldownEndedMaps.Where(map =>
            !map.GroupSettings.Any() ||
            map.GroupSettings.Count(setting => setting.GroupCooldown.CurrentCooldown > 0) == 0).ToList();
        DebugLogger.LogTrace($"[Filter | Gorup Cooldown] {cooldownEndedMaps.Count} maps found.");
        

        var notRestrectedToNominationOnly = alsoGroupCooldownEnded.Where(map => !map.OnlyNomination).ToList();
        DebugLogger.LogTrace($"[Filter | No Nomination Restriction] {notRestrectedToNominationOnly.Count} maps found.");
        

        var notRestrictedToCertainUsers = notRestrectedToNominationOnly.Where(map => !map.NominationConfig.RestrictToAllowedUsersOnly).ToList();
        DebugLogger.LogTrace($"[Filter | Not Restricted Certain users] {notRestrictedToCertainUsers.Count} maps found.");
        

        var greaterThanMinPlayers = notRestrictedToCertainUsers.Where(map => map.NominationConfig.MinPlayers == 0 || map.NominationConfig.MinPlayers <= Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false })).ToList();
        DebugLogger.LogTrace($"[Filter | Greater Than Min Players] {greaterThanMinPlayers.Count} maps found.");
        

        var lowerThanMaxPlayers = greaterThanMinPlayers.Where(map => map.NominationConfig.MaxPlayers == 0 || map.NominationConfig.MaxPlayers >= Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false })).ToList();
        DebugLogger.LogTrace($"[Filter | Lower Than Max Players] {lowerThanMaxPlayers.Count} maps found.");
        

        var notRequiresPermission = lowerThanMaxPlayers.Where(map => !map.NominationConfig.RequiredPermissions.Any()).ToList();
        DebugLogger.LogTrace($"[Filter | Not Requires Permission] {notRequiresPermission.Count} maps found.");
        

        var withinAllowedDays = notRequiresPermission.Where(map => !map.NominationConfig.DaysAllowed.Any() || map.NominationConfig.DaysAllowed.Contains(DateTime.Today.DayOfWeek)).ToList();
        DebugLogger.LogTrace($"[Filter | Within Allowed Days] {withinAllowedDays.Count} maps found.");
        

        var whithinAllowedTimeRange = withinAllowedDays.Where(map => !map.NominationConfig.AllowedTimeRanges.Any() || map.NominationConfig.AllowedTimeRanges.Count(range => range.IsInRange(TimeOnly.FromDateTime(DateTime.Now))) >= 1).ToList();
        DebugLogger.LogTrace($"[Filter | Within Allowed Time Range] {whithinAllowedTimeRange.Count} maps found.");
        

        var withoutCurrentMap = whithinAllowedTimeRange.Where(map => !map.MapName.Equals(CurrentMap?.MapName)).ToList();
        DebugLogger.LogTrace($"[Filter | Without Current Map] {withoutCurrentMap.Count} maps found.");
        

        var pickedMap = withoutCurrentMap.Take(1).First();
        DebugLogger.LogTrace($"[Filter | Finally] {pickedMap.MapName} has been picked for nextmap");
        
        return pickedMap;
    }

    internal void InitiateRtvVote()
    {
        if (_mcsMapVoteController.CurrentVoteState == McsMapVoteState.NextMapConfirmed)
            return;
        
        _voteStartTimer?.Kill();
        _voteStartTimer = null;
        _mcsMapVoteController.InitiateVote();
    }

    private void InitiateVote()
    {
        _voteStartTimer?.Kill();
        _voteStartTimer = null;
        _mcsMapVoteController.InitiateVote();
    }
    
    private void FireNextMapChangedEvent(IMapConfig newConfig)
    {
        var confirmedEvent = new McsNextMapChangedEvent(GetTextWithPluginPrefix(null, ""), newConfig);
        _mcsEventManager.FireEventNoResult(confirmedEvent);
    }
    
    private void FireNextMapRemovedEvent(IMapConfig newConfig)
    {
        var nextMapRemovedEvent = new McsNextMapRemovedEvent(GetTextWithPluginPrefix(null, ""), newConfig);
        _mcsEventManager.FireEventNoResult(nextMapRemovedEvent);
    }
}