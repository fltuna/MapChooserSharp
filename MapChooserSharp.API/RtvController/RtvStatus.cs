namespace MapChooserSharp.API.RtvController;

/// <summary>
/// Status of RTV
/// </summary>
public enum RtvStatus
{
    /// <summary>
    /// If enabled and accepting RTV
    /// </summary>
    Enabled = 0,
    
    /// <summary>
    /// If disabled by admin
    /// </summary>
    Disabled,
    
    /// <summary>
    /// If RTV is in cooldown
    /// </summary>
    InCooldown,
    
    /// <summary>
    /// If another vote is ongoing. like map vote
    /// </summary>
    AnotherVoteOngoing,
    
    /// <summary>
    /// If RTV is already triggered and waiting for map change or vote start.
    /// </summary>
    Triggered,
}