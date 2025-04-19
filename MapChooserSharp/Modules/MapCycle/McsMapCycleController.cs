using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities;
using MapChooserSharp.API.Events.MapVote;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;
using TNCSSPluginFoundation.Utils.Other;

namespace MapChooserSharp.Modules.MapCycle;

public sealed class McsMapCycleController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsMapCycleControllerApi
{
    public override string PluginModuleName => "McsMapCycleController";
    public override string ModuleChatPrefix => "McsMapCycleController";

    private McsEventManager _mcsEventManager = null!;
    private IMapConfigProvider _mapConfigProvider = null!;

    
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

    public FakeConVar<int> NextMapTransitionDelay = new("mcs_nextmap_transition_delay", "", 10, ConVarFlags.FCVAR_NONE,
        new RangeValidator<int>(0, 30));
    
    
    
    public int InvalidTimeLeft { get; } = -1;
    
    private ConVar? mp_timelimit = null;

    internal int TimeLeft
    {
        get
        {
            var gameRules = EntityUtil.GetGameRules();
            if (gameRules == null)
            {
                Logger.LogError("Failed to find the Game Rules entity!");
                return InvalidTimeLeft;
            }

            if (mp_timelimit == null)
            {
                mp_timelimit = ConVar.Find("mp_timelimit");
                if (mp_timelimit == null)
                {
                    Logger.LogWarning("Failed to find the mp_timelimit ConVar and try to find again.");
                    return InvalidTimeLeft;
                }
            }

            var timeLimit = mp_timelimit.GetPrimitiveValue<float>();
            if (timeLimit < 0.001f)
            {
                return 0;
            }

            return (int)((gameRules.GameStartTime + timeLimit * 60.0f) - Server.CurrentTime);
        }
    }

    
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnInitialize()
    {
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsEventManager = ServiceProvider.GetRequiredService<McsEventManager>();
        
        _mcsEventManager.RegisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
    }

    protected override void OnUnloadModule()
    {
        _mcsEventManager.UnregisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        
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
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {

        return HookResult.Continue;
    }


    private void OnNextMapConfirmed(McsNextMapConfirmedEvent @event)
    {
        NextMap = @event.MapConfig;
    }
    

    internal string GetFormattedTimeLeft(int timeLeft)
    {
        int hours = timeLeft / 3600;
        int minutes = (timeLeft % 3600) / 60;
        int seconds = timeLeft % 60;

        if (hours > 0)
        {
            return $"{hours} {(hours == 1 ? "hour" : "hours")} " +
                   $"{minutes} {(minutes == 1 ? "minute" : "minutes")} " +
                   $"{seconds} {(seconds == 1 ? "second" : "seconds")}";
        }

        if (minutes > 0)
        {
            return $"{minutes} {(minutes == 1 ? "minute" : "minutes")} " +
                   $"{seconds} {(seconds == 1 ? "second" : "seconds")}";
        }

        return $"{seconds} {(seconds == 1 ? "second" : "seconds")}";
    }


    internal string GetFormattedTimeLeft(int timeLeft, CCSPlayerController player)
    {
        SteamID? steamId = player.AuthorizedSteamID;
        if (steamId == null)
            return GetFormattedTimeLeft(timeLeft);
        
        int hours = timeLeft / 3600;
        int minutes = (timeLeft % 3600) / 60;
        int seconds = timeLeft % 60;

        var playerCulture = PlayerLanguageManager.Instance.GetLanguage(steamId);
        using var tempCulture = new WithTemporaryCulture(playerCulture);

        if (hours > 0)
        {
            return "TODO_TRANSLATE| HOURS";
        }

        if (minutes > 0)
        {
            return "TODO_TRANSLATE| MINUETS";
        }

        return "TODO_TRANSLATE| SECONDS";
    }
}