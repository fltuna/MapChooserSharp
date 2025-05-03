using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.MapCycle;

/// <summary>
/// This event will be called when next map is removed
/// </summary>
/// <param name="modulePrefix">Module Prefix</param>
/// <param name="previousNextMap">Previous map that was set</param>
public class McsNextMapRemovedEvent(string modulePrefix, IMapConfig previousNextMap): McsEventParam(modulePrefix), IMcsEventNoResult
{
    
    /// <summary>
    /// The previous map that was set.
    /// </summary>
    public IMapConfig PreviousNextMap { get; } = previousNextMap;
}