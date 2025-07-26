using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public class McsGeneralConfig(bool shouldUseAliasMapNameIfAvailable, bool verboseCooldownPrint, string[] workshopCollectionIds, bool shouldAutoFixMapName, IMcsSqlConfig sqlConfig) : IMcsGeneralConfig
{
    public bool ShouldUseAliasMapNameIfAvailable { get; } = shouldUseAliasMapNameIfAvailable;
    public bool VerboseCooldownPrint { get; } = verboseCooldownPrint;
    public string[] WorkshopCollectionIds { get; } = workshopCollectionIds;
    public bool ShouldAutoFixMapName { get; } = shouldAutoFixMapName;
    public IMcsSqlConfig SqlConfig { get; } = sqlConfig;
}