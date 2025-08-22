using MapChooserSharp.Modules.MapCycle.Services;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public class McsMapCycleConfig(int defaultMaxExtends, int fallbackMaxExtCommandUses, int fallbackExtendTimePerExtends, int fallbackExtendRoundsPerExtends, bool shouldStopSourceTvRecording, McsMapConfigExecutionType mapConfigExecutionType, string mapConfigDirectoryPath, string groupConfigDirectoryPath) : IMcsMapCycleConfig
{
    public int FallbackDefaultMaxExtends { get; } = defaultMaxExtends;
    public int FallbackMaxExtCommandUses { get; } = fallbackMaxExtCommandUses;
    public bool ShouldStopSourceTvRecording { get; } = shouldStopSourceTvRecording;
    public McsMapConfigExecutionType MapConfigExecutionType { get; } = mapConfigExecutionType;
    public string MapConfigDirectoryPath { get; } = mapConfigDirectoryPath;
    public string GroupConfigDirectoryPath { get; } = groupConfigDirectoryPath;
    public int FallbackExtendTimePerExtends { get; } = fallbackExtendTimePerExtends;
    public int FallbackExtendRoundsPerExtends { get; } = fallbackExtendRoundsPerExtends;
}