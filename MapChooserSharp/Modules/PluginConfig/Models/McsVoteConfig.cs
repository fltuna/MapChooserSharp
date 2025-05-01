using MapChooserSharp.Models;
using MapChooserSharp.Modules.McsMenu;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public class McsVoteConfig(List<McsSupportedMenuType> availableVoteMenuTypes, McsSupportedMenuType currentMcsVoteMenuType, int maxMenuElements)
    : IMcsVoteConfig
{
    public List<McsSupportedMenuType> AvailableMenuTypes { get; } = availableVoteMenuTypes;
    public McsSupportedMenuType CurrentMenuType { get; } = currentMcsVoteMenuType;
    public int MaxMenuElements { get; } = maxMenuElements;
}