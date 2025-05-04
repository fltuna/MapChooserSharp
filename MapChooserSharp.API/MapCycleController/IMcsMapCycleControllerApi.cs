using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.MapCycleController;

/// <summary>
/// McsMapCycleController API, You can manipulate the map cycle like Change to next map, Set next map, with this API.
/// </summary>
public interface IMcsMapCycleControllerApi
{
    /// <summary>
    /// Next map of IMapConfig.
    /// null until MapChanged -> Vote finished and next map confirmed.
    /// </summary>
    public IMapConfig? NextMap { get; }
    
    /// <summary>
    /// Current map of IMapConfig.
    /// Shouldn't be null, but sometimes can be nullable.
    /// </summary>
    public IMapConfig? CurrentMap { get; }
    
    /// <summary>
    /// True if next map confirmed
    /// </summary>
    public bool IsNextMapConfirmed { get; }

    /// <summary>
    /// If true, map will be transit to next map when round end.
    /// </summary>
    public bool ChangeMapOnNextRoundEnd { get; set; }
    
    /// <summary>
    /// Extend count of current map
    /// </summary>
    public int ExtendCount { get; }
    
    /// <summary>
    /// Returns remaining extend count
    /// </summary>
    public int ExtendsLeft { get; }
    
    /// <summary>
    /// Set next map to specified map config
    /// </summary>
    /// <param name="mapConfig">Map Config</param>
    /// <returns>True if map is valid and next map successfully changed, otherwise false</returns>
    public bool SetNextMap(IMapConfig mapConfig);

    /// <summary>
    /// Set next map to specified map name.
    /// </summary>
    /// <param name="mapName">Map Name</param>
    /// <returns>True if map is found by name and next map successfully changed, otherwise false</returns>
    public bool SetNextMap(string mapName);
    
    /// <summary>
    /// Removes next map
    /// </summary>
    /// <returns></returns>
    public bool RemoveNextMap();


    /// <summary>
    /// Change to next map <br/>
    /// If next map is null, this method will fail and do nothing.
    /// </summary>
    /// <param name="seconds">Seconds to change map</param>
    public void ChangeToNextMap(float seconds);
}