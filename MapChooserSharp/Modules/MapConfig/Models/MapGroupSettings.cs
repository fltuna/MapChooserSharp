using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapConfig.Models;

public class MapGroupSettings(string groupName, IMapCooldown groupCooldown) : IMapGroupSettings
{
    public string GroupName { get; } = groupName;
    public IMapCooldown GroupCooldown { get; } = groupCooldown;
}