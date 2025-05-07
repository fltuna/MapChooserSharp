using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.Cs2MenuManager.ScreenMenu;

public class McsCs2MenuManagerScreenMenuUiFactory(IServiceProvider provider) : IMcsMapVoteUiFactory
{
    public int MaxMenuElements { get; } = 7;
    
    public IMcsMapVoteUserInterface Create(CCSPlayerController player)
    {
        return new McsCs2MenuManagerScreenMenuUi(player, provider);
    }
}