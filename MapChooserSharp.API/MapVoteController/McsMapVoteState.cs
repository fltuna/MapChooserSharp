namespace MapChooserSharp.API.MapVoteController;

/// <summary>
/// Enum the state of the map vote.
/// </summary>
public enum McsMapVoteState
{
    /// <summary>
    /// There is no active vote and next map is not confirmed.
    /// </summary>
    NoActiveVote = 0,
    
    /// <summary>
    /// Cancelling the current vote
    /// </summary>
    Cancelling,
    
    /// <summary>
    /// Initialize accepted and starting vote
    /// </summary>
    InitializeAccepted,
    
    /// <summary>
    /// Initializing vote.
    /// </summary>
    Initializing,
    
    /// <summary>
    /// Vote in progress
    /// </summary>
    Voting,
    
    /// <summary>
    /// Runoff voting in progress
    /// </summary>
    RunoffVoting,
    
    /// <summary>
    /// Finalizing vote
    /// </summary>
    Finalizing,
    
    /// <summary>
    /// Next map confirmed and cannot be start vote.
    /// </summary>
    NextMapConfirmed
}