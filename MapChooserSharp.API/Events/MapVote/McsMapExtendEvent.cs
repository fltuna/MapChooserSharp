using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;

namespace MapChooserSharp.API.Events.MapVote;

/// <summary>
/// This event will be called when map is extended
/// </summary>
/// <param name="modulePrefix">Module Prefix</param>
/// <param name="extendTime">How long the map will be extended in minutes</param>
/// <param name="mapExtendType">Extend type of this event</param>
public class McsMapExtendEvent(string modulePrefix, int extendTime, McsMapExtendType mapExtendType): McsEventParam(modulePrefix), IMcsEventNoResult
{
    /// <summary>
    /// How long the map will be extended in minutes
    /// </summary>
    public int ExtendTime { get; } = extendTime;
    
    /// <summary>
    /// What time type should be extended
    /// </summary>
    public McsMapExtendType MapExtendType { get; } = mapExtendType;
}