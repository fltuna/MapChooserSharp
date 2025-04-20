using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.Nomination;

/// <summary>
/// Event of nomination
/// </summary>
public class McsMapNominatedEvent(CCSPlayerController? player, IMapConfig mapConfig, string modulePrefix) : McsNominationEventBase(modulePrefix), IMcsEventNoResult
{
    /// <summary>
    /// Player who nominated map. if nominator is console, then param is null
    /// </summary>
    public override CCSPlayerController? Player { get; } = player;

    /// <summary>
    /// Map config data
    /// </summary>
    public override IMapConfig MapConfig { get; } = mapConfig;
}