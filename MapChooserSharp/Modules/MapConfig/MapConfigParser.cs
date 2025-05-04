using System.Text;
using CounterStrikeSharp.API;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapConfig.Models;
using Tomlyn;
using Tomlyn.Model;

namespace MapChooserSharp.Modules.MapConfig;

internal class MapConfigParser(string configPath)
{
    private string ConfigPath { get; } = configPath;

    public IMcsInternalMapConfigProviderApi Load()
    {
        string directory = Path.GetDirectoryName(ConfigPath)!;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // If maps.toml exists, then load from this file.
        if (File.Exists(ConfigPath))
            return LoadConfigFromFile();
    
        // If there is no *.toml files in maps directory, then create default config.
        if (Directory.GetFiles(directory, "*.toml", SearchOption.AllDirectories).Length == 0)
        {
            WriteDefaultConfig();
        }

        // Load generated default config or splitted configs.
        return LoadConfigFromFile();
    }

    private IMcsInternalMapConfigProviderApi LoadConfigFromFile()
    {
        string configText;

        // If file exists, then load from file.
        if (File.Exists(ConfigPath))
        {
            configText = File.ReadAllText(ConfigPath);
        }
        // If not exists, load all toml file from directory
        else
        {
            StringBuilder combinedConfig = new StringBuilder();
            ProcessDirectory(Path.GetDirectoryName(ConfigPath)!, combinedConfig);
            configText = combinedConfig.ToString();
        }
        
        TomlTable toml = Toml.ToModel(configText);
        
        // 1. まずDefault設定を読み込む
        NullableMapConfig? defaultConfig = null;
        
        // TomlTableの階層構造にアクセス
        if (toml.TryGetValue("MapChooserSharpSettings", out var defaultSettingsObj) && defaultSettingsObj is TomlTable defaultSettings)
        {
            if (defaultSettings.TryGetValue("Default", out var defaultObj) && defaultObj is TomlTable defaultTable)
            {
                defaultConfig = new NullableMapConfig();
                ParseTomlTable(defaultTable, defaultConfig);
            }
        }
        
        // 2. Default設定が存在するか、および必須フィールドが設定されているかを検証
        if (defaultConfig == null)
        {
            throw new InvalidOperationException("Default settings section is missing");
        }
        
        VerifyRequiredDefaultSettings(defaultConfig);
        
        // 3. Default設定が正しく設定されていた場合のみ、グループとマップコンフィグの処理に進む
        Dictionary<string, NullableMapConfig> groupConfigs = new Dictionary<string, NullableMapConfig>();
        Dictionary<string, IMapGroupSettings> groupSettings = new Dictionary<string, IMapGroupSettings>();
        
        // グループ設定を処理
        if (toml.TryGetValue("MapChooserSharpSettings", out var groupSettingsObj) && groupSettingsObj is TomlTable groupSection)
        {
            if (groupSection.TryGetValue("Groups", out var groupsObj) && groupsObj is TomlTable groups)
            {
                foreach (var (groupName, groupValue) in groups)
                {
                    if (groupValue is TomlTable groupTable)
                    {
                        NullableMapConfig groupConfig = new NullableMapConfig { MapName = groupName };
                        ParseTomlTable(groupTable, groupConfig);
                        
                        // Look for extra configuration for this group
                        string sectionKey = $"MapChooserSharpSettings.Groups.{groupName}";
                        ProcessExtraSections(toml, sectionKey, groupConfig);
                        
                        groupConfigs[groupName] = groupConfig;
                        
                        // Store only group name and cooldown in an IMapGroupSettings
                        int cooldown = groupConfig.Cooldown ?? 0;
                        groupSettings[groupName] = new MapGroupSettings(groupName, new MapCooldown(cooldown));
                    }
                }
            }
        }
        
        // マップ設定を処理
        Dictionary<string, NullableMapConfig> mapConfigs = new Dictionary<string, NullableMapConfig>();
        
        foreach (var (key, value) in toml)
        {
            // MapChooserSharpSettings は特別な設定セクションなのでスキップ
            if (key == "MapChooserSharpSettings")
            {
                continue;
            }
            
            if (value is TomlTable mapTable)
            {
                // Create map config with default values
                NullableMapConfig mapConfig = new NullableMapConfig
                {
                    MapName = key,
                    // Copy default values
                    Cooldown = defaultConfig.Cooldown,
                    MapTime = defaultConfig.MapTime,
                    MaxExtends = defaultConfig.MaxExtends,
                    ExtendTimePerExtends = defaultConfig.ExtendTimePerExtends,
                    MapRounds = defaultConfig.MapRounds,
                    ExtendRoundsPerExtends = defaultConfig.ExtendRoundsPerExtends,
                    // Set default values for other properties
                    MapNameAlias = defaultConfig.MapNameAlias,
                    MapDescription = defaultConfig.MapDescription,
                    IsDisabled = defaultConfig.IsDisabled,
                    WorkshopId = defaultConfig.WorkshopId,
                    OnlyNomination = defaultConfig.OnlyNomination,
                    RequiredPermissions = defaultConfig.RequiredPermissions,
                    RestrictToAllowedUsersOnly = defaultConfig.RestrictToAllowedUsersOnly,
                    AllowedSteamIds = defaultConfig.AllowedSteamIds?.ToList(),
                    DisallowedSteamIds = defaultConfig.DisallowedSteamIds?.ToList(),
                    MaxPlayers = defaultConfig.MaxPlayers,
                    MinPlayers = defaultConfig.MinPlayers,
                    ProhibitAdminNomination = defaultConfig.ProhibitAdminNomination,
                    DaysAllowed = defaultConfig.DaysAllowed?.ToList(),
                    AllowedTimeRanges = defaultConfig.AllowedTimeRanges?.ToList()
                };
                
                // Parse map-specific settings
                ParseTomlTable(mapTable, mapConfig);
                
                // Apply group settings if specified
                if (mapConfig.GroupSettingsArray != null && mapConfig.GroupSettingsArray.Count > 0)
                {
                    // Process groups in reverse order (so first one has highest priority)
                    for (int i = mapConfig.GroupSettingsArray.Count - 1; i >= 0; i--)
                    {
                        string groupName = mapConfig.GroupSettingsArray[i];
                        if (groupConfigs.TryGetValue(groupName, out var groupConfig))
                        {
                            // Apply group settings
                            ApplyNonCooldownSettings(groupConfig, mapConfig);
                            
                            // Merge extra configuration from group
                            MergeExtraConfiguration(groupConfig, mapConfig);
                            
                            // Add group settings to the GroupSettings list
                            if (groupSettings.TryGetValue(groupName, out var groupSetting))
                            {
                                // Add group settings in order of priority (first in list = highest priority)
                                mapConfig.GroupSettings.Insert(0, groupSetting);
                            }
                        }
                    }
                }
                
                // Process extra sections for this map
                ProcessExtraSections(toml, key, mapConfig);
                
                mapConfigs[key] = mapConfig;
            }
        }
        
        // Convert NullableMapConfig to IMapConfig
        Dictionary<string, IMapConfig> finalMapConfigs = new Dictionary<string, IMapConfig>();
        foreach (var (mapName, config) in mapConfigs)
        {
            // Create IMapCooldown
            var mapCooldown = new MapCooldown(config.Cooldown!.Value);
            
            // Create INominationConfig
            var nominationConfig = new NominationConfig(
                config.RequiredPermissions!,
                config.RestrictToAllowedUsersOnly!.Value,
                config.AllowedSteamIds!,
                config.DisallowedSteamIds!,
                config.MaxPlayers!.Value,
                config.MinPlayers!.Value,
                config.ProhibitAdminNomination!.Value,
                config.DaysAllowed!,
                config.AllowedTimeRanges!
            );
            
            // Create IMapConfig
            finalMapConfigs[mapName] = new Models.MapConfig(
                mapName,
                config.MapNameAlias!,
                config.MapDescription!,
                config.IsDisabled!.Value,
                config.WorkshopId!.Value,
                config.OnlyNomination!.Value,
                config.MaxExtends!.Value,
                config.MapTime!.Value,
                config.ExtendTimePerExtends!.Value,
                config.MapRounds!.Value,
                config.ExtendRoundsPerExtends!.Value,
                nominationConfig,
                mapCooldown,
                config.ExtraConfiguration,
                config.GroupSettings
            );
        }

        Dictionary<string, IMapGroupSettings> actualGroupSettings = new();
        
        foreach (var (key, value) in groupConfigs)
        {
            actualGroupSettings[key] = new MapGroupSettings(key, new MapCooldown(value.Cooldown ?? 0));
        }
        
        return new McsMapConfigProvider(finalMapConfigs, actualGroupSettings);
    }
    
