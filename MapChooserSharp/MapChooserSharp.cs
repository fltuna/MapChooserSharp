using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.API.RtvController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Models;
using MapChooserSharp.Modules.DebugCommands;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.MapVote.Countdown;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsDatabase;
using MapChooserSharp.Modules.McsMenu.NominationMenu;
using MapChooserSharp.Modules.McsMenu.VoteMenu;
using MapChooserSharp.Modules.Nomination;
using MapChooserSharp.Modules.Nomination.Interfaces;
using MapChooserSharp.Modules.PluginConfig;
using MapChooserSharp.Modules.RockTheVote;
using MapChooserSharp.Modules.RockTheVote.Interfaces;
using MapChooserSharp.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation;

namespace MapChooserSharp;

public sealed class MapChooserSharp: TncssPluginBase
{
    public override string ModuleName => "MapChooserSharp";
    public override string ModuleVersion => PluginConstants.PluginVersion.ToString();
    public override string ModuleAuthor => "faketuna A.K.A fltuna or tuna, Spitice, uru";
    public override string ModuleDescription => "Provides basic map cycle functionality.";

    public override string BaseCfgDirectoryPath => "unused";
    public override string ConVarConfigPath => Path.Combine(Server.GameDirectory, "csgo/cfg/MapChooserSharp/convars.cfg");
    
    
    public override string PluginPrefix => "Prefix.Plugin";
    public override bool UseTranslationKeyInPluginPrefix => true;


    protected override void RegisterRequiredPluginServices(IServiceCollection collection, IServiceProvider provider)
    {
        DebugLogger = new SimpleDebugLogger(provider);
    }

    protected override void TncssOnPluginLoad(bool hotReload)
    {
        if (hotReload)
        {
            for (int i = 0; i < 5; i++)
            {
                Logger.LogWarning("MapChooserSharp is hot reloaded! This will causes UNEXPECTED BEHAVIOUR. Developers is not responsible for any problems that occur by hot reloading. Especially API will not work when hot reloaded.");
            }
        }
        
        // Plugin core dependencies
        RegisterModule<McsPluginConfigRepository>();
        RegisterModule<MapConfigRepository>();
        RegisterModule<McsEventManager>();
        RegisterModule<TimeLeftUtil>(hotReload);
        
        RegisterModule<McsDatabaseProvider>();
        
        // Plugin core modules
        RegisterModule<McsMapNominationController>();
        RegisterModule<McsMapNominationCommands>();
        RegisterModule<McsNominationMenuProvider>(hotReload);
        
        RegisterModule<McsMapVoteController>();
        RegisterModule<McsMapVoteCommands>();
        RegisterModule<McsCountdownUiController>(hotReload);
        RegisterModule<McsMapVoteMenuProvider>(hotReload);
        
        RegisterModule<McsRtvController>(hotReload);
        RegisterModule<McsRtvCommands>();
        
        RegisterModule<McsMapCycleController>(hotReload);
        RegisterModule<McsMapCycleCommands>();
        
        #if DEBUG
        RegisterModule<McsDebugCommands>();
        #endif
        
        RegisterListener<Listeners.OnMapEnd>(() =>
        {
            // Reset ConVar value to config value when map end
            ConVarConfigurationService.ExecuteConfigs();
        });
    }

    protected override void TncssLateOnPluginLoad(ServiceProvider provider)
    {
        RegisterMcsApi(provider);
    }

    private void RegisterMcsApi(ServiceProvider provider)
    {
        var nominationApi = provider.GetRequiredService<IMcsInternalNominationApi>();
        var mapVoteApi = provider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
        var rtvApi = provider.GetRequiredService<IMcsInternalRtvControllerApi>();
        var eventManager = provider.GetRequiredService<IMcsInternalEventManager>();
        var mcsMapCycle = provider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        var mcsMapConfig = provider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        
        
        var mcsApi = new McsApi(eventManager, mcsMapCycle, nominationApi, mapVoteApi, rtvApi, mcsMapConfig);
        
        Capabilities.RegisterPluginCapability(IMapChooserSharpApi.Capability, () => mcsApi);
    }
}
