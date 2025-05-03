namespace MapChooserSharp.API.MapConfig;

/// <summary>
/// Map's cooldown
/// </summary>
public interface IMapCooldown
{
    /// <summary>
    /// Map cooldown value specified in map config
    /// </summary>
    public int MapConfigCooldown { get; }
    
    /// <summary>
    /// Current cooldown from database
    /// </summary>
    public int CurrentCooldown { get; set; }
}