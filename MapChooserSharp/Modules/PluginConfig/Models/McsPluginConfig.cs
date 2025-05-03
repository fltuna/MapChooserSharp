using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

internal class McsPluginConfig(
    IMcsVoteConfig voteConfig,
    IMcsNominationConfig nominationConfig,
    IMcsMapCycleConfig mapCycleConfig)
    : IMcsPluginConfig
{
    public IMcsVoteConfig VoteConfig { get; } = voteConfig;
    public IMcsNominationConfig NominationConfig { get; } = nominationConfig;
    public IMcsMapCycleConfig MapCycleConfig { get; } = mapCycleConfig;
}