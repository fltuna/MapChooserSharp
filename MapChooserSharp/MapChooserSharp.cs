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
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.MapVote.Countdown;
using MapChooserSharp.Modules.Nomination;
using MapChooserSharp.Modules.PluginConfig;
using MapChooserSharp.Modules.RockTheVote;
using MapChooserSharp.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation;

namespace MapChooserSharp;

public sealed class MapChooserSharp: TncssPluginBase
{
    public override string ModuleName => "MapChooserSharp";
    public override string ModuleVersion => PluginConstants.PluginVersion.ToString();
    public override string ModuleAuthor => "faketuna A.K.A fltuna or tuna";
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
        
        // Plugin core modules
        RegisterModule<McsMapNominationController>();
        RegisterModule<McsMapNominationCommands>();
        
        RegisterModule<McsMapVoteController>();
        RegisterModule<McsMapVoteCommands>();
        RegisterModule<McsCountdownUiController>(hotReload);
        
        RegisterModule<McsRtvController>(hotReload);
        RegisterModule<McsRtvCommands>();
        
        RegisterModule<McsMapCycleController>(hotReload);
        RegisterModule<McsMapCycleCommands>();
        
        #if DEBUG
        RegisterModule<McsDebugCommands>();
        #endif
    }

    protected override void TncssLateOnPluginLoad(ServiceProvider provider)
    {
        RegisterMcsApi(provider);
    }

    private void RegisterMcsApi(ServiceProvider provider)
    {
        var nominationApi = provider.GetRequiredService<McsMapNominationController>();
        var mapVoteApi = provider.GetRequiredService<McsMapVoteController>();
        var rtvApi = provider.GetRequiredService<McsRtvController>();
        var eventManager = provider.GetRequiredService<IMcsInternalEventManager>();
        var mcsMapCycle = provider.GetRequiredService<McsMapCycleController>();
        
        
        var mcsApi = new McsApi(eventManager, mcsMapCycle, nominationApi, mapVoteApi, rtvApi);
        
        Capabilities.RegisterPluginCapability(IMapChooserSharpApi.Capability, () => mcsApi);
    }
}
