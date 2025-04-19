using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.RtvController;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.RockTheVote;

public class McsMcsRtvController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsRtvControllerApi
{
    public override string PluginModuleName => "McsRtvController";
    public override string ModuleChatPrefix => $" {ChatColors.Green}[RTV]{ChatColors.Default}";


    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }
}