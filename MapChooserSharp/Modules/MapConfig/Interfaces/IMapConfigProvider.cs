using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapConfig.Interfaces;

public interface IMapConfigProvider
{
    public Dictionary<string, IMapConfig> GetMapConfigs();
    
    public IMapConfig? GetMapConfig(string mapName);
    
    public IMapConfig? GetMapConfig(long workshopId);
}