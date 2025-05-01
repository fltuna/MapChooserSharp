using MapChooserSharp.Models;

namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsNominationConfig
{
    internal List<McsSupportedMenuType> AvailableMenuTypes { get; }
    
    internal McsSupportedMenuType CurrentMenuType { get; }
}