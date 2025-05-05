using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.DebugCommands;

internal sealed class McsDebugCommands(IServiceProvider serviceProvider): PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsDebugCommands";
    public override string ModuleChatPrefix => $" {ChatColors.Purple}[MCS DEBUG]{ChatColors.Default}";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = null!;
    private IMcsPluginConfigProvider _mcsPluginConfigProvider = null!;
    private IMcsInternalMapVoteControllerApi _mcsMapVoteController = null!;
    
    protected override void OnAllPluginsLoaded()
    {
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _mcsMapVoteController = ServiceProvider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
        _mcsPluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        
        Plugin.AddCommand("mcs_maplist", "", CommandMapList);
        Plugin.AddCommand("mcs_mapinfo", "", CommandMapInfo);
        Plugin.AddCommand("mcs_startvote", "test", CommandStartVote);
        Plugin.AddCommand("mcs_removevote", "test", CommandTestRemoveVote);
        Plugin.AddCommand("mcs_state", "test", CommandTestCurrentState);
        Plugin.AddCommand("mcs_pluginconf", "", CommandTestPluginConfing);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("mcs_maplist", CommandMapList);
        Plugin.RemoveCommand("mcs_mapinfo", CommandMapInfo);
        Plugin.RemoveCommand("mcs_startvote", CommandStartVote);
        Plugin.RemoveCommand("mcs_removevote", CommandTestRemoveVote);
        Plugin.RemoveCommand("mcs_state", CommandTestCurrentState);
        Plugin.RemoveCommand("mcs_pluginconf", CommandTestPluginConfing);
    }

    private void CommandMapList(CCSPlayerController? client, CommandInfo info)
    {
        info.ReplyToCommand("======= maps =======");

        foreach (var (key, value) in _mcsInternalMapConfigProviderApi.GetMapConfigs())
        {
            info.ReplyToCommand($"Name: {value.MapName} | Alias: {value.MapNameAlias} ");
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

        var mapCfg = _mcsInternalMapConfigProviderApi.GetMapConfig(info.ArgByIndex(1));

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
                info.ReplyToCommand($"Hash by map: {setting.GetHashCode()} | Hash by group settings: {_mcsInternalMapConfigProviderApi.GetGroupSettings()[setting.GroupName].GetHashCode()}");
                info.ReplyToCommand($"Group {setting.GroupName}'s cooldown Remains: {setting.GroupCooldown.CurrentCooldown}");
            }
        }
        else
        {
            info.ReplyToCommand("None");
        }
    }
    
    

    private void CommandStartVote(CCSPlayerController? player, CommandInfo info)
    {
        _mcsMapVoteController.InitiateVote();
    }

    private void CommandTestCurrentState(CCSPlayerController? player, CommandInfo info)
    {
        info.ReplyToCommand($"{_mcsMapVoteController.CurrentVoteState}");
    }
    
    private void CommandTestRemoveVote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;
        
        _mcsMapVoteController.RemovePlayerVote(player.Slot);
    }


    private void CommandTestPluginConfing(CCSPlayerController? player, CommandInfo info)
    {
        var config = _mcsPluginConfigProvider.PluginConfig;
        
        info.ReplyToCommand("=============== MAP CYCLE SETTINGS ===============");
        info.ReplyToCommand($"Fallback max extends: {config.MapCycleConfig.FallbackDefaultMaxExtends}");
        info.ReplyToCommand($"Fallback extend time: {config.MapCycleConfig.FallbackExtendTimePerExtends}");
        info.ReplyToCommand($"Fallback extend rounds: {config.MapCycleConfig.FallbackExtendRoundsPerExtends}");
        
        info.ReplyToCommand("=============== NOMINATION SETTINGS ===============");
        info.ReplyToCommand($"Avaiable menu types: {string.Join(", ", config.VoteConfig.AvailableMenuTypes)}");
        info.ReplyToCommand($"Server menu type: {config.VoteConfig.CurrentMenuType}");
        
        info.ReplyToCommand("=============== VOTE SETTINGS ===============");
        info.ReplyToCommand($"Avaiable menu types: {string.Join(", ", config.NominationConfig.AvailableMenuTypes)}");
        info.ReplyToCommand($"Server menu type: {config.NominationConfig.CurrentMenuType}");
    }
}