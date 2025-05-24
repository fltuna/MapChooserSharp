using System.Text;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.WorkshopSync;

internal class McsWorkshopMapSynchronizer(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsWorkshopMapSynchronizer";
    public override string ModuleChatPrefix => $" {ChatColors.Purple}[MCS WS]{ChatColors.Default}";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private IMcsPluginConfigProvider _configProvider = null!;
    private IMcsInternalMapConfigProviderApi _mapConfigProvider = null!;
    private HttpClient _httpClient = null!;
    private MapConfigRepository _mapConfigRepository = null!;

    protected override void OnAllPluginsLoaded()
    {
        _configProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _mapConfigRepository = ServiceProvider.GetRequiredService<MapConfigRepository>();
        _httpClient = new HttpClient();

        var workshopCollectionIds = _configProvider.PluginConfig.GeneralConfig.WorkshopCollectionIds;
        if (workshopCollectionIds != null && workshopCollectionIds.Length > 0)
        {
            Logger.LogInformation("[MCS WS] Found Workshop Collection IDs in config. Starting sync process...");
            SyncWorkshopCollections(workshopCollectionIds);
        }
        else
        {
            Logger.LogInformation("[MCS WS] No Workshop Collection IDs found in config. Skipping sync.");
        }
    }

    protected override void OnUnloadModule()
    {
        _httpClient.Dispose();
    }

    private void SyncWorkshopCollections(string[] collectionIds)
    {
        if (collectionIds.Length == 0)
            return;

        Logger.LogInformation($"[MCS WS] Syncing {collectionIds.Length} workshop collections.");

        foreach (string collectionId in collectionIds)
        {
            _ = SyncWorkshopCollectionAsync(collectionId.Trim());
        }
    }

    private async Task<int> SyncWorkshopCollectionAsync(string collectionId)
    {
        if (string.IsNullOrWhiteSpace(collectionId) || !long.TryParse(collectionId, out _))
        {
            Logger.LogWarning($"[MCS WS] Invalid Workshop Collection ID format: '{collectionId}'. Skipping.");
            return 0;
        }

        try
        {
            string url = $"https://steamcommunity.com/sharedfiles/filedetails/?id={collectionId}";
            Logger.LogInformation($"[MCS WS] Fetching Workshop collection: {url}");

            string pageSource;
            using (var response = await _httpClient.GetAsync(url))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError($"[MCS WS] Failed to fetch Workshop collection {collectionId}. Status code: {response.StatusCode}");
                    return 0;
                }
                pageSource = await response.Content.ReadAsStringAsync();
            }

            var pattern = new Regex(@"<a href=""https://steamcommunity.com/sharedfiles/filedetails/\?id=(\d+)"">.*?<div class=""workshopItemTitle"">(.*?)</div>", RegexOptions.Singleline);
            var matches = pattern.Matches(pageSource);

            if (matches.Count == 0)
            {
                Logger.LogWarning($"[MCS WS] No maps found in Workshop collection {collectionId}. The collection might be empty, private, or the page structure might have changed.");
                return 0;
            }

            Logger.LogInformation($"[MCS WS] Found {matches.Count} potential maps in Workshop collection {collectionId}.");

            int newMapsAdded = 0;

            Server.NextFrame(() =>
            {
                int processedInThisFrame = 0;
                foreach (Match match in matches)
                {
                    string currentWorkshopIdStr = match.Groups[1].Value;
                    string mapTitle = match.Groups[2].Value.Trim();

                    if (!long.TryParse(currentWorkshopIdStr, out long currentWorkshopId))
                    {
                        Logger.LogWarning($"[MCS WS] Failed to parse workshop ID '{currentWorkshopIdStr}' for map '{mapTitle}'. Skipping.");
                        continue;
                    }

                    string validMapName = CreateValidMapName(mapTitle, currentWorkshopIdStr);

                    if (_mapConfigProvider.GetMapConfig(currentWorkshopId) != null)
                    {
                        Logger.LogDebug($"[MCS WS] Map '{mapTitle}' (ID: {currentWorkshopId}) already exists in map settings. Skipping.");
                        continue;
                    }

                    var defaultConfig = _configProvider.PluginConfig.GeneralConfig; // For fallback values if needed
                    var defaultMapCycleConfig = _configProvider.PluginConfig.MapCycleConfig;

                    var defaultMapConfig = GetDefaultMapConfig();

                    var newMapConfig = new NullableMapConfig
                    {
                        MapName = validMapName,
                        MapNameAlias = mapTitle, // Use original title as alias by default
                        MapDescription = $"Workshop map: {mapTitle}",
                        IsDisabled = false,
                        WorkshopId = currentWorkshopId,
                        OnlyNomination = false
                    };

                    if (defaultMapConfig != null)
                    {
                        newMapConfig.IsDisabled = defaultMapConfig.IsDisabled;
                        newMapConfig.OnlyNomination = defaultMapConfig.OnlyNomination;
                        newMapConfig.MaxExtends = defaultMapConfig.MaxExtends;
                        newMapConfig.MaxExtCommandUses = defaultMapConfig.MaxExtCommandUses;
                        newMapConfig.MapTime = defaultMapConfig.MapTime;
                        newMapConfig.ExtendTimePerExtends = defaultMapConfig.ExtendTimePerExtends;
                        newMapConfig.MapRounds = defaultMapConfig.MapRounds;
                        newMapConfig.ExtendRoundsPerExtends = defaultMapConfig.ExtendRoundsPerExtends;
                        newMapConfig.Cooldown = defaultMapConfig.Cooldown;
                        newMapConfig.RequiredPermissions = defaultMapConfig.RequiredPermissions ?? new List<string>();
                        newMapConfig.RestrictToAllowedUsersOnly = defaultMapConfig.RestrictToAllowedUsersOnly;
                        newMapConfig.AllowedSteamIds = defaultMapConfig.AllowedSteamIds ?? new List<ulong>();
                        newMapConfig.DisallowedSteamIds = defaultMapConfig.DisallowedSteamIds ?? new List<ulong>();
                        newMapConfig.MaxPlayers = defaultMapConfig.MaxPlayers;
                        newMapConfig.MinPlayers = defaultMapConfig.MinPlayers;
                        newMapConfig.ProhibitAdminNomination = defaultMapConfig.ProhibitAdminNomination;
                        newMapConfig.DaysAllowed = defaultMapConfig.DaysAllowed ?? new List<DayOfWeek>();
                        newMapConfig.AllowedTimeRanges = defaultMapConfig.AllowedTimeRanges ?? new List<ITimeRange>();
                        newMapConfig.GroupSettingsArray = defaultMapConfig.GroupSettingsArray ?? new List<string> { "default" };
                    }
                    else
                    {
                        newMapConfig.MaxExtends = 3;
                        newMapConfig.MaxExtCommandUses = 1;
                        newMapConfig.MapTime = 20;
                        newMapConfig.ExtendTimePerExtends = 15;
                        newMapConfig.MapRounds = 10;
                        newMapConfig.ExtendRoundsPerExtends = 5;
                        newMapConfig.Cooldown = 0;
                        newMapConfig.RequiredPermissions = new List<string>();
                        newMapConfig.RestrictToAllowedUsersOnly = false;
                        newMapConfig.AllowedSteamIds = new List<ulong>();
                        newMapConfig.DisallowedSteamIds = new List<ulong>();
                        newMapConfig.MaxPlayers = 0;
                        newMapConfig.MinPlayers = 0;
                        newMapConfig.ProhibitAdminNomination = false;
                        newMapConfig.DaysAllowed = new List<DayOfWeek>();
                        newMapConfig.AllowedTimeRanges = new List<ITimeRange>();
                        newMapConfig.GroupSettingsArray = new List<string> { "default" };
                    }

                    try
                    {
                        string mapConfigToml = ConvertMapConfigToTomlString(newMapConfig);
                        string configDir = Path.Combine(Plugin.ModuleDirectory, "config");
                        Directory.CreateDirectory(configDir); // Ensure directory exists
                                                              // Individual map files are typically stored in a 'maps' subdirectory within 'config'
                        string mapsDir = Path.Combine(configDir, "maps");
                        Directory.CreateDirectory(mapsDir);
                        string filePath = Path.Combine(mapsDir, $"{validMapName}.toml");

                        File.WriteAllText(filePath, mapConfigToml);
                        Logger.LogInformation($"[MCS WS] Created map settings for '{mapTitle}' (ID: {currentWorkshopId}) as '{filePath}'");
                        newMapsAdded++;
                        processedInThisFrame++;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"[MCS WS] Error creating map settings file for '{mapTitle}' (ID: {currentWorkshopId}): {ex.Message}");
                    }
                }

                if (processedInThisFrame > 0)
                {
                    Logger.LogInformation($"[MCS WS] Processed {processedInThisFrame} new maps from collection {collectionId}.");
                    Logger.LogInformation("[MCS WS] Reloading map configurations for changes to take effect...");

                    _mapConfigRepository.ReloadMapConfiguration();

                    Logger.LogInformation("[MCS WS] Map configurations reloaded successfully.");
                }
                else if (matches.Count > 0)
                {
                    Logger.LogInformation($"[MCS WS] All {matches.Count} maps from collection {collectionId} already exist or were skipped.");
                }
            });

            return newMapsAdded;
        }
        catch (HttpRequestException httpEx)
        {
            Logger.LogError($"[MCS WS] HTTP request error syncing Workshop collection {collectionId}: {httpEx.Message}");
            return 0;
        }
        catch (Exception ex)
        {
            Logger.LogError($"[MCS WS] General error syncing Workshop collection {collectionId}: {ex.Message}");
            Logger.LogError($"[MCS WS] StackTrace: {ex.StackTrace}");
            return 0;
        }
    }

    private NullableMapConfig? GetDefaultMapConfig()
    {
        string defaultConfigPath = Path.Combine(Plugin.ModuleDirectory, "config", "maps", "default.toml");
        if (!File.Exists(defaultConfigPath))
        {
            return null;
        }

        try
        {
            string tomlContent = File.ReadAllText(defaultConfigPath);
            var toml = Tomlyn.Toml.ToModel(tomlContent);

            var defaultConfig = new NullableMapConfig();

            if (toml.TryGetValue("MapNameAlias", out var mapNameAliasObj) && mapNameAliasObj is string mapNameAlias)
                defaultConfig.MapNameAlias = mapNameAlias;

            if (toml.TryGetValue("MapDescription", out var mapDescriptionObj) && mapDescriptionObj is string mapDescription)
                defaultConfig.MapDescription = mapDescription;

            if (toml.TryGetValue("IsDisabled", out var isDisabledObj) && isDisabledObj is bool isDisabled)
                defaultConfig.IsDisabled = isDisabled;

            if (toml.TryGetValue("WorkshopId", out var workshopIdObj) && workshopIdObj is long workshopId)
                defaultConfig.WorkshopId = workshopId;

            if (toml.TryGetValue("OnlyNomination", out var onlyNominationObj) && onlyNominationObj is bool onlyNomination)
                defaultConfig.OnlyNomination = onlyNomination;

            if (toml.TryGetValue("MaxExtends", out var maxExtendsObj) && maxExtendsObj is long maxExtends)
                defaultConfig.MaxExtends = (int)maxExtends;

            if (toml.TryGetValue("MaxExtCommandUses", out var maxExtCommandUsesObj) && maxExtCommandUsesObj is long maxExtCommandUses)
                defaultConfig.MaxExtCommandUses = (int)maxExtCommandUses;

            if (toml.TryGetValue("MapTime", out var mapTimeObj) && mapTimeObj is long mapTime)
                defaultConfig.MapTime = (int)mapTime;

            if (toml.TryGetValue("ExtendTimePerExtends", out var extendTimePerExtendsObj) && extendTimePerExtendsObj is long extendTimePerExtends)
                defaultConfig.ExtendTimePerExtends = (int)extendTimePerExtends;

            if (toml.TryGetValue("MapRounds", out var mapRoundsObj) && mapRoundsObj is long mapRounds)
                defaultConfig.MapRounds = (int)mapRounds;

            if (toml.TryGetValue("ExtendRoundsPerExtends", out var extendRoundsPerExtendsObj) && extendRoundsPerExtendsObj is long extendRoundsPerExtends)
                defaultConfig.ExtendRoundsPerExtends = (int)extendRoundsPerExtends;

            if (toml.TryGetValue("Cooldown", out var cooldownObj) && cooldownObj is long cooldown)
                defaultConfig.Cooldown = (int)cooldown;

            if (toml.TryGetValue("RestrictToAllowedUsersOnly", out var restrictToAllowedUsersOnlyObj) && restrictToAllowedUsersOnlyObj is bool restrictToAllowedUsersOnly)
                defaultConfig.RestrictToAllowedUsersOnly = restrictToAllowedUsersOnly;

            if (toml.TryGetValue("MaxPlayers", out var maxPlayersObj) && maxPlayersObj is long maxPlayers)
                defaultConfig.MaxPlayers = (int)maxPlayers;

            if (toml.TryGetValue("MinPlayers", out var minPlayersObj) && minPlayersObj is long minPlayers)
                defaultConfig.MinPlayers = (int)minPlayers;

            if (toml.TryGetValue("ProhibitAdminNomination", out var prohibitAdminNominationObj) && prohibitAdminNominationObj is bool prohibitAdminNomination)
                defaultConfig.ProhibitAdminNomination = prohibitAdminNomination;

            return defaultConfig;
        }
        catch (Exception ex)
        {
            Logger.LogError($"[MCS WS] Error reading default config: {ex.Message}");
            return null;
        }
    }

    private string ConvertMapConfigToTomlString(NullableMapConfig mapConfig)
    {
        var sb = new StringBuilder();
        // Section name should be the map's actual name (filename without .toml)
        // However, the parser structure seems to use the map name as the key in the main TOML structure.
        // For individual files, the filename itself is the key.
        // So, the content of workshop_map_xyz.toml would be:
        // DisplayName = "Workshop Map XYZ"
        // WorkshopId = 12345...
        // etc., without the [workshop_map_xyz] header inside the file itself.
        // The parser combines these files, and the filename becomes the section header.

        sb.AppendLine($"DisplayName = \"{TomlEncode(mapConfig.MapNameAlias)}\""); // mapConfig.Name is the sanitized one, MapNameAlias is closer to original title
        sb.AppendLine($"MapDescription = \"{TomlEncode(mapConfig.MapDescription)}\"");
        sb.AppendLine($"IsDisabled = {mapConfig.IsDisabled?.ToString().ToLowerInvariant() ?? "false"}");
        if (mapConfig.WorkshopId.HasValue && mapConfig.WorkshopId.Value > 0)
            sb.AppendLine($"WorkshopId = {mapConfig.WorkshopId.Value}");
        sb.AppendLine($"OnlyNomination = {mapConfig.OnlyNomination?.ToString().ToLowerInvariant() ?? "false"}");

        sb.AppendLine($"MaxExtends = {mapConfig.MaxExtends ?? 3}");
        sb.AppendLine($"MaxExtCommandUses = {mapConfig.MaxExtCommandUses ?? 1}");
        sb.AppendLine($"MapTime = {mapConfig.MapTime ?? 20}");
        sb.AppendLine($"ExtendTimePerExtends = {mapConfig.ExtendTimePerExtends ?? 15}");
        sb.AppendLine($"MapRounds = {mapConfig.MapRounds ?? 10}");
        sb.AppendLine($"ExtendRoundsPerExtends = {mapConfig.ExtendRoundsPerExtends ?? 5}");

        sb.AppendLine($"Cooldown = {mapConfig.Cooldown ?? 0}");

        // NominationConfig settings
        if (mapConfig.RequiredPermissions != null && mapConfig.RequiredPermissions.Any())
            sb.AppendLine($"RequiredPermissions = [{string.Join(", ", mapConfig.RequiredPermissions.Select(p => $"\"{TomlEncode(p)}\""))}]");
        else
            sb.AppendLine("RequiredPermissions = []");

        sb.AppendLine($"RestrictToAllowedUsersOnly = {(mapConfig.RestrictToAllowedUsersOnly?.ToString().ToLowerInvariant() ?? "false")}");

        if (mapConfig.AllowedSteamIds != null && mapConfig.AllowedSteamIds.Any())
            sb.AppendLine($"AllowedSteamIds = [{string.Join(", ", mapConfig.AllowedSteamIds)}]");
        else
            sb.AppendLine("AllowedSteamIds = []");

        if (mapConfig.DisallowedSteamIds != null && mapConfig.DisallowedSteamIds.Any())
            sb.AppendLine($"DisallowedSteamIds = [{string.Join(", ", mapConfig.DisallowedSteamIds)}]");
        else
            sb.AppendLine("DisallowedSteamIds = []");

        sb.AppendLine($"MaxPlayers = {mapConfig.MaxPlayers ?? 0}");
        sb.AppendLine($"MinPlayers = {mapConfig.MinPlayers ?? 0}");
        sb.AppendLine($"ProhibitAdminNomination = {(mapConfig.ProhibitAdminNomination?.ToString().ToLowerInvariant() ?? "false")}");

        if (mapConfig.DaysAllowed != null && mapConfig.DaysAllowed.Any())
            sb.AppendLine($"DaysAllowed = [{string.Join(", ", mapConfig.DaysAllowed.Select(d => $"\"{d}\""))}]");
        else
            sb.AppendLine("DaysAllowed = []");

        if (mapConfig.AllowedTimeRanges != null && mapConfig.AllowedTimeRanges.Any())
        {
            var timeRangesStr = mapConfig.AllowedTimeRanges.Select(tr =>
                $"{{ Start = \"{tr.StartTime:hh\\:mm}\", End = \"{tr.EndTime:hh\\:mm}\" }}"
            );
            sb.AppendLine($"AllowedTimeRanges = [{string.Join(", ", timeRangesStr)}]");
        }
        else
        {
            sb.AppendLine("AllowedTimeRanges = []");
        }

        // GroupSettings are usually references, new maps might not have them or default to one.
        if (mapConfig.GroupSettingsArray != null && mapConfig.GroupSettingsArray.Any())
            sb.AppendLine($"GroupSettings = [{string.Join(", ", mapConfig.GroupSettingsArray.Select(g => $"\"{TomlEncode(g)}\""))}]");
        else
            sb.AppendLine("GroupSettings = [\"default\"]"); // Default to "default" group if none specified

        // ExtraConfiguration is complex; for new maps, it's likely empty.
        // If needed, it would require careful construction. For now, assume empty.
        // sb.AppendLine("[extra]"); (if any)

        return sb.ToString();
    }

    private string TomlEncode(string value)
    {
        if (value == null) return string.Empty;
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    // Prevent invalid characters in map names
    private string CreateValidMapName(string workshopTitle, string workshopId)
    {
        string validName = Regex.Replace(workshopTitle, @"[^a-zA-Z0-9_\-\.]", "_");

        if (string.IsNullOrWhiteSpace(validName))
        {
            validName = "workshop_map_" + workshopId;
        }
        else if (validName.Length > 0 && !char.IsLetterOrDigit(validName[0]) && validName[0] != '_')
        {
            // Ensure it starts with a letter, digit, or underscore if it's not already.
            // Prepending "map_" might be too aggressive if the name is already somewhat valid.
            // Let's allow names starting with digits or underscores.
            // If it starts with something truly problematic (e.g. '-'), then prepend.
            if (validName.StartsWith("-") || validName.StartsWith(".")) // Example problematic starts
            {
                validName = "map_" + validName;
            }
        }
        // Max filename length considerations (usually not an issue for modern systems but good to keep in mind)
        // if (validName.Length > 100) validName = validName.Substring(0, 100);

        return validName.ToLowerInvariant();
    }
}
