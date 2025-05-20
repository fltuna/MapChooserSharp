using CounterStrikeSharp.API;
using MapChooserSharp.Models;
using MapChooserSharp.Modules.MapVote.Countdown;
using MapChooserSharp.Modules.McsDatabase;
using MapChooserSharp.Modules.McsMenu;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Models;
using MapChooserSharp.Util;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using Tomlyn;
using Tomlyn.Model;

namespace MapChooserSharp.Modules.PluginConfig;

internal sealed class McsPluginConfigParser(string configPath, IServiceProvider provider): PluginBasicFeatureBase(provider)
{
    private string ConfigPath { get; } = configPath;
    
    private static readonly Version CurrentConfigVersion = PluginConstants.PluginVersion;
    
    private List<McsSupportedMenuType> _avaiableMenuTypes = new();

    internal IMcsPluginConfigProvider Load()
    {
        if (!File.Exists(ConfigPath))
        {
            WriteDefaultConfig();
        }

        _avaiableMenuTypes = GetAvailableMenuTypes();

        return LoadConfigFromFile();
    }

    private IMcsPluginConfigProvider LoadConfigFromFile()
    {
        string tomlContent;
        try
        {
            tomlContent = File.ReadAllText(ConfigPath);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to load settings file: {ex.Message}", ex);
        }

        TomlTable tomlModel;
        try
        {
            tomlModel = Toml.ToModel(tomlContent);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse settings file: {ex.Message}", ex);
        }
        
        CheckConfigVersion(tomlModel);

        var mapCycleConfig = ParseMapCycleConfig(tomlModel);
        var nominationConfig = ParseNominationConfig(tomlModel);
        var voteConfig = ParseVoteConfig(tomlModel);
        var generalConfig = ParseGeneralConfig(tomlModel);
        
        var pluginConfig = new McsPluginConfig(voteConfig, nominationConfig, mapCycleConfig, generalConfig);
        
        return new McsPluginConfigProvider(pluginConfig);
    }

    private void CheckConfigVersion(TomlTable tomlModel)
    {
        if (!tomlModel.TryGetValue("ConfigInformation", out var pluginInformationObj))
        {
            Logger.LogWarning("Failed to check plugin config version. We do not recommend to remove that.");
            return;
        }

        if (pluginInformationObj is not TomlTable pluginInformation)
        {
            Logger.LogWarning("Failed to check plugin config version. We do not recommend to remove that.");
            return;
        }

        if (!pluginInformation.TryGetValue("Version", out var configVersionObj) || 
            configVersionObj is not string configVersionString)
        {
            Logger.LogWarning("Failed to check plugin config version. We do not recommend to remove that.");
            return;
        }
        
        if (!Version.TryParse(configVersionString, out var configVersion))
        {
            Logger.LogWarning("Failed to check plugin config version. We do not recommend to remove that.");
            return;
        }
        
        int result = configVersion.CompareTo(CurrentConfigVersion);

        if (result < 0)
        {
            Logger.LogWarning("Config file version is older than the current version. Some settings may not work correctly.");
            Logger.LogWarning($"Config version: {configVersion} | Current version: {CurrentConfigVersion}");
        }
        else if (result > 0)
        {
            Logger.LogWarning("Config file version is newer than the current version. Some settings may not be compatible.");
            Logger.LogWarning($"Config version: {configVersion} | Current version: {CurrentConfigVersion}");
        }
        else
        {
            // Do nothing because versions match
        }
    }

