namespace MapChooserSharp.API.MapConfig;

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