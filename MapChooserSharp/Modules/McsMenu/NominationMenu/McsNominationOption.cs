using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu;

public sealed class McsNominationOption(IMapConfig mapConfig, bool isAdminNomination = false) : IMcsNominationOption
{
    public IMapConfig MapConfig { get; } = mapConfig;
    public bool IsAdminNomination { get; } = isAdminNomination;
}