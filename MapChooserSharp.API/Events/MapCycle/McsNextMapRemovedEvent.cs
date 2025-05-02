using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.MapCycle;


public class McsNextMapRemovedEvent(string modulePrefix, IMapConfig previousNextMap): McsEventParam(modulePrefix), IMcsEventNoResult
{
    
    public IMapConfig PreviousNextMap { get; } = previousNextMap;
}