namespace MapChooserSharp.API.MapConfig;

/// <summary>
/// Nomination related settings
/// </summary>
public interface INominationConfig
{
    /// <summary>
    /// Required CSSharp permission (e.g. css/generic)
    /// </summary>
    public List<string> RequiredPermissions { get; }

    /// <summary>
    /// Restrict nomination to user who in the AllowedSteamIds.
    /// </summary>
    public bool RestrictToAllowedUsersOnly { get; }
    
    /// <summary>
    /// If this value is specified in config, then bypasses the required permission check.
    /// But, cannot bypass check if ProhibitAdminNomination is true and user is not a root user.
    /// </summary>
    public List<long> AllowedSteamIds { get; }

    /// <summary>
    /// If this value is specified in config, then the user cannot be nominated
    /// </summary>
    public List<long> DisallowedSteamIds { get; }
    
    /// <summary>
    /// When player count is exceed this value, normal user will not able to nominate this map.
    /// </summary>
    public int MaxPlayers { get; }
    
    /// <summary>
    /// When player count is not exceed this value, normal user will not able to nominate this map.
    /// </summary>
    public int MinPlayers { get; }
    
    /// <summary>
    /// When this value set to true, only root user and console can nominate.
    /// </summary>
    public bool ProhibitAdminNomination { get; }
    
    /// <summary>
    /// Days when nomination is allowed
    /// </summary>
    public List<DayOfWeek> DaysAllowed { get; }

    /// <summary>
    /// Time ranges when nomination is allowed
    /// </summary>
    public List<ITimeRange> AllowedTimeRanges { get; }

}