using MapChooserSharp.Models;

namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsVoteConfig
{
    internal List<McsSupportedMenuType> AvailableMenuTypes { get; }
    
    internal McsSupportedMenuType CurrentMenuType { get; }
    
    internal int MaxMenuElements { get; }
}