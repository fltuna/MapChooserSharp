using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.Nomination.MapNominatedEvent;

/// <summary>
/// Event of nomination
/// </summary>
public class MapNominatedEvent(CCSPlayerController? player, IMapConfig mapConfig) : IMcsEvent
{
    /// <summary>
    /// Player who nominated map. if nominator is console, then param is null
    /// </summary>
    public CCSPlayerController? Player { get; } = player;

    /// <summary>
    /// Map config data
    /// </summary>
    public IMapConfig MapConfig { get; } = mapConfig;
}