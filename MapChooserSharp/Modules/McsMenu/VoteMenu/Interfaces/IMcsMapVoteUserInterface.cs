using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

public interface IMcsMapVoteUserInterface
{
    public int VoteOptionCount { get; }
    
    public void OpenMenu();

    public void CloseMenu();

    public void SetVoteOptions(List<IMcsVoteOption> voteOptions);

    public void SetRandomShuffle(bool enableShuffle);
}