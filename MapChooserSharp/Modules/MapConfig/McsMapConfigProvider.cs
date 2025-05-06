using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;

namespace MapChooserSharp.Modules.MapConfig;

public sealed class McsMapConfigProvider: IMcsInternalMapConfigProviderApi
{
    private readonly Dictionary<string, IMapConfig> _mapConfigs;
    private readonly Dictionary<string, IMapGroupSettings> _groupConfigs;
    
    public McsMapConfigProvider(Dictionary<string, IMapConfig> mapConfigs, Dictionary<string, IMapGroupSettings> groupConfigs)
    {
        _mapConfigs = mapConfigs;
        _groupConfigs = groupConfigs;


        _mapConfigs = mapConfigs
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary();
    }
    
    public IReadOnlyDictionary<string, IMapGroupSettings> GetGroupSettings()
    {
        return _groupConfigs;
    }

    public IReadOnlyDictionary<string, IMapConfig> GetMapConfigs()
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
        if (workshopId <= 0)
            return null;
        
        foreach (var (key, value) in _mapConfigs)
        {
            if (value.WorkshopId == workshopId)
                return value;
        }

        return null;
    }

    public string GetMapName(IMapConfig mapConfig)
    {
        if (mapConfig.MapNameAlias != string.Empty)
            return mapConfig.MapNameAlias;
        
        return mapConfig.MapName;
    }
}