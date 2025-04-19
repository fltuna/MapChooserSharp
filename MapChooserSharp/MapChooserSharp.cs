using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.API.RtvController;
using MapChooserSharp.Models;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig;
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.Nomination;
using MapChooserSharp.Modules.RockTheVote;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;

namespace MapChooserSharp;

public sealed class MapChooserSharp: TncssPluginBase
{
    public override string ModuleName => "MapChooserSharp";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "faketuna A.K.A fltuna or tuna";
    public override string ModuleDescription => "Provides basic map cycle functionality.";

    public override string BaseCfgDirectoryPath => "unused";
    public override string ConVarConfigPath => Path.Combine(Server.GameDirectory, "csgo/cfg/MapChooserSharp/convars.cfg");
    protected override string PluginPrefix => $" {ChatColors.Green}[MCS]{ChatColors.Default}";


    protected override void RegisterRequiredPluginServices(IServiceCollection collection, IServiceProvider provider)
    {
        DebugLogger = new SimpleDebugLogger(provider);
    }

    protected override void TncssOnPluginLoad(bool hotReload)
    {
        // Plugin core dependencies
        RegisterModule<MapConfigRepository>();
        RegisterModule<McsEventManager>();
        
        // Plugin core modules
        RegisterModule<McsMapMcsNominationController>();
        
        RegisterModule<McsMcsMapVoteController>();
        
        RegisterModule<McsMcsRtvController>();
        
        RegisterModule<McsMapCycleController>();
    }

    protected override void TncssLateOnPluginLoad(ServiceProvider provider)
    {
        RegisterMcsApi(provider);
    }

    private void RegisterMcsApi(ServiceProvider provider)
    {
        var nominationApi = provider.GetRequiredService<McsMapMcsNominationController>();
        var mapVoteApi = provider.GetRequiredService<McsMcsMapVoteController>();
        var rtvApi = provider.GetRequiredService<McsMcsRtvController>();
        var eventManager = provider.GetRequiredService<McsEventManager>();
        var mcsMapCycle = provider.GetRequiredService<McsMapCycleController>();
        
        
        var mcsApi = new McsApi(eventManager, mcsMapCycle, nominationApi, mapVoteApi, rtvApi);
        
        Capabilities.RegisterPluginCapability(IMapChooserSharpApi.Capability, () => mcsApi);
    }
}
