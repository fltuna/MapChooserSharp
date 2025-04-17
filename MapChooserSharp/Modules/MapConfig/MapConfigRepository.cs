﻿using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapConfig;

public sealed class MapConfigRepository(IServiceProvider serviceProvider): PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "MapConfigRepository";
    public override string ModuleChatPrefix => "MapConfigRepository";

    private IMapConfigProvider _mapConfigProvider = null!;
    
    private string _mapConfigLocation = null!;
    
    
    protected override void OnInitialize()
    {
        Plugin.AddCommand("mcs_maplist", "", CommandMapList);
        Plugin.AddCommand("mcs_mapinfo", "", CommandMapInfo);
        ReloadMapConfiguration();
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("mcs_maplist", CommandMapList);
        Plugin.RemoveCommand("mcs_mapinfo", CommandMapInfo);
    }

    private void ReloadMapConfiguration()
    {
        _mapConfigLocation = Path.Combine(Plugin.ModuleDirectory, "config", "maps.toml");
        _mapConfigProvider = new MapConfigParser(_mapConfigLocation).Load();
    }


    private void CommandMapList(CCSPlayerController? client, CommandInfo info)
    {
        info.ReplyToCommand("======= maps =======");

        foreach (var (key, value) in _mapConfigProvider.GetMapConfigs())
        {
            info.ReplyToCommand($"{value.MapName}");
            foreach (IMapGroupSettings settings in value.GroupSettings)
            {
                info.ReplyToCommand($"{settings.GroupName}");
            }
            foreach (string perm in value.NominationConfig.RequiredPermissions)
            {
                info.ReplyToCommand($"Required permission: {perm}");
            }
        }
    }


    [RequiresPermissions(@"css/map")]
    private void CommandMapInfo(CCSPlayerController? client, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            info.ReplyToCommand("Usage: mcs_mapinfo <map name>");
            return;
        }

        var mapCfg = _mapConfigProvider.GetMapConfig(info.ArgByIndex(1));

        if (mapCfg == null)
        {
            info.ReplyToCommand("Map config not found");
            return;
        }
        
        
        info.ReplyToCommand("=============================================");
        info.ReplyToCommand($"Map information of {mapCfg.MapName} | {mapCfg.MapNameAlias}");
        info.ReplyToCommand("=============================================");
        info.ReplyToCommand($"MapName: {mapCfg.MapName}");
        info.ReplyToCommand($"MapNameAlias: {mapCfg.MapNameAlias}");
        info.ReplyToCommand($"MapDescription: {mapCfg.MapDescription}");
        info.ReplyToCommand($"MapCooldown: {mapCfg.MapCooldown.MapConfigCooldown}");
        info.ReplyToCommand($"CooldownRemains: {mapCfg.MapCooldown.CurrentCooldown}");
        info.ReplyToCommand($"WorkshopId {mapCfg.WorkshopId}");
        info.ReplyToCommand($"OnlyNomination: {mapCfg.OnlyNomination}");
        info.ReplyToCommand($"MapRounds: {mapCfg.MapRounds}");
        info.ReplyToCommand($"MapTime: {mapCfg.MapTime}");
        info.ReplyToCommand($"MaxExtends: {mapCfg.MaxExtends}");
        info.ReplyToCommand($"ExtendRoundsPerExtends: {mapCfg.ExtendRoundsPerExtends}");
        info.ReplyToCommand($"ExtendTimePerExtends: {mapCfg.ExtendTimePerExtends}");
        info.ReplyToCommand($"IsDisabled: {mapCfg.IsDisabled}");
        info.ReplyToCommand("");
        info.ReplyToCommand("=============== NOMINATION SETTINGS ===============");
        info.ReplyToCommand("");
        info.ReplyToCommand($"MaxPlayers: {mapCfg.NominationConfig.MaxPlayers}");
        info.ReplyToCommand($"MinPlayers: {mapCfg.NominationConfig.MinPlayers}");
        info.ReplyToCommand($"RequiredPermissions: {string.Join(", ", mapCfg.NominationConfig.RequiredPermissions)}");
        info.ReplyToCommand($"ProhibitAdminNomination: {mapCfg.NominationConfig.ProhibitAdminNomination}");
        info.ReplyToCommand($"RestrictToAllowedUsersOnly: {mapCfg.NominationConfig.RestrictToAllowedUsersOnly}");
        info.ReplyToCommand($"AllowedSteamIds: {string.Join(", ", mapCfg.NominationConfig.AllowedSteamIds)}");
        info.ReplyToCommand($"DisallowedSteamIds: {string.Join(", ", mapCfg.NominationConfig.DisallowedSteamIds)}");
        


        if (mapCfg.NominationConfig.DaysAllowed.Any())
        {
            StringBuilder sb = new();
            sb.Append("DaysAllowed: ");
            foreach (DayOfWeek day in mapCfg.NominationConfig.DaysAllowed)
            {
                sb.Append($"{day}, ");
            }
            info.ReplyToCommand(sb.ToString());
        }
        else
        {
            info.ReplyToCommand("DaysAllowed: Always");
        }
        


        if (mapCfg.NominationConfig.AllowedTimeRanges.Any())
        {
            StringBuilder sb = new();
            sb.Append("AllowedTimeRanges: ");
            foreach (ITimeRange range in mapCfg.NominationConfig.AllowedTimeRanges)
            {
                sb.Append($"{range.StartTime} - {range.EndTime}, ");
            }
            info.ReplyToCommand(sb.ToString());
        }
        else
        {
            info.ReplyToCommand("AllowedTimeRanges: Always");
        }
        
        info.ReplyToCommand("");
        info.ReplyToCommand("=============== GROUP SETTINGS ===============");
        info.ReplyToCommand("");

        if (mapCfg.GroupSettings.Any())
        {
            foreach (IMapGroupSettings setting in mapCfg.GroupSettings)
            {
                info.ReplyToCommand($"Group {setting.GroupName}'s cooldown: {setting.GroupCooldown.MapConfigCooldown}");
                info.ReplyToCommand($"Group {setting.GroupName}'s cooldown Remains: {setting.GroupCooldown.CurrentCooldown}");
            }
        }
        else
        {
            info.ReplyToCommand("None");
        }
    }
}