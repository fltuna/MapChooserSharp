using MapChooserSharp.Modules.McsDatabase;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public class McsGeneralConfig(bool shouldUseAliasMapNameIfAvailable, bool verboseCooldownPrint, IMcsSqlConfig sqlConfig) : IMcsGeneralConfig
{
    public bool ShouldUseAliasMapNameIfAvailable { get; } = shouldUseAliasMapNameIfAvailable;
    public bool VerboseCooldownPrint { get; } = verboseCooldownPrint;
    public IMcsSqlConfig SqlConfig { get; } = sqlConfig;
}