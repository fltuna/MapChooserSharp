using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.PluginConfig;

internal sealed class McsPluginConfigRepository(IServiceProvider serviceProvider): PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "PluginConfigRepository";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private IMcsPluginConfigProvider _mcsPluginConfigProvider = null!;
    
    protected override void OnInitialize()
    {
        ReloadPluginConfiguration();
    }

    protected override void OnUnloadModule()
    {
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(_mcsPluginConfigProvider);
    }

    private void ReloadPluginConfiguration()
    {
        string mapConfigLocation = Path.Combine(Plugin.ModuleDirectory, "plugin.toml");
        _mcsPluginConfigProvider = new McsPluginConfigParser(mapConfigLocation, ServiceProvider).Load();
    }
}