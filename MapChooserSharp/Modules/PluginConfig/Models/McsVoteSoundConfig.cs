using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

internal class McsVoteSoundConfig: IMcsVoteSoundConfig
{
    public McsVoteSoundConfig(string vSndEvtsSoundFilePath, IMcsVoteSound voteCountdownSounds, IMcsVoteSound runoffVoteCountdownSounds)
    {
        if (!vSndEvtsSoundFilePath.EndsWith(".vsndevts"))
        {
            VSndEvtsSoundFilePath = string.Empty;
        }
        else
        {
            VSndEvtsSoundFilePath = vSndEvtsSoundFilePath;
        }
        
        InitialVoteSounds = voteCountdownSounds;
        RunoffVoteSounds = runoffVoteCountdownSounds;
    }

    public string VSndEvtsSoundFilePath { get; }
    public IMcsVoteSound InitialVoteSounds { get; }
    public IMcsVoteSound RunoffVoteSounds { get; }
}