using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

internal class McsVoteSound(
    string voteCountdownStartSound,
    string voteStartSound,
    string voteFinishSound,
    List<string> voteCountdownSounds)
    : IMcsVoteSound
{
    public string VoteCountdownStartSound { get; } = voteCountdownStartSound;
    public string VoteStartSound { get; } = voteStartSound;
    public string VoteFinishSound { get; } = voteFinishSound;
    public List<string> VoteCountdownSounds { get; } = voteCountdownSounds;
}