    private void VerifyRequiredDefaultSettings(NullableMapConfig defaultConfig)
    {
        List<string> missingSettings = new List<string>();
        
        // 基本設定のチェック
        if (defaultConfig.Cooldown == null) missingSettings.Add("Cooldown");
        if (defaultConfig.MapTime == null) missingSettings.Add("MapTime");
        if (defaultConfig.MaxExtends == null) missingSettings.Add("MaxExtends");
        if (defaultConfig.ExtendTimePerExtends == null) missingSettings.Add("ExtendTimePerExtends");
        if (defaultConfig.MapRounds == null) missingSettings.Add("MapRounds");
        if (defaultConfig.ExtendRoundsPerExtends == null) missingSettings.Add("ExtendRoundsPerExtends");
        
        // その他のIMapConfig関連のプロパティも確認
        if (defaultConfig.MapNameAlias == null) missingSettings.Add("MapNameAlias");
        if (defaultConfig.MapDescription == null) missingSettings.Add("MapDescription");
        if (defaultConfig.IsDisabled == null) missingSettings.Add("IsDisabled");
        if (defaultConfig.WorkshopId == null) missingSettings.Add("WorkshopId");
        if (defaultConfig.OnlyNomination == null) missingSettings.Add("OnlyNomination");
        
        // NominationConfig関連のプロパティ
        if (defaultConfig.RequiredPermissions == null) missingSettings.Add("RequiredPermission");
        if (defaultConfig.RestrictToAllowedUsersOnly == null) missingSettings.Add("RestrictToAllowedUsersOnly");
        if (defaultConfig.AllowedSteamIds == null) missingSettings.Add("AllowedSteamIds");
        if (defaultConfig.DisallowedSteamIds == null) missingSettings.Add("DisallowedSteamIds");
        if (defaultConfig.MaxPlayers == null) missingSettings.Add("MaxPlayers");
        if (defaultConfig.MinPlayers == null) missingSettings.Add("MinPlayers");
        if (defaultConfig.ProhibitAdminNomination == null) missingSettings.Add("ProhibitAdminNomination");
        if (defaultConfig.DaysAllowed == null) missingSettings.Add("DaysAllowed");
        if (defaultConfig.AllowedTimeRanges == null) missingSettings.Add("AllowedTimeRanges");
        
        if (missingSettings.Count > 0)
        {
            throw new InvalidOperationException($"Default settings section is missing required values: {string.Join(", ", missingSettings)}");
        }
    }
    
