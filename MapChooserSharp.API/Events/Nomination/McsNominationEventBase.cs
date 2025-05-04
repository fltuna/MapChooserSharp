using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.Nomination;

namespace MapChooserSharp.API.Events.Nomination;

/// <summary>
/// Base parameter class for McsNomination events
/// </summary>
/// <param name="modulePrefix">Prefix for module</param>
public abstract class McsNominationEventBase(string modulePrefix) : McsEventParam(modulePrefix)
{
    /// <summary>
    /// Player who nominated map. if nominator is console, then param is null
    /// </summary>
    public abstract CCSPlayerController? Player { get; }

    /// <summary>
    /// Nomination Data
    /// </summary>
    public abstract IMcsNominationData NominationData { get; }
}