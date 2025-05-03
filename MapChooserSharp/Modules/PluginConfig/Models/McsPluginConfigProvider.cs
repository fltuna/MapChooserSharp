using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

internal class McsPluginConfigProvider(IMcsPluginConfig mcsPluginConfig): IMcsPluginConfigProvider
{
    public IMcsPluginConfig PluginConfig { get; } = mcsPluginConfig;
}