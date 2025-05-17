using CounterStrikeSharp.API.Core.Capabilities;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapCycleController;
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
    /// MapCycleController API, You can manipulate map cycle system
    /// </summary>
    public IMcsMapCycleControllerApi McsMapCycleController { get; }
    
    /// <summary>
    /// MapCycleExtendController API, You can manipulate map extend system
    /// </summary>
    public IMcsMapCycleExtendControllerApi McsMapCycleExtendController { get; }
    
    /// <summary>
    /// McsMapCycleExtendVoteController API, You can manipulate vote map extend system
    /// </summary>
    public IMcsMapCycleExtendVoteControllerApi McsMapCycleExtendVoteController { get; }
    
    /// <summary>
    /// Nomination API, You can manipulate nomination system
    /// </summary>
    public IMcsNominationApi McsNominationApi { get; }
    
    /// <summary>
    /// VoteController API, You can manipulate vote system
    /// </summary>
    public IMcsMapVoteControllerApi McsMapVoteControllerApi { get; }
    
    /// <summary>
    /// RTVController API, You can manipulate RTV system
    /// </summary>
    public IMcsRtvControllerApi McsRtvControllerApi { get; }
    
    /// <summary>
    /// MapConfigProvider API, You can manipulate map config
    /// </summary>
    public IMcsMapConfigProviderApi McsMapConfigProviderApi { get; }
}