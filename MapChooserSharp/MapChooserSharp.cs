using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.Modules.MapConfig;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;

namespace MapChooserSharp;

public sealed class MapChooserSharp: TncssPluginBase
{
    public override string ModuleName => "MapChooserSharp";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "faketuna A.K.A fltuna or tuna";
    public override string ModuleDescription => "Provides basic map cycle functionality.";

    public override string BaseCfgDirectoryPath => ModuleDirectory;
    public override string ConVarConfigPath => Path.Combine(Server.GameDirectory, "csgo/cfg/MapChooserSharp/convars.cfg");
    protected override string PluginPrefix => $" {ChatColors.Green}[MCS]{ChatColors.Default}";


    protected override void RegisterRequiredPluginServices(IServiceCollection collection, IServiceProvider provider)
    {
    }

    protected override void TncssOnPluginLoad(bool hotReload)
    {
        RegisterModule<MapConfigRepository>();
    }
}
