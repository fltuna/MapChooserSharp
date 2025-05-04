using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapConfig.Interfaces;

public interface IMapConfigProvider
{
    public Dictionary<string, IMapGroupSettings> GetGroupSettings();
    
    public Dictionary<string, IMapConfig> GetMapConfigs();
    
    public IMapConfig? GetMapConfig(string mapName);
    
    public IMapConfig? GetMapConfig(long workshopId);

    /// <summary>
    /// Returns map name based on mapConfig
    /// </summary>
    /// <param name="mapConfig">Map Config</param>
    /// <returns>Alias name if ShouldUseAliasMapNameIfAvailable is true, otherwise actual map name</returns>
    public string GetMapName(IMapConfig mapConfig);
}