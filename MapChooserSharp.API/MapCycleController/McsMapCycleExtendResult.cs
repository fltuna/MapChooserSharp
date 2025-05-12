namespace MapChooserSharp.API.MapCycleController;

/// <summary>
/// Extend result of McsMapCycleExtendController
/// </summary>
public enum McsMapCycleExtendResult
{
    /// <summary>
    /// When map extended
    /// </summary>
    Extended,
    
    /// <summary>
    /// When failed to extend map somehow
    /// </summary>
    FailedToExtend,

    /// <summary>
    /// If map time is minus when extended
    /// </summary>
    FailedTimeCannotBeZeroOrNegative
}