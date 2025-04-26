using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.MapVote;

/// <summary>
/// This event will be called when after RTV vote with "don't change map"
/// </summary>
public class McsNextMapConfirmedEvent(string modulePrefix, IMapConfig mapConfig): McsEventParam(modulePrefix), IMcsEventNoResult
{
    /// <summary>
    /// Map config data
    /// </summary>
    public IMapConfig MapConfig { get; } = mapConfig;
}