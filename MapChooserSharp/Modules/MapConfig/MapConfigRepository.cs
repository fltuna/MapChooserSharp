using MapChooserSharp.Modules.MapConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapConfig;

internal sealed class MapConfigRepository(IServiceProvider serviceProvider): PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "MapConfigRepository";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private IMapConfigProvider _mapConfigProvider = null!;
    
    private string _mapConfigLocation = null!;
    
    
    protected override void OnInitialize()
    {
        ReloadMapConfiguration();
    }

    protected override void OnUnloadModule()
    {
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(_mapConfigProvider);
    }

    private void ReloadMapConfiguration()
    {
        _mapConfigLocation = Path.Combine(Plugin.ModuleDirectory, "config", "maps.toml");
        _mapConfigProvider = new MapConfigParser(_mapConfigLocation).Load();
    }
}