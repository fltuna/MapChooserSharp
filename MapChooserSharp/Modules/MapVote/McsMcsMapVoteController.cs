using MapChooserSharp.API.MapVoteController;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapVote;

public sealed class McsMcsMapVoteController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsMapVoteControllerApi
{
    public override string PluginModuleName => "McsMapVoteController";
    public override string ModuleChatPrefix => PluginModuleName;


    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }
}
