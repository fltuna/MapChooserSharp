using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Cs2ScreenMenuApi;

public class McsCs2ScreenMenuApiNominationUiFactory(IServiceProvider provider): IMcsNominationUiFactory
{
    public IMcsNominationUserInterface Create(CCSPlayerController player)
    {
        return new McsCs2ScreenMenuApiNominationUi(player, provider);
    }
}