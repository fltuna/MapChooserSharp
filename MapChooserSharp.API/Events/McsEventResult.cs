namespace MapChooserSharp.API.Events;

/// <summary>
/// API enum for map chooser api event
/// </summary>
public enum McsEventResult
{
    /// <summary>
    /// The action will normally execute
    /// </summary>
    Continue = 0,
    
    /// <summary>
    /// The action will normally execute, but value will be modified
    /// </summary>
    Changed = 1,
    
    /// <summary>
    /// The action will cancel
    /// </summary>
    Handled = 2,
}