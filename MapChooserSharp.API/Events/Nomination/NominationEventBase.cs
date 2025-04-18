using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Events.Nomination;

/// <summary>
/// Base parameter class for McsNomination events
/// </summary>
/// <param name="modulePrefix">Prefix for module</param>
public abstract class NominationEventBase(string modulePrefix) : McsEventParam(modulePrefix)
{
    /// <summary>
    /// Player who nominated map. if nominator is console, then param is null
    /// </summary>
    public abstract CCSPlayerController? Player { get; }

    /// <summary>
    /// Map config data
    /// </summary>
    public abstract IMapConfig MapConfig { get; }
}