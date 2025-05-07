using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Cs2MenuManager.ScreenMenu;

public class McsCs2MenuManagerScreenMenuNominationUiFactory(IServiceProvider provider): IMcsNominationUiFactory
{
    public IMcsNominationUserInterface Create(CCSPlayerController player)
    {
        return new McsCs2MenuManagerScreenMenuNominationUi(player, provider);
    }
}