    private IMcsMapCycleConfig ParseMapCycleConfig(TomlTable tomlModel)
    {
        if (!tomlModel.TryGetValue("MapCycle", out var mapCycleObj) || mapCycleObj is not TomlTable mapCycleTable)
        {
            throw new InvalidOperationException("MapCycle section is not found");
        }
        
        if (!mapCycleTable.TryGetValue("FallbackMaxExtends", out var defaultMaxExtendsObj) || 
            defaultMaxExtendsObj is not long defaultMaxExtendsLong)
        {
            throw new InvalidOperationException("MapCycle.FallbackMaxExtends is not found or invalid");
        }

        int defaultMaxExtends = (int)defaultMaxExtendsLong;

        
        if (!mapCycleTable.TryGetValue("FallbackMaxExtCommandUses", out var defaultExtCmdUsesObj) || 
            defaultExtCmdUsesObj is not long defaultExtCmdUsesLong)
        {
            throw new InvalidOperationException("MapCycle.FallbackMaxExtCommandUses is not found or invalid");
        }

        int defaultExtCmdUses = (int)defaultExtCmdUsesLong;
        

        if (!mapCycleTable.TryGetValue("FallbackExtendTimePerExtends", out var defaultExtendsTime) || 
            defaultExtendsTime is not long defaultExtendsTimeLong)
        {
            throw new InvalidOperationException("MapCycle.FallbackExtendTimePerExtends is not found or invalid");
        }
        
        int fallbackExtendTimePerExtends = (int)defaultExtendsTimeLong;
        

        if (!mapCycleTable.TryGetValue("FallbackExtendRoundsPerExtends", out var defaultExtendsRound) || 
            defaultExtendsRound is not long defaultExtendsRoundLong)
        {
            throw new InvalidOperationException("MapCycle.FallbackExtendRoundsPerExtends is not found or invalid");
        }
        
        int fallbackExtendRoundsPerExtends = (int)defaultExtendsRoundLong;
        
        
        
        return new McsMapCycleConfig(defaultMaxExtends, defaultExtCmdUses, fallbackExtendTimePerExtends, fallbackExtendRoundsPerExtends);
    }

    private IMcsNominationConfig ParseNominationConfig(TomlTable tomlModel)
    {
        if (!tomlModel.TryGetValue("Nomination", out var nominationObj) || nominationObj is not TomlTable nominationTable)
        {
            throw new InvalidOperationException("Nomination section is not found");
        }

        if (!nominationTable.TryGetValue("MenuType", out var menuTypeObj) || menuTypeObj is not string menuTypeStr)
        {
            throw new InvalidOperationException("Nomination.MenuType is not found or invalid");
        }
        
        var availableMenus =  _avaiableMenuTypes;
        
        var currentMenuType = DecideMenuType(menuTypeStr, availableMenus);

        return new McsNominationConfig(availableMenus, currentMenuType);
    }

    private IMcsVoteConfig ParseVoteConfig(TomlTable tomlModel)
    {
        if (!tomlModel.TryGetValue("MapVote", out var voteObj) || voteObj is not TomlTable voteTable)
        {
            throw new InvalidOperationException("MapVote section is not found");
        }

        if (!voteTable.TryGetValue("MenuType", out var menuTypeObj) || menuTypeObj is not string menuTypeStr)
        {
            throw new InvalidOperationException("MapVote.MenuType is not found or invalid");
        }

        if (!voteTable.TryGetValue("CountdownUiType", out var countdownUiTypeObj) || countdownUiTypeObj is not string countdownUiType)
        {
            throw new InvalidOperationException("MapVote.MenCountdownUiTypeuType is not found or invalid");
        }
        

        if (!voteTable.TryGetValue("MaxVoteElements", out var maxVoteElementsObj) || maxVoteElementsObj is not long maxVoteElementsLong)
        {
            throw new InvalidOperationException("MapVote.MaxVoteElements is not found or invalid");
        }

        if (!voteTable.TryGetValue("ShouldPrintVoteToChat", out var shouldPrintVoteToChatObj) || shouldPrintVoteToChatObj is not bool shouldPrintVoteToChatBool)
        {
            throw new InvalidOperationException("MapVote.ShouldPrintVoteToChat is not found or invalid");
        }

        if (!voteTable.TryGetValue("ShouldPrintVoteRemainingTime", out var shouldPrintVoteRemainingTimeToChatObj) || shouldPrintVoteRemainingTimeToChatObj is not bool shouldPrintVoteRemainingTimeBool)
        {
            throw new InvalidOperationException("MapVote.ShouldPrintVoteRemainingTime is not found or invalid");
        }
        
        var availableMenus =  _avaiableMenuTypes;

        var currentMenuType = DecideMenuType(menuTypeStr, availableMenus);

        var soundConfig = ParseVoteSoundConfig(voteTable);

        if (!Enum.TryParse(countdownUiType, true, out McsCountdownUiType countdownType))
        {
            throw new InvalidOperationException("MapVote.MenCountdownUiTypeuType is invalid");
        }

        return new McsVoteConfig(availableMenus, currentMenuType, (int)maxVoteElementsLong, shouldPrintVoteToChatBool, shouldPrintVoteRemainingTimeBool, soundConfig, countdownType);
    }

