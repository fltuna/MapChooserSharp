using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapConfig.Models;

public sealed class MapCooldown(int coolDown): IMapCooldown
{
    public int MapConfigCooldown { get; } = coolDown;
    public int CurrentCooldown { get; set; }
}