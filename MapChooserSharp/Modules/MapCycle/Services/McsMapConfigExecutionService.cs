using System.Collections.Concurrent;
using System.Diagnostics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using ZLinq;

namespace MapChooserSharp.Modules.MapCycle.Services;

public sealed class McsMapConfigExecutionService(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsMapConfigExecutionService";
    public override string ModuleChatPrefix => "";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
    private IMcsInternalMapCycleControllerApi _mcsMapCycleController = null!;
    private IMcsPluginConfigProvider _mcsPluginConfigProvider = null!;

    // K,V | configName(without extension), config path(relative path from game/csgo/cfg/)
    private ConcurrentDictionary<string, string> _groupConfigs = new(StringComparer.OrdinalIgnoreCase);
    private ConcurrentDictionary<string, string> _mapConfigs = new(StringComparer.OrdinalIgnoreCase);

    private static string _fixedCs2CfgDirectoryLocation = null!;
    
    // For avoid calling Server.GameDirectory from non-main thread.
    private static string _ServerGameDirectory = null!;

    internal bool IsConfigsAreReloading { get; private set; }
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsMapCycleController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        _mcsPluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        
        _ServerGameDirectory = Server.GameDirectory;
        _fixedCs2CfgDirectoryLocation = Path.Combine(_ServerGameDirectory, "csgo/cfg/");
        Task.Run(UpdateMapConfigs);
    }

    /// <summary>
    /// This method is should not called from main thread. otherwise it will block a lot of time
    /// </summary>
    internal void UpdateMapConfigs()
    {
        IsConfigsAreReloading = true;
        ReloadMapConfigs();
        ReloadGroupConfigs();
        IsConfigsAreReloading = false;
    }
    
    /// <summary>
    /// This method should be called after McsMapCycleController::OnMapStart
    /// </summary>
    internal void ExecuteMapConfigs()
    {
        Task.Run(() =>
        {
            IMapConfig? mapConfig = _mcsMapCycleController.CurrentMap;
    
            DebugLogger.LogInformation($"[{PluginModuleName}] Start executing map configs");
            var mapCfgsPath = FindCfgsWithFilter(mapConfig, _mcsPluginConfigProvider.PluginConfig.MapCycleConfig.MapConfigExecutionType);
            // We are currently using exact match to avoid possible config conflicts.
            var groupCfgsPath = FindCfgsWithFilter(mapConfig, mcsMapConfigType: McsMapConfigType.Group);
            
            Server.NextFrame(() =>
            {
                foreach (var mapCfgPath in mapCfgsPath)
                {
                    Server.ExecuteCommand($"exec {mapCfgPath}");
                }
                foreach (string groupCfgPath in groupCfgsPath)
                {
                    Server.ExecuteCommand($"exec {groupCfgPath}");
                }
            });
            DebugLogger.LogInformation($"[{PluginModuleName}] Execution done");
        }).ConfigureAwait(false);
    }

    private List<string> FindCfgsWithFilter(IMapConfig? mapConfig, McsMapConfigExecutionType filter = McsMapConfigExecutionType.ExactMatch, McsMapConfigType mcsMapConfigType = McsMapConfigType.Map)
    {
        List<string> possibleConfigNames = new();
        if (mapConfig == null)
        {
            possibleConfigNames.Add(Server.MapName);
        }
        else
        {
            if (mcsMapConfigType == McsMapConfigType.Map)
            {
                possibleConfigNames.Add(mapConfig.MapName);
            }
            else
            {
                foreach (IMapGroupSettings settings in mapConfig.GroupSettings)
                {
                    possibleConfigNames.Add(settings.GroupName);
                }
            }
        }
        
        List<string> cfgs = new();

        switch (filter)
        {
            case McsMapConfigExecutionType.ExactMatch:
                if (mcsMapConfigType == McsMapConfigType.Map)
                {
                    if (_mapConfigs.TryGetValue(possibleConfigNames.First(), out string? mapCfgLocation))
                    {
                        cfgs.Add(mapCfgLocation);
                    }
                }
                else
                {
                    foreach (string configName in possibleConfigNames)
                    {
                        if (!_groupConfigs.TryGetValue(configName, out string? groupCfgLocation))
                            continue;
                        
                        cfgs.Add(groupCfgLocation);
                    }
                }
                break;
            
            case McsMapConfigExecutionType.StartWithMach:
                if (mcsMapConfigType == McsMapConfigType.Map)
                {
                    cfgs.AddRange(_mapConfigs.Where(m => possibleConfigNames.First().StartsWith(m.Key)).Select(m => m.Value).ToList());
                }
                else
                {
                    throw new InvalidOperationException("StartWithMach is not supported for group configs.");
                }
                break;
            
            case McsMapConfigExecutionType.PartialMatch:
                if (mcsMapConfigType == McsMapConfigType.Map)
                {
                    cfgs.AddRange(_mapConfigs.Where(m => possibleConfigNames.First().Contains(m.Key)).Select(m => m.Value).ToList());
                }
                else
                {
                    throw new InvalidOperationException("PartialMatch is not supported for group configs.");
                }
                break;
        }
        
        return cfgs;
    }


    private void ReloadMapConfigs()
    {
        DebugLogger.LogInformation($"[{PluginModuleName}] Start reloading map configs");
        string configDirectory =  Path.Combine(_fixedCs2CfgDirectoryLocation, _mcsPluginConfigProvider.PluginConfig.MapCycleConfig.MapConfigDirectoryPath);
        GetConfigsFromDirectory(configDirectory, "*.cfg", _mapConfigs);
    }

    private void ReloadGroupConfigs()
    {
        DebugLogger.LogInformation($"[{PluginModuleName}] Start reloading group configs");
        string configDirectory =  Path.Combine(_fixedCs2CfgDirectoryLocation, _mcsPluginConfigProvider.PluginConfig.MapCycleConfig.GroupConfigDirectoryPath);
        GetConfigsFromDirectory(configDirectory, "*.cfg", _groupConfigs);
    }
    
    
    
    private void GetConfigsFromDirectory(string configsDirectory, string searchPattern, ConcurrentDictionary<string, string> updateTarget, SearchOption searchOption = SearchOption.AllDirectories)
    {
        if (!CheckDirectoryExists(configsDirectory))
        {
            Logger.LogError("Failed to find directory and MCS cannot be update config data!");
            return;
        }
        
        updateTarget.Clear();
        
        DebugLogger.LogDebug("[MCS][Map Config] Get files from directory and update dictionary");
        string[] files = Directory.GetFiles(configsDirectory, searchPattern, searchOption);
        
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);

            string relativePath =
                Path.GetRelativePath(Path.GetFullPath(Path.Combine(_ServerGameDirectory, "csgo/cfg/")), file);

            if (relativePath.StartsWith(Path.Combine(configsDirectory, "disabled")))
            {
                Logger.LogInformation($"{fileName} was skipped because located in disabled folder.");
                continue;
            }

            updateTarget[fileName.Substring(0, fileName.IndexOf(".cfg", StringComparison.OrdinalIgnoreCase))] = relativePath;
        }
        DebugLogger.LogDebug("[MCS][Map Config] Done");
    }

    private bool CheckDirectoryExists(string configDirectory)
    {
        if (!Directory.Exists(configDirectory))
        {
            try
            {
                Logger.LogWarning($"Map config folder {configDirectory} is not exists. Trying to create...");
                Directory.CreateDirectory(configDirectory);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to create map config folder!");
                return false;
            }
        }

        return true;
    }

    private enum McsMapConfigType
    {
        Map,
        Group,
    }
}