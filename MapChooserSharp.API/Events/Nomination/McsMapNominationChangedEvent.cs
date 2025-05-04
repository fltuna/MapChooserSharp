using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.Nomination;

namespace MapChooserSharp.API.Events.Nomination;

/// <summary>
/// Called when map nomination is changed
/// </summary>
public class McsMapNominationChangedEvent(CCSPlayerController player, IMcsNominationData nominationData, string modulePrefix) : McsNominationEventBase(modulePrefix), IMcsEventNoResult
{
    /// <summary>
    /// Player who changed the nomianated map
    /// </summary>
    public override CCSPlayerController Player { get; } = player;

    /// <summary>
    /// Nomination Data
    /// </summary>
    public override IMcsNominationData NominationData { get; } = nominationData;
}