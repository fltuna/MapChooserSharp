using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Models;

public class McsVoteOption(string optionText, Action<CCSPlayerController, byte> voteCallback) : IMcsVoteOption
{
    public string OptionText { get; } = optionText;
    public Action<CCSPlayerController, byte> VoteCallback { get; } = voteCallback;
}