    private void ProcessExtraSections(TomlTable toml, string sectionKey, NullableMapConfig config)
    {
        // sectionKeyを階層的に分割
        string[] keyParts = sectionKey.Split('.');
        
        // トップレベルから順に階層をたどる
        TomlTable currentLevel = toml;
        for (int i = 0; i < keyParts.Length; i++)
        {
            if (currentLevel.TryGetValue(keyParts[i], out var nextLevel) && nextLevel is TomlTable nextTable)
            {
                if (i == keyParts.Length - 1)
                {
                    // sectionKeyの最後の階層に到達した場合、extra設定を探す
                    if (nextTable.TryGetValue("extra", out var extraObj) && extraObj is TomlTable extraTable)
                    {
                        foreach (var (sectionName, sectionValue) in extraTable)
                        {
                            if (sectionValue is TomlTable valueTable)
                            {
                                if (!config.ExtraConfiguration.ContainsKey(sectionName))
                                {
                                    config.ExtraConfiguration[sectionName] = new Dictionary<string, string>();
                                }
                                
                                foreach (var (exKey, exValue) in valueTable)
                                {
                                    config.ExtraConfiguration[sectionName][exKey] = exValue.ToString() ?? string.Empty;
                                }
                            }
                        }
                    }
                }
                currentLevel = nextTable;
            }
            else
            {
                // 階層が見つからない場合は終了
                break;
            }
        }
    }
    