    private IMcsVoteSoundConfig ParseVoteSoundConfig(TomlTable tomlModel)
    {
        if (!tomlModel.TryGetValue("Sound", out var voteSoundObj) || voteSoundObj is not TomlTable voteSoundTable)
        {
            throw new InvalidOperationException("MapVote.Sound section is not found");
        }

        if (!voteSoundTable.TryGetValue("SoundFile", out var soundFileObj) || soundFileObj is not string soundFile)
        {
            throw new InvalidOperationException("MapVote.Sound.SoundFile is not found or invalid");
        }
        
        
        // ====================================
        // Initial vote sounds
        // ====================================

        if (!voteSoundTable.TryGetValue("InitialVoteCountdownStartSound", out var initialVoteCountdownStartSoundObj) || initialVoteCountdownStartSoundObj is not string initialVoteCountdownStartSound)
        {
            throw new InvalidOperationException("MapVote.Sound.InitialVoteCountdownStartSound is not found or invalid");
        }

        if (!voteSoundTable.TryGetValue("InitialVoteStartSound", out var initialVoteStartSoundObj) || initialVoteStartSoundObj is not string initialVoteStartSound)
        {
            throw new InvalidOperationException("MapVote.Sound.InitialVoteStartSound is not found or invalid");
        }

        if (!voteSoundTable.TryGetValue("InitialVoteFinishSound", out var initialVoteFinishSoundObj) || initialVoteFinishSoundObj is not string initialVoteFinishSound)
        {
            throw new InvalidOperationException("MapVote.Sound.InitialVoteFinishSound is not found or invalid");
        }
        
        List<string> initialVoteCountdownSoundTable = new();
        
        for (int i = 1; i <= 10; i++)
        {
            if (!voteSoundTable.TryGetValue($"InitialVoteCountdownSound{i}", out var initialVoteCountdownSoundObj) || initialVoteCountdownSoundObj is not string initialVoteCountdownSound)
            {
                throw new InvalidOperationException($"MapVote.Sound.InitialVoteCountdownSound{i} is not found or invalid");
            }
            
            initialVoteCountdownSoundTable.Add(initialVoteCountdownSound);
        }
        
        var initialVoteSounds = new McsVoteSound(initialVoteCountdownStartSound, initialVoteStartSound, initialVoteFinishSound, initialVoteCountdownSoundTable);
        
        
        // ====================================
        // Runoff vote sounds
        // ====================================

        if (!voteSoundTable.TryGetValue("RunoffVoteCountdownStartSound", out var runoffVoteCountdownStartSoundObj) || runoffVoteCountdownStartSoundObj is not string runoffVoteCountdownStartSound)
        {
            throw new InvalidOperationException("MapVote.Sound.RunoffVoteCountdownStartSound is not found or invalid");
        }

        if (!voteSoundTable.TryGetValue("RunoffVoteStartSound", out var runoffVoteStartSoundObj) || runoffVoteStartSoundObj is not string runoffVoteStartSound)
        {
            throw new InvalidOperationException("MapVote.Sound.RunoffVoteStartSound is not found or invalid");
        }

        if (!voteSoundTable.TryGetValue("RunoffVoteFinishSound", out var runoffVoteFinishSoundObj) || runoffVoteFinishSoundObj is not string runoffVoteFinishSound)
        {
            throw new InvalidOperationException("MapVote.Sound.RunoffVoteFinishSound is not found or invalid");
        }
        
        List<string> runoffVoteCountdownSoundTable = new();
        
        for (int i = 1; i <= 10; i++)
        {
            if (!voteSoundTable.TryGetValue($"RunoffVoteCountdownSound{i}", out var runoffVoteCountdownSoundObj) || runoffVoteCountdownSoundObj is not string runoffVoteCountdownSound)
            {
                throw new InvalidOperationException($"MapVote.Sound.RunoffVoteCountdownSound{i} is not found or invalid");
            }
            
            runoffVoteCountdownSoundTable.Add(runoffVoteCountdownSound);
        }
        
        var runoffVoteSounds = new McsVoteSound(runoffVoteCountdownStartSound, runoffVoteStartSound, runoffVoteFinishSound, runoffVoteCountdownSoundTable);
        

        return new McsVoteSoundConfig(soundFile, initialVoteSounds, runoffVoteSounds);
    }


