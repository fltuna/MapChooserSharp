using MapChooserSharp.Modules.RockTheVote;

namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsGeneralConfig
{
    internal bool ShouldUseAliasMapNameIfAvailable { get; }
    
    internal bool VerboseCooldownPrint { get; }
    
    internal string[] WorkshopCollectionIds { get; }
    
    internal bool ShouldAutoFixMapName { get; }
    
    internal IMcsSqlConfig SqlConfig { get; }
    
    internal RtvMapChangeBehaviourType RtvMapChangeBehaviour { get; }
}