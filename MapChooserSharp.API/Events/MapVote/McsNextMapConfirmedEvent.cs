using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.MapVote;

public class McsNextMapConfirmedEvent(string modulePrefix, IMapConfig mapConfig): McsEventParam(modulePrefix), IMcsEventNoResult
{
    public IMapConfig MapConfig { get; } = mapConfig;
}