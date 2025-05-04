namespace MapChooserSharp.API.RtvController;

/// <summary>
/// Result of player rtv execution
/// </summary>
public enum PlayerRtvResult
{
    /// <summary>
    /// RTV is successfully executed
    /// </summary>
    Success = 0,
    
    /// <summary>
    /// Player is already voted to RTV
    /// </summary>
    AlreadyInRtv,
    
    /// <summary>
    /// If RTV command is in cooldown
    /// </summary>
    CommandInCooldown,
    
    /// <summary>
    /// If RTV command is disabled by admin command
    /// </summary>
    CommandDisabled,
    
    /// <summary>
    /// If another vote is ongoing. like map vote
    /// </summary>
    AnotherVoteOngoing,
    
    /// <summary>
    /// If player is disallowed by external API's return
    /// </summary>
    NotAllowed,
    
    /// <summary>
    /// If RTV is already triggered and we don't need to do anything
    /// </summary>
    RtvTriggeredAlready,
}