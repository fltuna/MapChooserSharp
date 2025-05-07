namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsVoteSound
{
    /// <summary>
    /// Sound name will be played when vote coundown starting
    /// </summary>
    internal string VoteCountdownStartSound { get; }
    
    /// <summary>
    /// Sound name will be played when vote starting
    /// </summary>
    internal string VoteStartSound { get; }
    
    /// <summary>
    /// Sound name will be played when vote finishing
    /// </summary>
    internal string VoteFinishSound { get; }
    
    /// <summary>
    /// Mapped to seconds <br/>
    /// Element 0 means 1 seconds <br/>
    /// Element 9 means 10 seconds
    /// </summary>
    internal List<string> VoteCountdownSounds { get; }
}