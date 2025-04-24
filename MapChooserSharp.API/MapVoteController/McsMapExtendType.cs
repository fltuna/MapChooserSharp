namespace MapChooserSharp.API.MapVoteController;

/// <summary>
/// Decides what should be extended
/// </summary>
public enum McsMapExtendType
{
    /// <summary>
    /// Extend type is extending mp_timelimit
    /// </summary>
    TimeLimit = 0,
    
    /// <summary>
    /// Extend type is extending mp_maxrounds
    /// </summary>
    Rounds,
    
    /// <summary>
    /// Extend type is extending mp_roundtime
    /// </summary>
    RoundTime,
}