using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.Events.Nomination.MapNominatedEvent;
using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.Nomination.MapNominationBeginEvent;

/// <summary>
/// Event of nomination
/// </summary>
public class McsMapNominationBeginEvent(CCSPlayerController? player, IMapConfig mapConfig) : IMcsEvent
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