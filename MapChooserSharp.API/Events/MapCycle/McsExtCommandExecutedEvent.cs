using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.MapCycle;

/// <summary>
/// This event will be called when next map is changed
/// </summary>
/// <param name="modulePrefix">Module Prefix</param>
/// <param name="player">Player who executed</param>
public class McsExtCommandExecutedEvent(string modulePrefix, CCSPlayerController player): McsEventParam(modulePrefix), IMcsEventWithResult
{
    /// <summary>
    /// Player who executed
    /// </summary>
    public CCSPlayerController Player = player;
}