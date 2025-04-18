namespace MapChooserSharp.API.Events;

/// <summary>
/// Base parameter class for McsEvent
/// </summary>
public abstract class McsEventParam(string modulePrefix): IMcsEvent
{
    /// <summary>
    /// Module prefix for printing in Callback result.
    /// </summary>
    public string ModulePrefix { get; } = modulePrefix;
}