    private void ParseTomlTable(TomlTable table, NullableMapConfig config)
    {
        foreach (var (key, value) in table)
        {
            switch (key)
            {
                case "MapNameAlias":
                    if (value is string aliasName)
                        config.MapNameAlias = aliasName;
                    break;
                case "MapDescription":
                    if (value is string description)
                        config.MapDescription = description;
                    break;
                case "IsDisabled":
                    if (value is bool boolValue)
                        config.IsDisabled = boolValue;
                    break;
                case "WorkshopId":
                    if (value is long longValue)
                        config.WorkshopId = longValue;
                    break;
                case "OnlyNomination":
                    if (value is bool nominationValue)
                        config.OnlyNomination = nominationValue;
                    break;
                case "MaxExtends":
                    if (value is long extendsValue)
                        config.MaxExtends = (int)extendsValue;
                    break;
                case "MapTime":
                    if (value is long mapTimeValue)
                        config.MapTime = (int)mapTimeValue;
                    break;
                case "ExtendTimePerExtends":
                    if (value is long extendTimeValue)
                        config.ExtendTimePerExtends = (int)extendTimeValue;
                    break;
                case "MapRounds":
                    if (value is long mapRoundsValue)
                        config.MapRounds = (int)mapRoundsValue;
                    break;
                case "ExtendRoundsPerExtends":
                    if (value is long extendRoundsValue)
                        config.ExtendRoundsPerExtends = (int)extendRoundsValue;
                    break;
                case "Cooldown":
                    if (value is long cooldownValue)
                        config.Cooldown = (int)cooldownValue;
                    break;
                case "RequiredPermissions":
                    if (value is TomlArray permission)
                        config.RequiredPermissions = ParseStringArray(permission);
                    break;
                case "RestrictToAllowedUsersOnly":
                    if (value is bool restrictValue)
                        config.RestrictToAllowedUsersOnly = restrictValue;
                    break;
                case "AllowedSteamIds":
                    if (value is TomlArray allowedArray)
                    {
                        if (config.AllowedSteamIds == null)
                        {
                            config.AllowedSteamIds = ParseULongArray(allowedArray);
                        }
                        else
                        {
                            config.AllowedSteamIds.AddRange(ParseULongArray(allowedArray));
                        }
                    }
                    break;
                case "DisallowedSteamIds":
                    if (value is TomlArray disallowedArray)
                    {
                        if (config.DisallowedSteamIds == null)
                        {
                            config.DisallowedSteamIds = ParseULongArray(disallowedArray);
                        }
                        else
                        {
                            config.DisallowedSteamIds.AddRange(ParseULongArray(disallowedArray));
                        }
                    }
                    break;
                case "MaxPlayers":
                    if (value is long maxPlayersValue)
                        config.MaxPlayers = (int)maxPlayersValue;
                    break;
                case "MinPlayers":
                    if (value is long minPlayersValue)
                        config.MinPlayers = (int)minPlayersValue;
                    break;
                case "ProhibitAdminNomination":
                    if (value is bool prohibitValue)
                        config.ProhibitAdminNomination = prohibitValue;
                    break;
                case "DaysAllowed":
                    if (value is TomlArray daysArray)
                        config.DaysAllowed = ParseDayOfWeekArray(daysArray);
                    break;
                case "AllowedTimeRanges":
                    if (value is TomlArray timeRangesArray)
                        config.AllowedTimeRanges = ParseTimeRangeArray(timeRangesArray);
                    break;
                case "GroupSettings":
                    if (value is TomlArray groupSettingsArray)
                        config.GroupSettingsArray = ParseStringArray(groupSettingsArray);
                    break;
            }
        }
    }
    
