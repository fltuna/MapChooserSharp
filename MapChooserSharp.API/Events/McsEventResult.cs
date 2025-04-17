namespace MapChooserSharp.API.Events;

/// <summary>
/// API enum for map chooser api event
/// </summary>
public enum McsEventResult
{
    /// <summary>
    /// The original action will normally execute.
    /// </summary>
    Continue = 0,
    
    /// <summary>
    /// The original action will normally execute, but value will be modified.
    /// But currently unused.
    /// </summary>
    Changed = 1,
    
    /// <summary>
    /// The original action cancelled, but other event listeners are still executed.
    /// </summary>
    Handled = 2,
    
    /// <summary>
    /// The original action cancelled, Also, cancels all other event listeners.
    /// </summary>
    Stop = 3,
}