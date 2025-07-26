using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.Modules.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Other;

namespace MapChooserSharp.Modules.AutoFixMapWorkshopId;

internal class McsAutoFixMapWorkshopId(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsAutoFixMapWorkshopId";
    public override string ModuleChatPrefix => $" {ChatColors.Purple}[MCS AF]{ChatColors.Default}";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private IMcsPluginConfigProvider _configProvider = null!;
    private IMcsInternalMapConfigProviderApi _mapConfigProvider = null!;
    private MapConfigRepository _mapConfigRepository = null!;

    protected override void OnAllPluginsLoaded()
    {
        _configProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _mapConfigRepository = ServiceProvider.GetRequiredService<MapConfigRepository>();

        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    protected override void OnUnloadModule()
    {
    }

    private void OnMapStart(string mapName)
    {
        if (!_configProvider.PluginConfig.GeneralConfig.ShouldAutoFixMapName)
        {
            Logger.LogDebug("[MCS AF] Auto fix map name is disabled. Skipping.");
            return;
        }

        FixMapName(mapName);
    }

    private void FixMapName(string mapName)
    {
        try
        {
            string? workshopId = ForceFullUpdate.GetWorkshopId();

            if (string.IsNullOrEmpty(workshopId))
            {
                Logger.LogDebug("[MCS AF] Current map is not from workshop. Skipping.");
                return;
            }

            Logger.LogInformation($"[MCS AF] Current map is from workshop. Map: {mapName}, Workshop ID: {workshopId}");

            var mapConfig = _mapConfigProvider.GetMapConfig(long.Parse(workshopId));
            if (mapConfig == null)
            {
                Logger.LogWarning($"[MCS AF] No map config found for workshop ID: {workshopId}. Skipping.");
                return;
            }

            if (mapConfig.MapName == mapName)
            {
                Logger.LogDebug($"[MCS AF] Map name in config already matches the actual map name: {mapName}. Skipping.");
                return;
            }

            Logger.LogInformation($"[MCS AF] Updating map name in config from {mapConfig.MapName} to {mapName}");

            string configDir = Path.Combine(Plugin.ModuleDirectory, "config");
            string mapsDir = Path.Combine(configDir, "maps");
            string oldFilePath = Path.Combine(mapsDir, $"{mapConfig.MapName}.toml");
            string newFilePath = Path.Combine(mapsDir, $"{mapName}.toml");

            if (!File.Exists(oldFilePath))
            {
                Logger.LogWarning($"[MCS AF] Map config file not found: {oldFilePath}. Skipping.");
                return;
            }

            string tomlContent = File.ReadAllText(oldFilePath);

            File.WriteAllText(newFilePath, tomlContent);

            if (oldFilePath != newFilePath)
            {
                File.Delete(oldFilePath);
                Logger.LogInformation($"[MCS AF] Renamed map config file from {oldFilePath} to {newFilePath}");
            }

            _mapConfigRepository.ReloadMapConfiguration();
            Logger.LogInformation("[MCS AF] Map configurations reloaded successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[MCS AF] Error fixing map name: {ex.Message}");
            Logger.LogError($"[MCS AF] StackTrace: {ex.StackTrace}");
        }
    }
}
