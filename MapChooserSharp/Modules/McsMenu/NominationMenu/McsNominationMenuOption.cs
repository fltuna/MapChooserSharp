using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu;

public sealed class McsNominationMenuOption(
    IMcsNominationOption nominationOption,
    Action<CCSPlayerController, IMcsNominationOption> selectionCallback,
    bool menuDisabled = true)
    : IMcsNominationMenuOption
{
    public IMcsNominationOption NominationOption { get; } = nominationOption;
    public bool MenuDisabled { get; } = menuDisabled;
    public Action<CCSPlayerController, IMcsNominationOption> SelectionCallback { get; } = selectionCallback;
}