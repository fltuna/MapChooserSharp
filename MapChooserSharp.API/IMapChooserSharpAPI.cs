using CounterStrikeSharp.API.Core.Capabilities;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.API.RtvController;

namespace MapChooserSharp.API;

/// <summary>
/// Api class for MapChooserSharp
/// </summary>
public interface IMapChooserSharpApi
{
    /// <summary>
    /// Plugin capability
    /// </summary>
    public static readonly PluginCapability<IMapChooserSharpApi> Capability = new("mapchoosersharp:api");
    
    /// <summary>
    /// Mcs event system, you can register and unregister event handlers here
    /// </summary>
    public IMcsEventSystem EventSystem { get; }
    
    /// <summary>
    /// Nomination API, You can manipulate nomination system
    /// </summary>
    public INominationApi NominationApi { get; }
    
    /// <summary>
    /// VoteController API, You can manipulate vote system
    /// </summary>
    public IMapVoteControllerApi MapVoteControllerApi { get; }
    
    /// <summary>
    /// RTVController API, You can manipulate RTV system
    /// </summary>
    public IRtvControllerApi RtvControllerApi { get; }
}