namespace MapChooserSharp.API.MapConfig;

/// <summary>
/// Represents a time range with a start and end time
/// </summary>
public interface ITimeRange
{
    /// <summary>
    /// Start time of the range
    /// </summary>
    TimeOnly StartTime { get; }
    
    /// <summary>
    /// End time of the range
    /// </summary>
    TimeOnly EndTime { get; }
    
    /// <summary>
    /// Checks if the specified time is within this time range
    /// </summary>
    /// <param name="time">The time to check</param>
    /// <returns>True if the time is within the range or not specified in config (default is 00:00 - 00:00), otherwise false</returns>
    bool IsInRange(TimeOnly time);
}