using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.API.Events.RockTheVote;

/// <summary>
/// Event of player RTV cast, this event is cancellable
/// </summary>
public class McsPlayerRtvCastEvent(CCSPlayerController player, string modulePrefix): McsEventParam(modulePrefix), IMcsEventWithResult
{
    /// <summary>
    /// Player who cast rtv.
    /// </summary>
    public CCSPlayerController Player { get; } = player;
}