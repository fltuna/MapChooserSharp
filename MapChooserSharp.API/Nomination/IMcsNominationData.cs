using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.API.Nomination;


/// <summary>
/// Nomination data of MapChooserSharp
/// </summary>
public interface IMcsNominationData
{
    /// <summary>
    /// Nominated map config
    /// </summary>
    public IMapConfig MapConfig { get; }
    
    /// <summary>
    /// User slot of nomination participants
    /// </summary>
    public HashSet<int> NominationParticipants { get; }
    
    /// <summary>
    /// Is force nominated by admin
    /// </summary>
    public bool IsForceNominated { get; set; }
}