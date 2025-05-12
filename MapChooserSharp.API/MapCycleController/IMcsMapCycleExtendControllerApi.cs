namespace MapChooserSharp.API.MapCycleController;

/// <summary>
/// Map extending management
/// </summary>
public interface IMcsMapCycleExtendControllerApi
{
    /// <summary>
    /// Extends a current map.
    /// </summary>
    /// <param name="extendTime">extends a round, timelimit and roundtime. It depends on a map type</param>
    /// <returns>McsMapCycleExtendResult.Extended if map is extended successfully</returns>
    public McsMapCycleExtendResult ExtendCurrentMap(int extendTime);
}