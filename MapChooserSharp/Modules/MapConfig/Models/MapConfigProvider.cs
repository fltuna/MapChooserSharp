using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;

namespace MapChooserSharp.Modules.MapConfig.Models;

public class MapConfigProvider: IMapConfigProvider
{
    private readonly Dictionary<string, IMapConfig> _mapConfigs;
    private readonly Dictionary<string, IMapGroupSettings> _groupConfigs;
    
    public MapConfigProvider(Dictionary<string, IMapConfig> mapConfigs, Dictionary<string, IMapGroupSettings> groupConfigs)
    {
        this._mapConfigs = mapConfigs;
        this._groupConfigs = groupConfigs;


        _mapConfigs = mapConfigs
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary();
    }
    
    public Dictionary<string, IMapGroupSettings> GetGroupSettings()
    {
        return _groupConfigs;
    }

    public Dictionary<string, IMapConfig> GetMapConfigs()
    {
        return _mapConfigs;
    }

    public IMapConfig? GetMapConfig(string mapName)
    {
        foreach (var (key, value) in _mapConfigs)
        {
            if (key == mapName)
                return value;
        }
        
        return null;
    }

    public IMapConfig? GetMapConfig(long workshopId)
    {
        foreach (var (key, value) in _mapConfigs)
        {
            if (value.WorkshopId == workshopId)
                return value;
        }

        return null;
    }
}