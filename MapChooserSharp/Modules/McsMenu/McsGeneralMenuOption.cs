using MapChooserSharp.Modules.McsMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu;

public class McsGeneralMenuOption(string menuTitle, bool useTranslationKey) : IMcsGeneralMenuOption
{
    public string MenuTitle { get; } = menuTitle;
    
    public bool UseTranslationKey { get; } = useTranslationKey;
}