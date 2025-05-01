using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

public interface IMcsMapVoteMenuProvider
{
    public IMcsMapVoteUserInterface CreateNewVoteUi(CCSPlayerController player);
}