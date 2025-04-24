using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using MapChooserSharp.API.Events.MapVote;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapVote;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;
using TNCSSPluginFoundation.Utils.Other;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace MapChooserSharp.Modules.MapCycle;

internal sealed class McsMapCycleController(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider), IMcsMapCycleControllerApi
{
    public override string PluginModuleName => "McsMapCycleController";
    public override string ModuleChatPrefix => "McsMapCycleController";

    private IMcsInternalEventManager _mcsEventManager = null!;
    private IMapConfigProvider _mapConfigProvider = null!;
    private McsMapVoteController _mcsMapVoteController = null!;
    private ITimeLeftUtil _timeLeftUtil = null!;

    
    private IMapConfig? _nextMap = null;

    public IMapConfig? NextMap
    {
        get => _nextMap;
        private set => _nextMap = value;
    }

    private IMapConfig? _currentMap = null;

    public IMapConfig? CurrentMap
    {
        get
        {
            return _currentMap ??= _mapConfigProvider.GetMapConfig(Server.MapName);
        }
        private set => _currentMap = value;
    }
    
    public bool ChangeMapImmediately { get; set; }= false;

    private bool _isMapStarted = false;

    private Timer? _voteStartTimer = null;

    private const float VoteStartCheckInterval = 1.0F;
    
    private const float DefaultRoundRestartDelay = 7.0F;

    public FakeConVar<int> VoteStartTimingTime = new("mcs_vote_start_timing_time", "When should vote started if map is based on mp_timelimit or mp_roundtime? (minutes)", 3,
        ConVarFlags.FCVAR_NONE, new RangeValidator<int>(2, 15));
    
    public FakeConVar<int> VoteStartTimingRound = new("mcs_vote_start_timing_round", "When should vote started if map is based on mp_maxrounds? (rounds)", 2,
        ConVarFlags.FCVAR_NONE, new RangeValidator<int>(2, 15));
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnInitialize()
    {
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        
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
            _timeLeftUtil.ReDetermineExtendType();
            _isMapStarted = true;
            RecreateVoteTimer();
        }
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<McsMapVoteController>();
        
        _mcsEventManager.RegisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.RegisterEventHandler<McsMapExtendEvent>(OnMapExtended);
        _mcsEventManager.RegisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
    }

    protected override void OnUnloadModule()
    {
        _mcsEventManager.UnregisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.UnregisterEventHandler<McsMapExtendEvent>(OnMapExtended);
        _mcsEventManager.RegisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        
        Plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
    }

    public bool ChangeToNextMap()
    {
        if (NextMap == null)
        {
            Logger.LogError("Failed to change map: next map is null");
            return false;
        }

        long workshopId = NextMap.WorkshopId;

        if (workshopId == 0)
        {
            DebugLogger.LogInformation($"No workshop ID defined! We will try change level with {NextMap.MapName}");
            // Use MapUtil.ChangeMap(string) instead of MapUtil.ChangeToWorkshopMap(string)
            // Because, This IMapConfig is no guarantee official map or not.
            MapUtil.ChangeMap(NextMap.MapName);
            return true;
        }

        DebugLogger.LogInformation($"We will try to change map to {NextMap.MapName} with workshop ID: {workshopId}");
        MapUtil.ChangeToWorkshopMap(workshopId);
        return true;
    }


    private void OnMapStart(string mapName)
    {
        CurrentMap = NextMap;
        NextMap = null;

        // This is extra check for server startup
        if (CurrentMap == null)
        {
            CurrentMap = _mapConfigProvider.GetMapConfig(mapName);
            
            // If map name isn't match with config then find with workshop ID
            if (CurrentMap == null)
            {
                if (long.TryParse(ForceFullUpdate.GetWorkshopId(), out long workshopId))
                {
                    // Find map config my workshopId
                    // But if not find with this way, CurrentMap become null.
                    CurrentMap = _mapConfigProvider.GetMapConfig(workshopId);
                }
            }
        }

        // Wait for first people joined
        Plugin.AddTimer(0.0F, RecreateVoteTimer);
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (NextMap == null)
            return HookResult.Continue;

        McsMapExtendType extendType = _timeLeftUtil.ExtendType;

        if (extendType == McsMapExtendType.TimeLimit && _timeLeftUtil.TimeLimit > 0)
            return HookResult.Continue;

        if (extendType == McsMapExtendType.Rounds && _timeLeftUtil.RoundsLeft > 0)
            return HookResult.Continue;

        if (extendType == McsMapExtendType.RoundTime && _timeLeftUtil.RoundTimeLeft > 0)
            return HookResult.Continue;
        
        DebugLogger.LogDebug("Changing to next map!");
        if (ChangeMapImmediately)
        {
            ChangeToNextMap();
            return HookResult.Continue;
        }

        ConVar? mp_round_restart_delay = ConVar.Find("mp_round_restart_delay");

        float delay = mp_round_restart_delay?.GetPrimitiveValue<float>() ?? DefaultRoundRestartDelay;
        
        Plugin.AddTimer(delay-1, () =>
        {
            ChangeToNextMap();
        }, TimerFlags.STOP_ON_MAPCHANGE);
        
        return HookResult.Continue;
    }


    private void OnNextMapConfirmed(McsNextMapConfirmedEvent @event)
    {
        NextMap = @event.MapConfig;
    }

    private void OnMapExtended(McsMapExtendEvent @event)
    {
        if (@event.MapExtendType == McsMapExtendType.TimeLimit)
        {
            _timeLeftUtil.ExtendTimeLimit(@event.ExtendTime);
        }

        if (@event.MapExtendType == McsMapExtendType.Rounds)
        {
            _timeLeftUtil.ExtendRounds(@event.ExtendTime);
        }

        if (@event.MapExtendType == McsMapExtendType.RoundTime)
        {
            _timeLeftUtil.ExtendRoundTime(@event.ExtendTime);
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
                _voteStartTimer?.Kill();
                _voteStartTimer = null;
                _mcsMapVoteController.InitiateVote();
            }
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
    }
}