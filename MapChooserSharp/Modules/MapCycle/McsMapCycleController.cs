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
}