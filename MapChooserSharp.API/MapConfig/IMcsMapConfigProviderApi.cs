namespace MapChooserSharp.API.MapConfig;

/// <summary>
/// 
/// </summary>
public interface IMcsMapConfigProviderApi
{
    /// <summary>
    /// Returns all group config data that loaded in this server
    /// </summary>
    /// <returns>Group config data</returns>
    public IReadOnlyDictionary<string, IMapGroupSettings> GetGroupSettings();
    
    /// <summary>
    /// Returns all map configs that loaded in this server
    /// </summary>
    /// <returns>Map configs that loaded in this server</returns>
    public IReadOnlyDictionary<string, IMapConfig> GetMapConfigs();
    
    /// <summary>
    /// Search map config by map name
    /// </summary>
    /// <param name="mapName">Map name defined in config (e.g. ze_example_v1)</param>
    /// <returns>MapConfig if found, otherwise null</returns>
    public IMapConfig? GetMapConfig(string mapName);
    
    
    /// <summary>
    /// Search map config by workshop ID
    /// </summary>
    /// <param name="workshopId">Map workshop ID defined in config</param>
    /// <returns>MapConfig if ID is not under 0 and map found, otherwise null</returns>
    public IMapConfig? GetMapConfig(long workshopId);
}