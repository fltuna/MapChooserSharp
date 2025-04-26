using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.Nomination.Interfaces;

namespace MapChooserSharp.API.Events.Nomination;

/// <summary>
/// Event of nomination
/// </summary>
public class McsNominationBeginEvent(CCSPlayerController? player, IMcsNominationData mapConfig, string modulePrefix) : McsNominationEventBase(modulePrefix), IMcsEventWithResult
{
    /// <summary>
    /// Player who nominated map. if nominator is console, then param is null
    /// </summary>
    public override CCSPlayerController? Player { get; } = player;

    /// <summary>
    /// Nomination Data
    /// </summary>
    public override IMcsNominationData NominationData { get; } = mapConfig;
}