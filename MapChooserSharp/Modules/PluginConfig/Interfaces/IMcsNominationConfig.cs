using MapChooserSharp.Modules.McsMenu;

namespace MapChooserSharp.Modules.PluginConfig.Interfaces;

internal interface IMcsNominationConfig
{
    internal List<McsSupportedMenuType> AvailableMenuTypes { get; }
    
    internal McsSupportedMenuType CurrentMenuType { get; }
    /// <summary>
    /// Minutes
    /// </summary>
    [Obsolete("This variable will removed in feature. after migrated to PlayerManager")]
    public DateTime LoginSessionExpiringTime { get; }
    
    /// <summary>
    /// Minutes
    /// </summary>
    [Obsolete("This variable will removed in feature. after migrated to PlayerManager")]
    public int RequiredTimeToLogin { get; }
}