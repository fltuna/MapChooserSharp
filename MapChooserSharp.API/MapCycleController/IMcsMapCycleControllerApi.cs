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
}