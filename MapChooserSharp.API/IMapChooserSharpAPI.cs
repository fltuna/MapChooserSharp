using CounterStrikeSharp.API.Core.Capabilities;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.API.RtvController;

namespace MapChooserSharp.API;

public interface IMapChooserSharpApi
{
    public static readonly PluginCapability<IMapChooserSharpApi> Capability = new("mapchoosersharp:api");
    
    public INominationApi NominationApi { get; }
    
    public IMapChooserSharpApi MapChooserSharpApi { get; }
    
    public IRtvControllerApi RtvControllerApi { get; }
}