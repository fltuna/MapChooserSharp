using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapVote;

internal sealed class McsMapVoteController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsMapVoteControllerApi
{
    public override string PluginModuleName => "McsMapVoteController";
    public override string ModuleChatPrefix => PluginModuleName;
    
    
    private IMcsInternalEventManager _mcsEventManager = null!;
    private IMapConfigProvider _mapConfigProvider = null!;
    
    

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }


    protected override void OnInitialize()
    {
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
    }


    protected override void OnUnloadModule()
    {
        
    }
}