    private List<ulong> ParseULongArray(TomlArray array)
    {
        List<ulong> result = new List<ulong>();
        foreach (var item in array)
        {
            if (item is long longValue)
            {
                if (longValue <= 0)
                    continue;
                
                ulong ulongValue = (ulong)longValue;
                
                result.Add(ulongValue);
            }
        }
        return result;
    }
    
    private List<string> ParseStringArray(TomlArray array)
    {
        List<string> result = new List<string>();
        foreach (var item in array)
        {
            if (item is string stringValue)
            {
                result.Add(stringValue);
            }
        }
        return result;
    }
    
    private List<DayOfWeek> ParseDayOfWeekArray(TomlArray array)
    {
        List<DayOfWeek> result = new List<DayOfWeek>();
        foreach (var item in array)
        {
            if (item is string dayString)
            {
                if (Enum.TryParse<DayOfWeek>(dayString, true, out var day))
                {
                    result.Add(day);
                }
            }
        }
        return result;
    }
    
    private List<ITimeRange> ParseTimeRangeArray(TomlArray array)
    {
        List<ITimeRange> result = new List<ITimeRange>();
        foreach (var item in array)
        {
            if (item is string timeRangeString)
            {
                string[] parts = timeRangeString.Split('-');
                if (parts.Length == 2)
                {
                    if (TimeOnly.TryParse(parts[0], out var startTime) && 
                        TimeOnly.TryParse(parts[1], out var endTime))
                    {
                        result.Add(new TimeRange(startTime, endTime));
                    }
                }
            }
        }
        return result;
    }
    
    private void ApplyNonCooldownSettings(NullableMapConfig groupConfig, NullableMapConfig mapConfig)
    {
        // if (groupConfig.MapNameAlias != null)
        //     mapConfig.MapNameAlias = groupConfig.MapNameAlias;
        //
        //
        // if (groupConfig.MapDescription != null)
        //     mapConfig.MapDescription = groupConfig.MapDescription;
            
        if (groupConfig.IsDisabled != null)
            mapConfig.IsDisabled = groupConfig.IsDisabled;
            
        if (groupConfig.WorkshopId != null)
            mapConfig.WorkshopId = groupConfig.WorkshopId;
            
        if (groupConfig.OnlyNomination != null)
            mapConfig.OnlyNomination = groupConfig.OnlyNomination;
            
        if (groupConfig.MaxExtends != null)
            mapConfig.MaxExtends = groupConfig.MaxExtends;
            
        if (groupConfig.MapTime != null)
            mapConfig.MapTime = groupConfig.MapTime;
            
        if (groupConfig.ExtendTimePerExtends != null)
            mapConfig.ExtendTimePerExtends = groupConfig.ExtendTimePerExtends;
            
        if (groupConfig.MapRounds != null)
            mapConfig.MapRounds = groupConfig.MapRounds;
            
        if (groupConfig.ExtendRoundsPerExtends != null)
            mapConfig.ExtendRoundsPerExtends = groupConfig.ExtendRoundsPerExtends;
            
        // Nomination config
        if (groupConfig.RequiredPermissions != null)
            mapConfig.RequiredPermissions = groupConfig.RequiredPermissions;
            
        if (groupConfig.RestrictToAllowedUsersOnly != null)
            mapConfig.RestrictToAllowedUsersOnly = groupConfig.RestrictToAllowedUsersOnly;
            
        if (groupConfig.AllowedSteamIds != null)
            mapConfig.AllowedSteamIds?.AddRange(groupConfig.AllowedSteamIds);
            
        if (groupConfig.DisallowedSteamIds != null)
            mapConfig.DisallowedSteamIds?.AddRange(groupConfig.DisallowedSteamIds);
            
        if (groupConfig.MaxPlayers != null)
            mapConfig.MaxPlayers = groupConfig.MaxPlayers;
            
        if (groupConfig.MinPlayers != null)
            mapConfig.MinPlayers = groupConfig.MinPlayers;
            
        if (groupConfig.ProhibitAdminNomination != null)
            mapConfig.ProhibitAdminNomination = groupConfig.ProhibitAdminNomination;
            
        if (groupConfig.DaysAllowed != null)
            mapConfig.DaysAllowed = [..groupConfig.DaysAllowed];
            
        if (groupConfig.AllowedTimeRanges != null)
            mapConfig.AllowedTimeRanges = [..groupConfig.AllowedTimeRanges];
    }
    
