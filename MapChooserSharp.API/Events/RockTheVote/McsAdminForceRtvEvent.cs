using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.API.Events.RockTheVote;

/// <summary>
/// Event of admins force RTV cast, this event is cancellable
/// </summary>
public class McsAdminForceRtvEvent(CCSPlayerController? player, string modulePrefix): McsEventParam(modulePrefix), IMcsEventWithResult
{
    /// <summary>
    /// Player who cast rtv. if executor is console, then param is null
    /// </summary>
    public CCSPlayerController? Player { get; } = player;
}