    private IMcsGeneralConfig ParseGeneralConfig(TomlTable tomlModel)
    {
        if (!tomlModel.TryGetValue("General", out var generalObj) || generalObj is not TomlTable generalTable)
        {
            throw new InvalidOperationException("General section is not found");
        }
        
        if (!generalTable.TryGetValue("ShouldUseAliasMapNameIfAvailable", out var aliasNameSettingObj) || aliasNameSettingObj is not bool aliasNameSetting)
        {
            throw new InvalidOperationException("General.ShouldUseAliasMapNameIfAvailable is not found or invalid");
        }
        
        if (!generalTable.TryGetValue("VerboseCooldownPrint", out var verboseCooldownPrintObj) || verboseCooldownPrintObj is not bool verboseCooldownPrint)
        {
            throw new InvalidOperationException("General.VerboseCooldownPrint is not found or invalid");
        }

        var workshopCollectionIds = generalTable.TryGetValue("WorkshopCollectionIds", out var workshopCollectionIdsObj) && workshopCollectionIdsObj is TomlArray workshopCollectionIdsArray
            ? workshopCollectionIdsArray.Select(x => x.ToString() ?? string.Empty).ToArray()
            : Array.Empty<string>();
        
        bool shouldAutoFixMapName = generalTable.TryGetValue("ShouldAutoFixMapName", out var shouldAutoFixMapNameObj) && shouldAutoFixMapNameObj is bool autoFixMapNameSetting
            ? autoFixMapNameSetting
            : false;
        
        var sqlConfig = ParseSqlConfig(generalTable);
        
        return new McsGeneralConfig(aliasNameSetting, verboseCooldownPrint, workshopCollectionIds, shouldAutoFixMapName, sqlConfig);
    }

