using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Timers;
using MapChooserSharp.API.Events.MapCycle;
using MapChooserSharp.API.Events.MapVote;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.McsDatabase.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using MapChooserSharp.Modules.RockTheVote;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Other;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace MapChooserSharp.Modules.MapCycle;

internal sealed class McsMapCycleController(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider), IMcsMapCycleControllerApi
{
    public override string PluginModuleName => "McsMapCycleController";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private IMcsInternalEventManager _mcsEventManager = null!;
    private IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = null!;
    private IMcsPluginConfigProvider _mcsPluginConfigProvider = null!;
    private McsMapVoteController _mcsMapVoteController = null!;
    private McsRtvController _mcsRtvController = null!;
    private IMcsDatabaseProvider _mcsDatabaseProvider = null!;
    private ITimeLeftUtil _timeLeftUtil = null!;

    
    private IMapConfig? _nextMap = null;

    public IMapConfig? NextMap
    {
        get => _nextMap;
        private set => _nextMap = value;
    }
    
    public bool IsNextMapConfirmed => _nextMap != null;

    internal bool ChangeMapOnNextRoundEnd { get; set; } = false;

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

    public readonly FakeConVar<int> VoteStartTimingTime = new("mcs_vote_start_timing_time", "When should vote started if map is based on mp_timelimit or mp_roundtime? (minutes)", 3,
        ConVarFlags.FCVAR_NONE, new RangeValidator<int>(2, 15));
    
    public readonly FakeConVar<int> VoteStartTimingRound = new("mcs_vote_start_timing_round", "When should vote started if map is based on mp_maxrounds? (rounds)", 2,
        ConVarFlags.FCVAR_NONE, new RangeValidator<int>(2, 15));
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<McsMapVoteController>();
        _mcsRtvController = ServiceProvider.GetRequiredService<McsRtvController>();
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _mcsPluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        _mcsDatabaseProvider = ServiceProvider.GetRequiredService<IMcsDatabaseProvider>();
        
        _mcsEventManager.RegisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.RegisterEventHandler<McsMapExtendEvent>(OnMapExtended);
        _mcsEventManager.RegisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.RegisterListener<Listeners.OnMapEnd>(() =>
        {
            _isMapStarted = false;
            ChangeMapOnNextRoundEnd = false;
        });
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        
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
        }
    }

    protected override void OnUnloadModule()
    {
        _mcsEventManager.UnregisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.UnregisterEventHandler<McsMapExtendEvent>(OnMapExtended);
        _mcsEventManager.UnregisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        
        Plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
    }


    public void ChangeToNextMap(float seconds)
    {
        if (seconds <= 0)
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


    private void OnMapStart(string mapName)
    {
        ExtendCount = 0;

        var previousMap = CurrentMap;
        
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

        // Set previous map cooldown if defined in config
        if (previousMap != null)
        {
            _mcsDatabaseProvider.MapInfoRepository.UpsertMapCooldownAsync(previousMap.MapName, previousMap.MapCooldown.MapConfigCooldown).ConfigureAwait(false);
            previousMap.MapCooldown.CurrentCooldown = previousMap.MapCooldown.MapConfigCooldown;
            
            foreach (IMapGroupSettings setting in previousMap.GroupSettings)
            {
                _mcsDatabaseProvider.GroupInfoRepository.UpsertGroupCooldownAsync(setting.GroupName, setting.GroupCooldown.MapConfigCooldown).ConfigureAwait(false);
                setting.GroupCooldown.CurrentCooldown = setting.GroupCooldown.MapConfigCooldown;
            }
        }
        
        
        CurrentMap = NextMap;
        NextMap = null;
        

        // Wait for first people joined
        Plugin.AddTimer(0.0F, () =>
        {
            ObtainCurrentMap(mapName);
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

        McsMapExtendType extendType = _timeLeftUtil.ExtendType;

        if (extendType == McsMapExtendType.TimeLimit && _timeLeftUtil.TimeLimit > 0)
            return HookResult.Continue;

        if (extendType == McsMapExtendType.Rounds && _timeLeftUtil.RoundsLeft > 0)
            return HookResult.Continue;

        if (extendType == McsMapExtendType.RoundTime && _timeLeftUtil.RoundTimeLeft > 0)
            return HookResult.Continue;
        

        ConVar? mp_round_restart_delay = ConVar.Find("mp_round_restart_delay");

        float delay = mp_round_restart_delay?.GetPrimitiveValue<float>() ?? DefaultRoundRestartDelay;
        
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

        switch (_timeLeftUtil.ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                CreateVoteStartTimer(() => _timeLeftUtil.TimeLimit / 60 > VoteStartTimingTime.Value) ;
                break;
            
            case McsMapExtendType.RoundTime:
                CreateVoteStartTimer(() =>
                {
                    Server.PrintToChatAll($"{_timeLeftUtil.RoundTimeLeft} > {VoteStartTimingTime.Value}");
                    return _timeLeftUtil.RoundTimeLeft / 60 > VoteStartTimingTime.Value;
                });
                break;
            
            case McsMapExtendType.Rounds:
                CreateVoteStartTimer(() => _timeLeftUtil.RoundsLeft > VoteStartTimingRound.Value);
                break;
        }
    }
    
    private void CreateVoteStartTimer(Func<bool> shouldContinueCheck)
    {
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
        var confirmedEvent = new McsNextMapChangedEvent(GetTextWithPluginPrefix(""), newConfig);
        _mcsEventManager.FireEventNoResult(confirmedEvent);
    }
    
    private void FireNextMapRemovedEvent(IMapConfig newConfig)
    {
        var nextMapRemovedEvent = new McsNextMapRemovedEvent(GetTextWithPluginPrefix(""), newConfig);
        _mcsEventManager.FireEventNoResult(nextMapRemovedEvent);
    }
}