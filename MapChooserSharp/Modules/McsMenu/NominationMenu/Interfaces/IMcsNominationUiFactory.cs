using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

public interface IMcsNominationUiFactory
{
    IMcsNominationUserInterface Create(CCSPlayerController player);
}