    private McsSqlConfig ParseSqlConfig(TomlTable tomlModel)
    {
        if (!tomlModel.TryGetValue("Sql", out var sqlTableObj) || sqlTableObj is not TomlTable sqlTable)
        {
            throw new InvalidOperationException("General.Sql section is not found");
        }
        
        if (!sqlTable.TryGetValue("Type", out var sqlTypeObj) || sqlTypeObj is not string sqlType)
        {
            throw new InvalidOperationException("General.Sql.Type is not found or invalid");
        }
        
        if (!sqlTable.TryGetValue("Address", out var sqlAddressObj) || sqlAddressObj is not string sqlHost)
        {
            throw new InvalidOperationException("General.Sql.Address is not found or invalid");
        }
        
        if (!sqlTable.TryGetValue("Port", out var sqlPortObj) || sqlPortObj is not string sqlPort)
        {
            throw new InvalidOperationException("General.Sql.Port is not found or invalid");
        }
        
        if (!sqlTable.TryGetValue("DatabaseName", out var sqlDbNameObj) || sqlDbNameObj is not string sqlDbName)
        {
            throw new InvalidOperationException("General.Sql.DatabaseName is not found or invalid");
        }
        
        if (!sqlTable.TryGetValue("User", out var sqlUserObj) || sqlUserObj is not string sqlUser)
        {
            throw new InvalidOperationException("General.Sql.User is not found or invalid");
        }
        
        if (!sqlTable.TryGetValue("Password", out var sqlPasswordObj) || sqlPasswordObj is not string sqlPassword)
        {
            throw new InvalidOperationException("General.Sql.Password is not found or invalid");
        }
        
        
        
        if (!sqlTable.TryGetValue("GroupInformationTableName", out var groupTableNameObj) || groupTableNameObj is not string groupTableName)
        {
            throw new InvalidOperationException("General.Sql.GroupInformationTableName is not found or invalid");
        }
        
        if (!sqlTable.TryGetValue("MapInformationTableName", out var mapTableNameObj) || mapTableNameObj is not string mapTableName)
        {
            throw new InvalidOperationException("General.Sql.MapInformationTableName is not found or invalid");
        }


        if (!Enum.TryParse(sqlType, true, out McsSupportedSqlType type))
        {
            throw new InvalidOperationException("General.Sql.Type is invalid");
        }

        var sqlConfig = new McsSqlConfig(sqlHost, sqlPort, sqlDbName, sqlUser, ref sqlPassword, groupTableName, mapTableName, type);
        
        return sqlConfig;
    }
    
    
    private McsSupportedMenuType DecideMenuType(string menuTypeStr, List<McsSupportedMenuType> availableMenus)
    {
        // if menu type is invalid, fall back to builtin html menu
        if (!Enum.TryParse(menuTypeStr, true, out McsSupportedMenuType currentMenuType))
        {
            currentMenuType = McsSupportedMenuType.BuiltInHtml;
        }
        
        // if menu type is blank, fall back to builtin html menu
        if (string.IsNullOrWhiteSpace(menuTypeStr))
        {
            currentMenuType = McsSupportedMenuType.BuiltInHtml;
        }
        
        // if menu type is not avaialbe in server, fall back to builtin html menu
        if (!availableMenus.Contains(currentMenuType))
        {
            currentMenuType = McsSupportedMenuType.BuiltInHtml;
        }
        
        return currentMenuType;
    }
    
    private List<McsSupportedMenuType> GetAvailableMenuTypes()
    {
        List<McsSupportedMenuType> availableMenuTypes = new();

        foreach (McsSupportedMenuType type in Enum.GetValues<McsSupportedMenuType>())
        {
            switch (type)
            {
                case McsSupportedMenuType.BuiltInHtml:
                    availableMenuTypes.Add(McsSupportedMenuType.BuiltInHtml);
                    break;
                
                case McsSupportedMenuType.Cs2ScreenMenuApi:
                    if (!AssemblyUtility.IsAssemblyLoaded("CS2ScreenMenuAPI"))
                        break;

                    availableMenuTypes.Add(McsSupportedMenuType.Cs2ScreenMenuApi);
                    break;
                
                case McsSupportedMenuType.Cs2MenuManagerScreen:
                    Server.PrintToConsole("CHECKING");
                    if (!AssemblyUtility.IsAssemblyLoaded("CS2MenuManager"))
                        break;
                    
                    availableMenuTypes.Add(McsSupportedMenuType.Cs2MenuManagerScreen);
                    break;
            }
        }
        
        return availableMenuTypes;
    }

    private void WriteDefaultConfig()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

