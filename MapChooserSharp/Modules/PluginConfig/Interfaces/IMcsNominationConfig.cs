using MapChooserSharp.Models;
using MapChooserSharp.Modules.McsMenu;

namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsNominationConfig
{
    internal List<McsSupportedMenuType> AvailableMenuTypes { get; }
    
    internal McsSupportedMenuType CurrentMenuType { get; }
}