using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.Nomination.Interfaces;

namespace MapChooserSharp.API.Events.Nomination;

/// <summary>
/// Event of nomination
/// </summary>
public class McsMapAdminNominatedEvent(CCSPlayerController? player, IMcsNominationData nominationData, string modulePrefix) : McsNominationEventBase(modulePrefix), IMcsEventNoResult
{
    /// <summary>
    /// Player who nominated map. if nominator is console, then param is null
    /// </summary>
    public override CCSPlayerController? Player { get; } = player;

    /// <summary>
    /// Nomination Data
    /// </summary>
    public override IMcsNominationData NominationData { get; } = nominationData;
}