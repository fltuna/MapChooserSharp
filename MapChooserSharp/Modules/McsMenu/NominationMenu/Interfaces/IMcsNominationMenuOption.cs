using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

public interface IMcsNominationMenuOption
{
    public IMcsNominationOption NominationOption { get; }
    
    public bool MenuDisabled { get; }
    
    public Action<CCSPlayerController, IMcsNominationOption> SelectionCallback { get; }
}