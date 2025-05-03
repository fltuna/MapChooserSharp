using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

public interface IMcsNominationOption
{
    public IMapConfig MapConfig { get; }
    
    public bool IsAdminNomination { get; }
}