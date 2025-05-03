using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public class McsMapCycleConfig(int defaultMaxExtends, int fallbackExtendTimePerExtends, int fallbackExtendRoundsPerExtends) : IMcsMapCycleConfig
{
    public int FallbackDefaultMaxExtends { get; } = defaultMaxExtends;
    public int FallbackExtendTimePerExtends { get; } = fallbackExtendTimePerExtends;
    public int FallbackExtendRoundsPerExtends { get; } = fallbackExtendRoundsPerExtends;
}