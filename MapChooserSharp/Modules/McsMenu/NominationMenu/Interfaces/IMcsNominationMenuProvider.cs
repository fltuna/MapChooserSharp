using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

public interface IMcsNominationMenuProvider
{
    public IMcsNominationUserInterface CreateNewNominationUi(CCSPlayerController player);
}