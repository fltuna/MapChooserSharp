using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;

namespace MapChooserSharp.Modules.MapConfig.Models;

public class MapConfigProvider(Dictionary<string, IMapConfig> mapConfigs): IMapConfigProvider
{
    public Dictionary<string, IMapConfig> GetMapConfigs()
    {
        return mapConfigs;
    }

    public IMapConfig? GetMapConfig(string mapName)
    {
        foreach (var (key, value) in mapConfigs)
        {
            if (key == mapName)
                return value;
        }
        
        return null;
    }

    public IMapConfig? GetMapConfig(long workshopId)
    {
        foreach (var (key, value) in mapConfigs)
        {
            if (value.WorkshopId == workshopId)
                return value;
        }

        return null;
    }
}