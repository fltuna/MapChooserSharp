using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.Interfaces;

namespace MapChooserSharp.Modules.Nomination.Interfaces;

internal interface IMcsInternalNominationApi: IMcsInternalApiBase, IMcsNominationApi
{
    /// <summary>
    /// Internal nomination data collection
    /// </summary>
    internal Dictionary<string, IMcsNominationData> NominatedMaps { get; }

    /// <summary>
    /// Internal check to player can nominate map
    /// </summary>
    /// <param name="player">Player Instance</param>
    /// <param name="mapConfig">Map Config</param>
    /// <returns></returns>
    internal McsMapNominationController.NominationCheck PlayerCanNominateMap(CCSPlayerController player, IMapConfig mapConfig);
}