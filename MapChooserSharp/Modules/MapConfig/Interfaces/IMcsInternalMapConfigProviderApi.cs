using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapConfig.Interfaces;

public interface IMcsInternalMapConfigProviderApi: IMcsMapConfigProviderApi
{
    /// <summary>
    /// Returns map name based on mapConfig
    /// </summary>
    /// <param name="mapConfig">Map Config</param>
    /// <returns>Alias name if ShouldUseAliasMapNameIfAvailable is true, otherwise actual map name</returns>
    public string GetMapName(IMapConfig mapConfig);
}