    private void MergeExtraConfiguration(NullableMapConfig sourceConfig, NullableMapConfig targetConfig)
    {
        foreach (var (sectionName, values) in sourceConfig.ExtraConfiguration)
        {
            if (!targetConfig.ExtraConfiguration.ContainsKey(sectionName))
            {
                targetConfig.ExtraConfiguration[sectionName] = new Dictionary<string, string>();
            }
            
            foreach (var (key, value) in values)
            {
                if (!targetConfig.ExtraConfiguration[sectionName].ContainsKey(key))
                {
                    targetConfig.ExtraConfiguration[sectionName][key] = value;
                }
            }
        }
    }

    private static void ProcessDirectory(string directoryPath, StringBuilder combinedContent)
    {
        foreach (string filePath in Directory.GetFiles(directoryPath, "*.toml"))
        {
            try
            {
                string content = File.ReadAllText(filePath);

                if (combinedContent.Length > 0 && !combinedContent.ToString().EndsWith(Environment.NewLine))
                {
                    combinedContent.AppendLine();
                }

                combinedContent.Append(content);

                if (!content.EndsWith(Environment.NewLine))
                {
                    combinedContent.AppendLine();
                }
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"[MapChooserSharp] failed to parse file: {filePath}\n{ex}");
            }
        }

        // Recursive directory processing
        foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
        {
            ProcessDirectory(subDirectoryPath, combinedContent);
        }
    }
    
    private void WriteDefaultConfig()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

        string defaultConfig = @"
[MapChooserSharpSettings.Default]
# These are default value for map settings.
# You can modify these section, but do not remove.
MapNameAlias = """"
MapDescription = """"
IsDisabled = false
WorkshopId = 0
OnlyNomination = false
Cooldown = 0
MaxExtends = 3
ExtendTimePerExtends = 15
MapTime = 20
ExtendRoundsPerExtends = 5
MapRounds = 10
RequiredPermissions = []
RestrictToAllowedUsersOnly = false
AllowedSteamIds = []
DisallowedSteamIds = []
MaxPlayers = 0
MinPlayers = 0
ProhibitAdminNomination = false
DaysAllowed = []
AllowedTimeRanges = []




# ==============================================================
# Full configuration example
# ==============================================================

# Full configuration
[ze_example_abc]

# ===== Basic Map Settings =====

# If set, it will display this string instead of map name
MapNameAlias = ""ze example a b c""

# If set, it will display this string when vote finished, and before map transition
MapDescription = ""Let's play ze_example_abc!""

# If set, the map cannot be nominated even if admin
# Default value is disabled
IsDisabled = false

# If set, this map recognized as workshop map
WorkshopId = 1234567891234

# If set, the map will not included in random vote pick
OnlyNomination = false

# Cooldown for this map, specify with hours
Cooldown = 60

# Max extends
MaxExtends = 3

# Extend time per extend (minutes)
ExtendTimePerExtends = 15

# Map time (Map's default mp_timelimit value)
MapTime = 20

# If cycle is round based
# How many rounds extended in per extend?
# ExtendRoundsPerExtends = 5
# Map rounds (Map's default mp_maxrounds)
# MapRounds = 10

# ===== Nomination Settings =====

# If this value is set, nominator should have specified permission
RequiredPermissions = [""css/generic""]

# Restrict nomination to user who in the AllowedSteamIds
RestrictToAllowedUsersOnly = false

# If this value(s) is set, Allowed clients are able to bypasses the permission check
# But, cannot bypass check if ProhibitAdminNomination is true and user is not a root user.
AllowedSteamIds = [987654321]

# If this value(s) is set, Disallowed clients are not able to nominate this map
DisallowedSteamIds = [123456789]

# Max players requires to nominate this map
MaxPlayers = 64

# Minimal players requires to nominate this map
MinPlayers = 10

# Make only root user and console can nominate this map
ProhibitAdminNomination = false

