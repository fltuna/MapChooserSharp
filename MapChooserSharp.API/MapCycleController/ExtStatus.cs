namespace MapChooserSharp.API.MapCycleController;

/// <summary>
/// Status of !ext command
/// </summary>
public enum ExtStatus
{
    /// <summary>
    /// If enabled and accepting !ext
    /// </summary>
    Enabled = 0,
    
    /// <summary>
    /// If disabled by admin
    /// </summary>
    Disabled,
    
    /// <summary>
    /// If !ext is in cooldown
    /// </summary>
    InCooldown,
    
    /// <summary>
    /// If !ext is reached its limit.
    /// </summary>
    ReachedLimit
}