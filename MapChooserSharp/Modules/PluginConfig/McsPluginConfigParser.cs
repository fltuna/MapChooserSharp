﻿using CounterStrikeSharp.API;
using MapChooserSharp.Models;
using MapChooserSharp.Modules.McsDatabase;
using MapChooserSharp.Modules.McsMenu;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Models;
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
        
        
        
        return new McsMapCycleConfig(defaultMaxExtends, fallbackExtendTimePerExtends, fallbackExtendRoundsPerExtends);
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
        

        if (!voteTable.TryGetValue("MaxVoteElements", out var maxVoteElementsObj) || maxVoteElementsObj is not long maxVoteElementsLong)
        {
            throw new InvalidOperationException("MapVote.MaxVoteElements is not found or invalid");
        }

        if (!voteTable.TryGetValue("ShouldPrintVoteToChat", out var shouldPrintVoteToChatObj) || shouldPrintVoteToChatObj is not bool shouldPrintVoteToChatBool)
        {
            throw new InvalidOperationException("MapVote.ShouldPrintVoteToChat is not found or invalid");
        }
        
        var availableMenus =  _avaiableMenuTypes;

        var currentMenuType = DecideMenuType(menuTypeStr, availableMenus);

        var soundConfig = ParseVoteSoundConfig(voteTable);

        return new McsVoteConfig(availableMenus, currentMenuType, (int)maxVoteElementsLong, shouldPrintVoteToChatBool, soundConfig);
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
            throw new InvalidOperationException("General.ShouldUseAliasMapNameIfAvailable is not found or invalid");
        }

        var sqlConfig = ParseSqlConfig(generalTable);
        
        return new McsGeneralConfig(aliasNameSetting, verboseCooldownPrint, sqlConfig);
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
        
        if (!sqlTable.TryGetValue("Address", out var sqlAddressObj) || sqlAddressObj is not string sqlAddress)
        {
            throw new InvalidOperationException("General.Sql.Address is not found or invalid");
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

        var sqlConfig = new McsSqlConfig(sqlAddress, sqlUser, ref sqlPassword, groupTableName, mapTableName, type);
        
        return sqlConfig;
    }
    
    
    private McsSupportedMenuType DecideMenuType(string menuTypeStr, List<McsSupportedMenuType> availableMenus)
    {
        // if menu type is invalid, fall back to builtin html menu
        if (!Enum.TryParse<McsSupportedMenuType>(menuTypeStr, true, out var currentMenuType))
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
                
                // TODO() Add availability check
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

[MapCycle]
# Fallback settings for maps with no config
# These settings are ignored when map has a config.

# How many extends allowed if map is not in map config.
FallbackMaxExtends = 6

# How long to extend when map is extended in time left/ round time based game?
FallbackExtendTimePerExtends = 15

# How long to extend when map is extended in round based game?
FallbackExtendRoundsPerExtends = 5


[MapVote]
# What menu type should be use?
# See GitHub readme for more information.
MenuType = ""BuiltInHtml""

# How many maps should be appeared in map vote?
# I would recommend to set 5 when you using BuiltInHtml menu
MaxVoteElements = 5

# Should print vote text to everyone?
ShouldPrintVoteToChat = true


[Nomination]
# What menu type should be use?
# See GitHub readme for more information.
MenuType = ""BuiltInHtml""


[ConfigInformation]
Version = ""{PluginConstants.PluginVersion.ToString()}""
";
        
        File.WriteAllText(ConfigPath, defaultConfig);
    }
}