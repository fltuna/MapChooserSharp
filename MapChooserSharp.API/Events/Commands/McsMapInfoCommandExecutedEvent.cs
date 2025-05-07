using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.Commands;

/// <summary>
///  This event will be called when player executes a !mapinfo command. <br/>
/// You can extend !mapinfo command result by listening this event and printing additional information.
/// </summary>
/// <param name="modulePrefix">Module Prefix</param>
/// <param name="player">Player Controller</param>
/// <param name="mapConfig">Map config this player searched for</param>
public class McsMapInfoCommandExecutedEvent(string modulePrefix, CCSPlayerController player, IMapConfig mapConfig): McsEventParam(modulePrefix), IMcsEventNoResult
{
    /// <summary>
    /// Player who executed this command
    /// </summary>
    public readonly CCSPlayerController Player = player;
    
    /// <summary>
    /// Map config this player searched for.
    /// </summary>
    public readonly IMapConfig MapConfig = mapConfig;
}