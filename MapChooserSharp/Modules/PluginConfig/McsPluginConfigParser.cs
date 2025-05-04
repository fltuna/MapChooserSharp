using CounterStrikeSharp.API;
using MapChooserSharp.Models;
using MapChooserSharp.Modules.McsMenu;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Models;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using Tomlyn;
using Tomlyn.Model;

namespace MapChooserSharp.Modules.PluginConfig;

internal class McsPluginConfigParser(string configPath, IServiceProvider provider): PluginBasicFeatureBase(provider)
{
    private string ConfigPath { get; } = configPath;
    
    private static Version _currentConfigVersion = PluginConstants.PluginVersion;
    
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
        
        int result = configVersion.CompareTo(_currentConfigVersion);

        if (result < 0)
        {
            Logger.LogWarning("Config file version is older than the current version. Some settings may not work correctly.");
            Logger.LogWarning($"Config version: {configVersion} | Current version: {_currentConfigVersion}");
        }
        else if (result > 0)
        {
            Logger.LogWarning("Config file version is newer than the current version. Some settings may not be compatible.");
            Logger.LogWarning($"Config version: {configVersion} | Current version: {_currentConfigVersion}");
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

        return new McsVoteConfig(availableMenus, currentMenuType, (int)maxVoteElementsLong, shouldPrintVoteToChatBool);
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
        
        return new McsGeneralConfig(aliasNameSetting);
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