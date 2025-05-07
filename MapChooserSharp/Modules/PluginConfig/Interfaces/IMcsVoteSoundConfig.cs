namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsVoteSoundConfig
{
    /// <summary>
    /// Vsndevts file path
    /// </summary>
    internal string VSndEvtsSoundFilePath { get; }
    
    /// <summary>
    /// Initial vote sounds
    /// </summary>
    internal IMcsVoteSound InitialVoteSounds { get; }
    
    
    /// <summary>
    /// Runoff vote sounds
    /// </summary>
    internal IMcsVoteSound RunoffVoteSounds { get; }
}