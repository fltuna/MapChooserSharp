using MapChooserSharp.Models;
using MapChooserSharp.Modules.McsMenu;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public class McsNominationConfig(
    List<McsSupportedMenuType> availableMenuTypes,
    McsSupportedMenuType currentMenuType)
    : IMcsNominationConfig
{
    public List<McsSupportedMenuType> AvailableMenuTypes { get; } = availableMenuTypes;
    public McsSupportedMenuType CurrentMenuType { get; } = currentMenuType;
}