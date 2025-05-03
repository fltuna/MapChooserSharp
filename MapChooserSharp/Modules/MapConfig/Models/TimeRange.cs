using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapConfig.Models;

public class TimeRange(
    TimeOnly startTime,
    TimeOnly endTime
    )
    : ITimeRange
{
    
    
    public TimeOnly StartTime { get; } = startTime;
    public TimeOnly EndTime { get; } = endTime;
    
    

    public bool IsInRange(TimeOnly time)
    {
        // If these values are not defined in config, will initialize to 00:00, so return true.
        if (StartTime.Equals(EndTime))
            return true;
        
        
        if (StartTime <= EndTime)
        {
            return time >= StartTime && time <= EndTime;
        }
        

        // If time is over a day (e.g. 22:00-6:00)
        return time >= StartTime || time <= EndTime;
    }

    public override string ToString()
    {
        return StartTime + " - " + EndTime;
    }
}