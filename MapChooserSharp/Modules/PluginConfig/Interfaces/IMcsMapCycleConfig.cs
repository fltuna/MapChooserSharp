namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsMapCycleConfig
{
    internal int FallbackDefaultMaxExtends { get; }
    
    internal int FallbackExtendTimePerExtends { get; }
    
    internal int FallbackExtendRoundsPerExtends { get; }
    
    internal int FallbackMaxExtCommandUses { get; }
    
    internal bool ShouldStopSourceTvRecording { get; }
}