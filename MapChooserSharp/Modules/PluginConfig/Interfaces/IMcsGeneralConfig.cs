using MapChooserSharp.Modules.McsDatabase;

namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsGeneralConfig
{
    internal bool ShouldUseAliasMapNameIfAvailable { get; }
    
    internal bool VerboseCooldownPrint { get; }
    
    internal IMcsSqlConfig SqlConfig { get; }
}