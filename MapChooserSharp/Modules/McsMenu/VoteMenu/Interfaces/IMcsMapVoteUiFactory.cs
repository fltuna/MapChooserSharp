using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

public interface IMcsMapVoteUiFactory
{
    int MaxMenuElements { get; }
    
    IMcsMapVoteUserInterface Create(CCSPlayerController player);
}