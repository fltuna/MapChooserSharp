namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsPluginConfig
{
    internal IMcsVoteConfig VoteConfig { get; }
    
    internal IMcsNominationConfig NominationConfig { get; }
    
    internal IMcsMapCycleConfig MapCycleConfig { get; }
}