# If this value(s) are set, the nomination will be restricted to these days.
# Also, if DaysAllowed is set, the check will be combined.
DaysAllowed = [""wednesday"", ""monday""]

# If this value(s) are set, the nomination will be restricted to these time ranges.
# Also, if DaysAllowed is set, the check will be combined.
AllowedTimeRanges = [""10:00-12:00"", ""20:00-22:00"", ""22:00-03:00""]


# Extra configuration for external plugin
[ze_example_abc.extra.shop]
cost = 100


# ==============================================================
# Minimal configuration example
# ==============================================================

# Minimal configuration....... Yes you can define simply!
# This will uses default value from [MapChooserSharpSettings.Default] section
[ze_example_xyz]




# ==============================================================
# Group configuration example
# ==============================================================

# Configuration with groups
# By assigning this group to the map's GroupSettings, the group's setting values can be treated as the map's default settings values.
[MapChooserSharpSettings.Groups.HardZeMap]
# Cooldown is are special value.
Cooldown = 30

OnlyNomination = true
DaysAllowed = [""saturday"", ""sunday""]
AllowedTimeRanges = [""18:00-00:00""]

# Yes! you can still define extra configuration for external plugin even if in group settings!
[MapChooserSharpSettings.Groups.HardZeMap.extra.shop]
cost = 100


[ze_example_123]
# If this is set, we will use group settings.
# If there is no matched group, it will silently ignored.
GroupSettings = [""HardZeMap""]

# But if we set value in map settings and already defined in group, it will override group settings.
OnlyNomination = false

# But cooldown is are individual
# If
Cooldown = 60



# ==============================================================
# Adding multiple groups to map group
# ==============================================================

[MapChooserSharpSettings.Groups.Group1]
RequiredPermissions = [""css/root""]
AllowedTimeRanges = [""18:00-00:00""]
MaxPlayers = 1000
AllowedSteamIds = [987654321]
DisallowedSteamIds = [987654321]


[MapChooserSharpSettings.Groups.Group2]
RequiredPermissions = [""css/generic""]
DaysAllowed = [""saturday"", ""sunday""]
MinPlayers = 300
AllowedSteamIds = [123456789]
DisallowedSteamIds = [123456789]



[ze_example_789]
# If there are multiple groups and the values of the groups are the same, the value of the first group specified takes precedence.
# In the following example, Group1 has the highest priority and RequiredPermissions is ""css/root"".
#
# But AllowedSteamIds and DisallowedSteamIds are exceptional.
# All AllowedSteamIds and DisallowedSteamIds will combined to 1 array. this is intended for grouping the special user
#
GroupSettings = [""Group1"", ""Group2""]
";
    
    
        File.WriteAllText(ConfigPath, defaultConfig);
    }
}


internal class NullableMapConfig
{
    public string MapName { get; set; } = null!;
    public string? MapNameAlias { get; set; }
    public string? MapDescription { get; set; }
    public bool? IsDisabled { get; set; }
    public long? WorkshopId { get; set; }
    public bool? OnlyNomination { get; set; }
    public int? MaxExtends { get; set; }
    public int? MapTime { get; set; }
    public int? ExtendTimePerExtends { get; set; }
    public int? MapRounds { get; set; }
    public int? ExtendRoundsPerExtends { get; set; }
    public List<IMapGroupSettings> GroupSettings { get; set; } = new();
        
    // Nomination config properties
    public List<string>? RequiredPermissions { get; set; }
    public bool? RestrictToAllowedUsersOnly { get; set; }
    public List<ulong>? AllowedSteamIds { get; set; }
    public List<ulong>? DisallowedSteamIds { get; set; }
    public int? MaxPlayers { get; set; }
    public int? MinPlayers { get; set; }
    public bool? ProhibitAdminNomination { get; set; }
    public List<DayOfWeek>? DaysAllowed { get; set; }
    public List<ITimeRange>? AllowedTimeRanges { get; set; }
        
    // Map cooldown
    public int? Cooldown { get; set; }
        
    // Extra configuration
    public Dictionary<string, Dictionary<string, string>> ExtraConfiguration { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        
    // Group settings array
    public List<string>? GroupSettingsArray { get; set; }
}