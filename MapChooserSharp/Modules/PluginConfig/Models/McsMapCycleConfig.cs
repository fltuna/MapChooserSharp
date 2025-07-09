using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public class McsMapCycleConfig(int defaultMaxExtends, int fallbackMaxExtCommandUses, int fallbackExtendTimePerExtends, int fallbackExtendRoundsPerExtends, bool shouldStopSourceTvRecording) : IMcsMapCycleConfig
{
    public int FallbackDefaultMaxExtends { get; } = defaultMaxExtends;
    public int FallbackMaxExtCommandUses { get; } = fallbackMaxExtCommandUses;
    public bool ShouldStopSourceTvRecording { get; } = shouldStopSourceTvRecording;
    public int FallbackExtendTimePerExtends { get; } = fallbackExtendTimePerExtends;
    public int FallbackExtendRoundsPerExtends { get; } = fallbackExtendRoundsPerExtends;
}