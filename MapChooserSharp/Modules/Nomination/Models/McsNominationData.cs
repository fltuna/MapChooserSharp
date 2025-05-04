using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.Nomination;

namespace MapChooserSharp.Modules.Nomination.Models;

public class McsNominationData(IMapConfig mapConfig)
    : IMcsNominationData
{
    public IMapConfig MapConfig { get; } = mapConfig;
    public HashSet<int> NominationParticipants { get; } = new ();
    
    // Make mutable for overriding nomination status by admin nomination
    public bool IsForceNominated { get; set; } = false;
}