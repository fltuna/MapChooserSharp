namespace MapChooserSharp.API.MapConfig;

/// <summary>
/// Map group's name and cooldown
/// </summary>
public interface IMapGroupSettings
{
    /// <summary>
    /// Group name
    /// </summary>
    public string GroupName { get; }
    
    /// <summary>
    /// Group cooldown
    /// </summary>
    public IMapCooldown GroupCooldown { get; }
}