using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.API.Events.Commands;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.MapCycle.Services;
using MapChooserSharp.Modules.McsDatabase.Interfaces;
using MapChooserSharp.Modules.Nomination;
using MapChooserSharp.Modules.Nomination.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;
using ZLinq;

namespace MapChooserSharp.Modules.MapCycle;

internal sealed class McsMapCycleCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsMapCycleCommands";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
    private IMcsInternalMapCycleControllerApi _mapCycleController = null!;
    private IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = null!;
    private ITimeLeftUtil _timeLeftUtil = null!;
    private IMcsDatabaseProvider _mcsDatabaseProvider = null!;
    private IMcsInternalEventManager _mcsInternalEventManager = null!;
    private IMcsInternalNominationApi _mcsInternalNominationApi = null!;
    private McsMapConfigExecutionService _mcsMapConfigExecutionService = null!;


    protected override void OnAllPluginsLoaded()
    {
        _mapCycleController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        _mcsDatabaseProvider = ServiceProvider.GetRequiredService<IMcsDatabaseProvider>();
        _mcsInternalEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _mcsInternalNominationApi = ServiceProvider.GetRequiredService<IMcsInternalNominationApi>();
        _mcsMapConfigExecutionService = ServiceProvider.GetRequiredService<McsMapConfigExecutionService>();
        
        Plugin.AddCommand("css_reloadmapcfgs", "Reload Map ConVar Configurations", CommandReloadMapConfigs);
        
        Plugin.AddCommand("css_timeleft", "Show timeleft", CommandTimeLeft);
        Plugin.AddCommand("css_nextmap", "Show next map", CommandNextMap);
        Plugin.AddCommand("css_currentmap", "Show current map", CommandCurrentMap);
        
        Plugin.AddCommand("css_setnextmap", "Set next map", CommandSetNextMap);
        Plugin.AddCommand("css_removenextmap", "Remove next map", CommandRemoveNextMap);
        
        Plugin.AddCommand("css_mapinfo", "Show current map's information if available", CommandMapInfo);
        Plugin.AddCommand("css_extends", "Shows remaining extends", CommandExtendsLeft);
        
        Plugin.AddCommand("css_setmapcooldown", "Set specified map's cooldown", CommandSetMapCooldown);
        Plugin.AddCommand("css_setgroupcooldown", "Set specified group's cooldown", CommandSetGroupCooldown);
        
        Plugin.AddCommandListener("say", SayCommandListener, HookMode.Pre);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_reloadmapcfgs", CommandReloadMapConfigs);
        
        Plugin.RemoveCommand("css_timeleft", CommandTimeLeft);
        Plugin.RemoveCommand("css_nextmap", CommandNextMap);
        Plugin.RemoveCommand("css_currentmap", CommandCurrentMap);
        
        Plugin.RemoveCommand("css_setnextmap", CommandSetNextMap);
        Plugin.RemoveCommand("css_removenextmap", CommandRemoveNextMap);
        
        Plugin.RemoveCommand("css_mapinfo", CommandMapInfo);
        Plugin.RemoveCommand("css_extends", CommandExtendsLeft);
        
        Plugin.RemoveCommand("css_setmapcooldown", CommandSetMapCooldown);
        Plugin.RemoveCommand("css_setgroupcooldown", CommandSetGroupCooldown);
        
        Plugin.RemoveCommandListener("say", SayCommandListener, HookMode.Pre);
    }

    [RequiresPermissions(@"css/root")]
    private void CommandReloadMapConfigs(CCSPlayerController? player, CommandInfo info)
    {
        if (_mcsMapConfigExecutionService.IsConfigsAreReloading)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.MapConfigReload.ExecutingInProgress"));
            return;
        }
        
        Logger.LogInformation($"Admin: {PlayerUtil.GetPlayerName(player)} has started config reload task.");

        Task.Run(() =>
        {
            Server.NextFrame(() =>
            {
                PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.MapConfigReload.Start"));
            });
            _mcsMapConfigExecutionService.UpdateMapConfigs();
        }).ContinueWith(task =>
        {
           Server.NextFrame(() =>
           {
               if (task.IsFaulted)
               {
                   PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.MapConfigReload.Failure", task.Exception.Message));
               }
               else
               {
                   PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.MapConfigReload.Success"));
               }
           });
        });
    }

    private HookResult SayCommandListener(CCSPlayerController? player, CommandInfo info)
    {
        if(player == null)
            return HookResult.Continue;

        if (info.ArgCount < 2)
            return HookResult.Continue;
        
        string arg1 = info.ArgByIndex(1);

        bool commandFound = false;


        if (arg1.Equals("timeleft", StringComparison.OrdinalIgnoreCase))
        {
            player.ExecuteClientCommandFromServer("css_timeleft");
            commandFound = true;
        }
        else if (arg1.Equals("nextmap", StringComparison.OrdinalIgnoreCase))
        {
            player.ExecuteClientCommandFromServer("css_nextmap");
            commandFound = true;
        }
        else if (arg1.Equals("currentmap", StringComparison.OrdinalIgnoreCase))
        {
            player.ExecuteClientCommandFromServer("css_currentmap");
            commandFound = true;
        }
            
        return commandFound ? HookResult.Handled : HookResult.Continue;
    }
    
    
    private void CommandTimeLeft(CCSPlayerController? player, CommandInfo info)
    {
        switch (_timeLeftUtil.ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                string timeleft = _timeLeftUtil.GetFormattedTimeLeft(_timeLeftUtil.TimeLimit, player);
                PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.TimeLeft", timeleft));
                break;

            case McsMapExtendType.RoundTime:
                string roundTimeLeft = _timeLeftUtil.GetFormattedTimeLeft(_timeLeftUtil.RoundTimeLeft, player);
                PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.TimeLeft", roundTimeLeft));
                break;
            
            case McsMapExtendType.Rounds:
                string roundsLeft = _timeLeftUtil.GetFormattedRoundsLeft(_timeLeftUtil.RoundsLeft, player);
                PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.RoundLeft", roundsLeft));
                break;
        }
    }


    private void CommandNextMap(CCSPlayerController? player, CommandInfo info)
    {
        var nextMap = _mapCycleController.NextMap;
        if (nextMap != null)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.NextMap", _mcsInternalMapConfigProviderApi.GetMapName(nextMap)));
        }
        else
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.NextMap", LocalizeString(player, "Word.VotePending")));
        }
    }

    [RequiresPermissions(@"css/root")]
    private void CommandSetNextMap(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.SetNextMap.Usage"));
            return;
        }
        
        string mapName = info.ArgByIndex(1);
        
        IMapConfig? newNextMap = _mcsInternalMapConfigProviderApi.GetMapConfig(mapName);

        if (newNextMap == null)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "General.Notification.MapNotFound", mapName));
            return;
        }
        
        
        var previousNextMapConfig = _mapCycleController.NextMap;

        string executorName = PlayerUtil.GetPlayerName(player);
        
        
        if (!_mapCycleController.SetNextMap(newNextMap!))
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.SetNextMap.Failed", _mcsInternalMapConfigProviderApi.GetMapName(newNextMap)));
            return;
        }
        
        
        if (previousNextMapConfig != null)
        {
            PrintLocalizedChatToAll("MapCycle.Broadcast.Admin.ChangedNextMap", executorName, _mcsInternalMapConfigProviderApi.GetMapName(newNextMap));
        }
        else
        {
            PrintLocalizedChatToAll("MapCycle.Broadcast.Admin.SetNextMap", executorName, _mcsInternalMapConfigProviderApi.GetMapName(newNextMap));
        }
        
        Logger.LogInformation($"Admin {executorName} is set next map to {newNextMap.MapName} (Workshop ID: {newNextMap.WorkshopId})");
    }
    
    [RequiresPermissions(@"css/root")]
    private void CommandRemoveNextMap(CCSPlayerController? player, CommandInfo info)
    {
        if (_mapCycleController.NextMap == null)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.RemoveNextmap.NextMapIsNotSet"));
            return;
        }

        string nextMapName = _mcsInternalMapConfigProviderApi.GetMapName(_mapCycleController.NextMap);
        string nextMapActualName = _mapCycleController.NextMap.MapName;
        
        _mapCycleController.RemoveNextMap();
        
        string executorName = PlayerUtil.GetPlayerName(player);
        
        PrintLocalizedChatToAll("MapCycle.Command.Admin.Broadcast.RemoveNextmap.Removed", executorName, nextMapName);
        Logger.LogInformation($"Admin {executorName} removed {nextMapActualName} from next map");
    }
    
    private void CommandCurrentMap(CCSPlayerController? player, CommandInfo info)
    {
        var currentMap = _mapCycleController.CurrentMap;
        if (currentMap != null)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.CurrentMap", _mcsInternalMapConfigProviderApi.GetMapName(currentMap)));
        }
        else
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.CurrentMap", Server.MapName));
        }
    }

    private void CommandMapInfo(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        IMapConfig? mapConfig;


        if (info.ArgCount < 2)
        {
            mapConfig = _mapCycleController.CurrentMap;
        }
        else
        {
            mapConfig = _mcsInternalMapConfigProviderApi.GetMapConfig(info.ArgByIndex(1));
        }
        
        if (mapConfig == null)
        {
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.NotAvailable"));
            return;
        }
        
        
        
        player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo", mapConfig.MapName));
        
        if (mapConfig.MapNameAlias != String.Empty)
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.AliasName", mapConfig.MapNameAlias));
        
        if (mapConfig.MapDescription != String.Empty)
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.Description", mapConfig.MapDescription));
        
        if (mapConfig.MaxExtends > 0)
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.MaxExtends", mapConfig.MaxExtends));
        
        if (mapConfig.WorkshopId > 0)
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.WorkshopId", mapConfig.WorkshopId));
        
        if (mapConfig.NominationConfig.DaysAllowed.Any())
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.DaysAllowed", string.Join(", ", mapConfig.NominationConfig.DaysAllowed)));
        
        if (mapConfig.NominationConfig.AllowedTimeRanges.Any())
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.AllowedTimeRanges", string.Join(", ", mapConfig.NominationConfig.AllowedTimeRanges)));

        if (mapConfig.NominationConfig.MaxPlayers > 0)
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.MaxPlayers", mapConfig.NominationConfig.MaxPlayers));

        if (mapConfig.NominationConfig.MinPlayers > 0)
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.MinPlayers", mapConfig.NominationConfig.MinPlayers));

        if (mapConfig.MapCooldown.CurrentCooldown > 0 ||
            mapConfig.GroupSettings.Any(g => g.GroupCooldown.CurrentCooldown > 0))
        {
            var gorupCooldowns = mapConfig.GroupSettings.Where(g => g.GroupCooldown.CurrentCooldown > 0).ToList();
            var maxGroupCooldown = gorupCooldowns.Any() 
                ? gorupCooldowns.Max(g => g.GroupCooldown.CurrentCooldown) 
                : 0;
            
            int cooldown = Math.Max(mapConfig.MapCooldown.CurrentCooldown, maxGroupCooldown);
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.Cooldown", cooldown));
        }

        string nominationCheckResult = GetNominationCheckReuslt(player, mapConfig);
        player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.MapInfo.YouCanNominate", nominationCheckResult));


        var infoCommandExecutedEvent = new McsMapInfoCommandExecutedEvent(GetTextWithPluginPrefix(player, ""), player, mapConfig);
        _mcsInternalEventManager.FireEventNoResult(infoCommandExecutedEvent);
    }

    private void CommandExtendsLeft(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;
        
        player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.ExtendsLeft", _mapCycleController.ExtendsLeft));
    }

    [RequiresPermissions(@"css/root")]
    private void CommandSetMapCooldown(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 3)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.SetMapCooldown.Usage"));
            return;
        }

        var mapConfig = _mcsInternalMapConfigProviderApi.GetMapConfig(info.ArgByIndex(1));

        if (mapConfig == null)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "General.Notification.MapNotFound"));
            return;
        }

        if (!int.TryParse(info.ArgByIndex(2), out int cooldown))
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "General.Notification.InvalidArgument.WithParam", info.ArgByIndex(2)));
            return;
        }
        
        string executorName = PlayerUtil.GetPlayerName(player);

        Task.Run(async () =>
        {
            bool isOperationSucceeded = false;
            try
            {
                await _mcsDatabaseProvider.MapInfoRepository.UpsertMapCooldownAsync(mapConfig.MapName, cooldown);
                mapConfig.MapCooldown.CurrentCooldown = cooldown;
                isOperationSucceeded = true;
            }
            catch (Exception)
            {
                // ignored
            }
            
            
            await Server.NextFrameAsync(() =>
            {
                if (isOperationSucceeded)
                {
                    PrintLocalizedChatToAll("MapCycle.Command.Admin.Broadcast.SetMapCooldown.Updated", executorName, mapConfig.MapName, cooldown);
                    Logger.LogInformation($"Admin {executorName} is updated {mapConfig.MapName} map cooldown to {cooldown} (Workshop ID: {mapConfig.WorkshopId})");
                }
                else
                {
                    PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.SetMapCooldown.Failed.NoDatabaseConnection"));
                    Logger.LogInformation($"Admin {executorName} is tried to update map {mapConfig.MapName} cooldown to {cooldown}, but failed to connect to database.");
                }
            });
        });
    }

    [RequiresPermissions(@"css/root")]
    private void CommandSetGroupCooldown(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 3)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.SetGroupCooldown.Usage"));
            return;
        }

        var groupSettings = _mcsInternalMapConfigProviderApi.GetGroupSettings();

        var setting = groupSettings.Where(gs => gs.Value.GroupName == info.ArgByIndex(1)).ToDictionary();

        if (setting.Count == 0)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "General.Notification.GroupNotFound"));
            return;
        }

        if (!int.TryParse(info.ArgByIndex(2), out int cooldown))
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "General.Notification.InvalidArgument.WithParam", info.ArgByIndex(2)));
            return;
        }
        
        IMapGroupSettings groupSetting = setting.Values.First();
        
        string executorName = PlayerUtil.GetPlayerName(player);

        Task.Run(async () =>
        {
            bool isOperationSucceeded = false;
            try
            {
                await _mcsDatabaseProvider.GroupInfoRepository.UpsertGroupCooldownAsync(groupSetting.GroupName, cooldown);
                groupSetting.GroupCooldown.CurrentCooldown = cooldown;
                isOperationSucceeded = true;
            }
            catch (Exception)
            {
                // ignored
            }
            
            
            await Server.NextFrameAsync(() =>
            {
                if (isOperationSucceeded)
                {
                    PrintLocalizedChatToAll("MapCycle.Command.Admin.Broadcast.SetGroupCooldown.Updated", executorName, groupSetting.GroupName, cooldown);
                    Logger.LogInformation($"Admin {executorName} is updated {groupSetting.GroupName} group cooldown to {cooldown}");
                }
                else
                {
                    PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Admin.Notification.SetGroupCooldown.Failed.NoDatabaseConnection"));
                    Logger.LogInformation($"Admin {executorName} is tried to update group {groupSetting.GroupName} cooldown to {cooldown}, but failed to connect to database.");
                }
            });
        });
    }


    private string GetNominationCheckReuslt(CCSPlayerController player, IMapConfig mapConfig)
    {
        

        var nominationCheck = _mcsInternalNominationApi.PlayerCanNominateMap(player, mapConfig);

        string canNominate = nominationCheck == McsMapNominationController.NominationCheck.Success
            ? LocalizeString(player, "Word.Yes")
            : LocalizeString(player, "Word.No");
        
        switch (nominationCheck)
        {
            case McsMapNominationController.NominationCheck.Success:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.Success")}";
            
            case McsMapNominationController.NominationCheck.Failed:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.Failed")}";
            
            case McsMapNominationController.NominationCheck.Disabled:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.Disabled")}";
            
            case McsMapNominationController.NominationCheck.NotEnoughPermissions:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.NotEnoughPermissions", string.Join(", ", mapConfig.NominationConfig.RequiredPermissions))}";
            
            case McsMapNominationController.NominationCheck.TooMuchPlayers:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.TooMuchPlayers")}";
            
            case McsMapNominationController.NominationCheck.NotEnoughPlayers:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.NotEnoughPlayers")}";
            
            case McsMapNominationController.NominationCheck.NotAllowed:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.NotAllowed")}";
            
            case McsMapNominationController.NominationCheck.DisabledAtThisTime:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.DisabledAtThisTime")}";
            
            case McsMapNominationController.NominationCheck.OnlySpecificDay:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.OnlySpecificDay")}";
            
            case McsMapNominationController.NominationCheck.OnlySpecificTime:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.OnlySpecificTime")}";
            
            case McsMapNominationController.NominationCheck.MapIsInCooldown:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.MapIsInCooldown")}";
            
            case McsMapNominationController.NominationCheck.AlreadyNominated:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.AlreadyNominated")}";
            
            case McsMapNominationController.NominationCheck.NominatedByAdmin:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.NominatedByAdmin")}";
            
            case McsMapNominationController.NominationCheck.SameMap:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.SameMap")}";
            
            case McsMapNominationController.NominationCheck.GroupNominationLimitReached:
                return $"{canNominate} {LocalizeString(player, "Word.MapInfo.NominationCheck.GroupNominationLimitReached")}";
            
            default:
                return "Error";
        }
    }
}