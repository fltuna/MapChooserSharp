using MapChooserSharp.Modules.McsMenu;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.PluginConfig.Models;

public class McsNominationConfig(
    List<McsSupportedMenuType> availableMenuTypes,
    McsSupportedMenuType currentMenuType,
    DateTime nominationExpiringTime,
    int requiredTimeToLogin)
    : IMcsNominationConfig
{
    public List<McsSupportedMenuType> AvailableMenuTypes { get; } = availableMenuTypes;
    public McsSupportedMenuType CurrentMenuType { get; } = currentMenuType;
    
    /// <summary>
    /// Minutes
    /// </summary>
    [Obsolete("This variable will removed in feature. after migrated to PlayerManager")]
    public DateTime LoginSessionExpiringTime { get; } = nominationExpiringTime;
    
    /// <summary>
    /// Minutes
    /// </summary>
    [Obsolete("This variable will removed in feature. after migrated to PlayerManager")]
    public int RequiredTimeToLogin { get; } = requiredTimeToLogin;
}