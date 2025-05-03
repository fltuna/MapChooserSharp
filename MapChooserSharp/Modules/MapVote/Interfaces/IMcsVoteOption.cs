using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.Modules.MapVote.Interfaces;

public interface IMcsVoteOption
{
    public string OptionText { get; }
    public Action<CCSPlayerController, byte> VoteCallback { get; }
}