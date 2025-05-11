using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;

namespace MapChooserSharp.Modules.MapConfig;

public sealed class McsMapConfigProvider: IMcsInternalMapConfigProviderApi
{
    private readonly Dictionary<string, IMapConfig> _mapConfigs;
    private readonly Dictionary<string, IMapGroupSettings> _groupConfigs;
    
    public McsMapConfigProvider(Dictionary<string, IMapConfig> mapConfigs, Dictionary<string, IMapGroupSettings> groupConfigs)
    {
        _mapConfigs = mapConfigs
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        _groupConfigs = groupConfigs
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
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
        _mapConfigs.TryGetValue(mapName, out var mapConfig);
        return mapConfig;
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