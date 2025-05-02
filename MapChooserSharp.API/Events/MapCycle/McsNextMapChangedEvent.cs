using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.MapCycle;


public class McsNextMapChangedEvent(string modulePrefix, IMapConfig newNextMap): McsEventParam(modulePrefix), IMcsEventNoResult
{
    
    public IMapConfig NewNextMap { get; } = newNextMap;
}