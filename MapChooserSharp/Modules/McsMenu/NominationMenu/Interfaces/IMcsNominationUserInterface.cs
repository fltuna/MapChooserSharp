using MapChooserSharp.Modules.McsMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

public interface IMcsNominationUserInterface
{
    public int NominationOptionCount { get; }
    
    public void OpenMenu();

    public void CloseMenu();

    public void SetNominationOption(List<IMcsNominationMenuOption> mcsNominationMenuOptions);
    
    public void SetMenuOption(IMcsGeneralMenuOption generalMenuOption);
}