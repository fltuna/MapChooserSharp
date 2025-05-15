namespace MapChooserSharp.API.MapCycleController;

/// <summary>
/// Result of player !ext execution
/// </summary>
public enum PlayerExtResult
{
    /// <summary>
    /// !ext is successfully executed
    /// </summary>
    Success,
    
    /// <summary>
    /// Player is already voted to extend
    /// </summary>
    AlreadyVoted,
    
    /// <summary>
    /// If command is cooldown
    /// </summary>
    CommandInCooldown,
    
    /// <summary>
    /// If command is disabled by admin command
    /// </summary>
    CommandDisabled,
    
    /// <summary>
    /// If player is disallowed by external API's return
    /// </summary>
    NotAllowed,
    
    /// <summary>
    /// If reached the extend count limit
    /// </summary>
    ReachedLimit
}