using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapConfig.Models;

public class NominationConfig(
    List<string> requiredPermission,
    bool restrictToAllowedUsersOnly,
    List<ulong> allowedSteamIds,
    List<ulong> disallowedSteamIds,
    int maxPlayers,
    int minPlayers,
    bool prohibitAdminNomination,
    List<DayOfWeek> daysAllowed,
    List<ITimeRange> allowedTimeRanges)
    : INominationConfig
{
    public List<string> RequiredPermissions { get; } = requiredPermission;
    public bool RestrictToAllowedUsersOnly { get; } = restrictToAllowedUsersOnly;
    public List<ulong> AllowedSteamIds { get; } = allowedSteamIds;
    public List<ulong> DisallowedSteamIds { get; } = disallowedSteamIds;
    public int MaxPlayers { get; } = maxPlayers;
    public int MinPlayers { get; } = minPlayers;
    public bool ProhibitAdminNomination { get; } = prohibitAdminNomination;
    public List<DayOfWeek> DaysAllowed { get; } = daysAllowed;
    public List<ITimeRange> AllowedTimeRanges { get; } = allowedTimeRanges;

    // 曜日文字列をDayOfWeek列挙型に変換するヘルパーメソッド
    public static DayOfWeek ParseDayOfWeek(string day)
    {
        return day.ToLower() switch
        {
            "monday" => DayOfWeek.Monday,
            "tuesday" => DayOfWeek.Tuesday,
            "wednesday" => DayOfWeek.Wednesday,
            "thursday" => DayOfWeek.Thursday,
            "friday" => DayOfWeek.Friday,
            "saturday" => DayOfWeek.Saturday,
            "sunday" => DayOfWeek.Sunday,
            _ => throw new ArgumentException($"Invalid day of week: {day}")
        };
    }
}