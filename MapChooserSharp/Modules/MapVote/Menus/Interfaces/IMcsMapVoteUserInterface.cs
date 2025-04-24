using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.MapVote.Models;

namespace MapChooserSharp.Modules.MapVote.Menus.Interfaces;

public interface IMcsMapVoteUserInterface
{
    public int VoteOptionCount { get; }
    
    public void OpenMenu(CCSPlayerController player);

    public void CloseMenu(CCSPlayerController player);

    public void SetVoteOptions(List<IMcsVoteOption> voteOptions);

    public void SetRandomShuffle(bool enableShuffle);
}