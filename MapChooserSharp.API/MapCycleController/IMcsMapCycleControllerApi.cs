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
}