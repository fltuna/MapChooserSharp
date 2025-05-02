using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.MapCycle;

/// <summary>
/// This event will be called when next map is changed
/// </summary>
/// <param name="modulePrefix">Module Prefix</param>
/// <param name="newNextMap">New next map that was set</param>
public class McsNextMapChangedEvent(string modulePrefix, IMapConfig newNextMap): McsEventParam(modulePrefix), IMcsEventNoResult
{
    /// <summary>
    /// The new next map that was set.
    /// </summary>
    public IMapConfig NewNextMap { get; } = newNextMap;
}