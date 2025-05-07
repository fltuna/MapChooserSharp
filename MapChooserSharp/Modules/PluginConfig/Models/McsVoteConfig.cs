using MapChooserSharp.Models;
using MapChooserSharp.Modules.MapVote.Countdown;
using MapChooserSharp.Modules.McsMenu;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

internal class McsVoteConfig(List<McsSupportedMenuType> availableVoteMenuTypes, McsSupportedMenuType currentMcsVoteMenuType, int maxMenuElements, bool shouldPrintVoteToChat, IMcsVoteSoundConfig voteSoundConfig, McsCountdownType currentCountdownType)
    : IMcsVoteConfig
{
    public List<McsSupportedMenuType> AvailableMenuTypes { get; } = availableVoteMenuTypes;
    public McsSupportedMenuType CurrentMenuType { get; } = currentMcsVoteMenuType;
    public McsCountdownType CurrentCountdownType { get; } = currentCountdownType;
    public int MaxMenuElements { get; } = maxMenuElements;
    public bool ShouldPrintVoteToChat { get; } = shouldPrintVoteToChat;
    public IMcsVoteSoundConfig VoteSoundConfig { get; } = voteSoundConfig;
}