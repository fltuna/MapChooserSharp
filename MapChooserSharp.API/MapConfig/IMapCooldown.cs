namespace MapChooserSharp.API.MapConfig;

public interface IMapCooldown
{
    // Map cooldown value specified in map config
    public int MapConfigCooldown { get; }
    
    // Current cooldown from database
    public int CurrentCooldown { get; set; }
}