        string defaultConfig = @$"# MapChooserSharp Plugin Configuration

[General]
# Should use alias map name if available? (This will take effect to all things that prints a map name)
ShouldUseAliasMapNameIfAvailable = true

# Should print the cooldown? 
# if true, and commands in cooldown, it will show cooldown message with seconds
# if false, and commands in cooldown, it will show only cooldown message
VerboseCooldownPrint = true

# Workshop Collection IDs to automatically fetch maps from
# Example: WorkshopCollectionIds = [ ""3070257939"", ""1234567890"" ]
WorkshopCollectionIds = []

# Should automatically fix map name in map settings when map starts?
# This will update the map name in settings to match the actual map name from the server
ShouldAutoFixMapName = true


[General.Sql]
# SQL settings for MapChooserSharp

# What SQL provider should be use?
#
# Currently Supports:
# - Sqlite
# - MySQL
#
# See GitHub readme for more and updated information.
Type = ""sqlite""
DatabaseName = ""MapChooserSharp.db""
Address = """"
Port = """"
User = """"
Password = """"

GroupInformationTableName = ""McsGroupInformation""
MapInformationTableName = ""McsMapInformation""



[MapCycle]
# Fallback settings for maps with no config
# These settings are ignored when map has a config.

# How many extends allowed if map is not in map config.
FallbackMaxExtends = 3

# How many times allowed to extend a map using !ext command
FallbackMaxExtCommandUses = 1

# How long to extend when map is extended in time left/ round time based game?
FallbackExtendTimePerExtends = 15

# How long to extend when map is extended in round based game?
FallbackExtendRoundsPerExtends = 5



[MapVote]
# What menu type should be use?
#
# Currently supports:
# - BuiltInHtml
# - Cs2ScreenMenuApi
# - Cs2MenuManagerScreen
#
# See GitHub readme for more and updated information.
MenuType = ""BuiltInHtml""

# How many maps should be appeared in map vote?
# I would recommend to set 5 when you using BuiltInHtml menu
MaxVoteElements = 5

# Should print vote text to everyone?
ShouldPrintVoteToChat = true

# Should print the vote remaining time?
ShouldPrintVoteRemainingTime = true


# What countdown ui type should be use?
#
# Currently supports:
# - None
# - CenterHud
# - CenterAlert
# - CenterHtml
# - Chat
#
# See GitHub readme for more information.
CountdownUiType = ""CenterHud""


[MapVote.Sound]
# Sound setting of map vote
# If you leave value as blank, then no sound will played.


# Path to .vsndevts. file extension should be end with `.vsndevts`
# If you already precached a .vsndevts file in another plugin, then you can leave as blank.
SoundFile = """"


# Initial vote sounds

# This sound will be played when starting initial vote countdown
InitialVoteCountdownStartSound = """"

# This sound will be played when starting initial vote
InitialVoteStartSound = """"

# This sound will be played when finishing initial vote (This sound will not be played when runoff vote starts)
InitialVoteFinishSound = """"

# Vote countdown sound mapped to its seconds
InitialVoteCountdownSound1 = """"
InitialVoteCountdownSound2 = """"
InitialVoteCountdownSound3 = """"
InitialVoteCountdownSound4 = """"
InitialVoteCountdownSound5 = """"
InitialVoteCountdownSound6 = """"
InitialVoteCountdownSound7 = """"
InitialVoteCountdownSound8 = """"
InitialVoteCountdownSound9 = """"
InitialVoteCountdownSound10 = """"


# Runoff vote sounds

# This sound will be played when starting runoff vote countdown
RunoffVoteCountdownStartSound = """"

# This sound will be played when starting runoff vote
RunoffVoteStartSound = """"

# This sound will be played when finishing runoff vote
RunoffVoteFinishSound = """"


# Runoff vote countdown sound mapped to its seconds
RunoffVoteCountdownSound1 = """"
RunoffVoteCountdownSound2 = """"
RunoffVoteCountdownSound3 = """"
RunoffVoteCountdownSound4 = """"
RunoffVoteCountdownSound5 = """"
RunoffVoteCountdownSound6 = """"
RunoffVoteCountdownSound7 = """"
RunoffVoteCountdownSound8 = """"
RunoffVoteCountdownSound9 = """"
RunoffVoteCountdownSound10 = """"



[Nomination]
# What menu type should be use?
#
# Currently supports:
# - BuiltInHtml
# - Cs2ScreenMenuApi
# - Cs2MenuManagerScreen
#
# See GitHub readme for more and updated information.
MenuType = ""BuiltInHtml""


[ConfigInformation]
Version = ""{PluginConstants.PluginVersion.ToString()}""
";
        
        File.WriteAllText(ConfigPath, defaultConfig);
    }
}