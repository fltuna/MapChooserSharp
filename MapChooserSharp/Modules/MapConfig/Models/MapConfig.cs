using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapConfig.Models;

public sealed class MapConfig(
    string mapName,
    string mapNameAlias,
    string mapDescription,
    bool isDisabled,
    long workshopId,
    bool onlyNomination,
    int maxExtends,
    int maxExtCommandUses,
    int mapTime,
    int extendTimePerExtends,
    int mapRounds,
    int extendRoundsPerExtends,
    INominationConfig nominationConfig,
    IMapCooldown mapCooldown,
    Dictionary<string, Dictionary<string, string>> extraConfiguration,
    List<IMapGroupSettings> groupSettings)
    : IMapConfig
{
    public string MapName { get; } = mapName;
    public string MapNameAlias { get; } = mapNameAlias;
    public string MapDescription { get; } = mapDescription;
    public bool IsDisabled { get; } = isDisabled;
    public long WorkshopId { get; } = workshopId;
    public bool OnlyNomination { get; } = onlyNomination;
    public int MaxExtends { get; } = maxExtends;
    public int MaxExtCommandUses { get; } = maxExtCommandUses;
    public int MapTime { get; } = mapTime;
    public int ExtendTimePerExtends { get; } = extendTimePerExtends;
    public int MapRounds { get; } = mapRounds;
    public int ExtendRoundsPerExtends { get; } = extendRoundsPerExtends;
    public List<IMapGroupSettings> GroupSettings { get; } = groupSettings;
    public INominationConfig NominationConfig { get; } = nominationConfig;
    public IMapCooldown MapCooldown { get; } = mapCooldown;
    public Dictionary<string, Dictionary<string, string>> ExtraConfiguration { get; } = extraConfiguration;
}