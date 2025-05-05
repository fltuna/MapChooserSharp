using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

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


    protected override void OnAllPluginsLoaded()
    {
        _mapCycleController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        _mcsDatabaseProvider = ServiceProvider.GetRequiredService<IMcsDatabaseProvider>();
        
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
        // TODO() Support round time and round count
        string timeleft = _timeLeftUtil.GetFormattedTimeLeft(_timeLeftUtil.TimeLimit, player);
        if (player == null)
        {
            Server.PrintToConsole(Plugin.LocalizeString("MapCycle.Command.Notification.TimeLeft", timeleft));
        }
        else
        {
            player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.TimeLeft", timeleft));
        }
    }


    private void CommandNextMap(CCSPlayerController? player, CommandInfo info)
    {
        var nextMap = _mapCycleController.NextMap;
        if (player == null)
        {
            if (nextMap != null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Notification.NextMap", _mcsInternalMapConfigProviderApi.GetMapName(nextMap)));
            }
            else
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Notification.NextMap", LocalizeString("Word.VotePending")));
            }
        }
        else
        {
            if (nextMap != null)
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.NextMap", _mcsInternalMapConfigProviderApi.GetMapName(nextMap)));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.NextMap", LocalizeStringForPlayer(player, "Word.VotePending")));
            }
        }
    }

    [RequiresPermissions(@"css/root")]
    private void CommandSetNextMap(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.SetNextMap.Usage"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.SetNextMap.Usage"));
            }
            
            return;
        }
        
        string mapName = info.ArgByIndex(1);
        
        IMapConfig? newNextMap = _mcsInternalMapConfigProviderApi.GetMapConfig(mapName);

        if (newNextMap == null)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("General.Notification.MapNotFound", mapName));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.MapNotFound", mapName));
            }
            
            return;
        }
        
        
        var previousNextMapConfig = _mapCycleController.NextMap;

        string executorName = PlayerUtil.GetPlayerName(player);
        
        
        if (!_mapCycleController.SetNextMap(newNextMap!))
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.SetNextMap.Failed", _mcsInternalMapConfigProviderApi.GetMapName(newNextMap)));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.SetNextMap.Failed", _mcsInternalMapConfigProviderApi.GetMapName(newNextMap)));
            }
            
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
    }
    
    [RequiresPermissions(@"css/root")]
    private void CommandRemoveNextMap(CCSPlayerController? player, CommandInfo info)
    {
        if (_mapCycleController.NextMap == null)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.RemoveNextmap.NextMapIsNotSet"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.RemoveNextmap.NextMapIsNotSet"));
            }
        }
        else
        {
            string nextMapName = _mcsInternalMapConfigProviderApi.GetMapName(_mapCycleController.NextMap);
            
            _mapCycleController.RemoveNextMap();
            
            string executorName = PlayerUtil.GetPlayerName(player);
            
            PrintLocalizedChatToAll("MapCycle.Command.Admin.Broadcast.RemoveNextmap.Removed", executorName, nextMapName);
        }
    }
    
    private void CommandCurrentMap(CCSPlayerController? player, CommandInfo info)
    {
        var currentMap = _mapCycleController.CurrentMap;
        if (player == null)
        {
            if (currentMap != null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Notification.CurrentMap", _mcsInternalMapConfigProviderApi.GetMapName(currentMap)));
            }
            else
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Notification.CurrentMap", Server.MapName));
            }
        }
        else
        {
            if (currentMap != null)
            {
                // TODO() Use alias name if available 
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.CurrentMap", _mcsInternalMapConfigProviderApi.GetMapName(currentMap)));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.CurrentMap", Server.MapName));
            }
        }
    }

    private void CommandMapInfo(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;
        
        var currentMap = _mapCycleController.CurrentMap;
        
        if (currentMap != null && currentMap.MapDescription != string.Empty)
        {
            player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.MapInfo", currentMap.MapDescription));
        }
        else
        {
            player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.MapInfo.NotAvailable"));
        }
    }

    private void CommandExtendsLeft(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;
        
        player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.ExtendsLeft", _mapCycleController.ExtendsLeft));
    }

    [RequiresPermissions(@"css/root")]
    private void CommandSetMapCooldown(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 3)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.SetMapCooldown.Usage"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.SetMapCooldown.Usage"));
            }
            return;
        }

        var mapConfig = _mcsInternalMapConfigProviderApi.GetMapConfig(info.ArgByIndex(1));

        if (mapConfig == null)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("General.Notification.MapNotFound"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.MapNotFound"));
            }
            return;
        }

        if (!int.TryParse(info.ArgByIndex(2), out int cooldown))
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("General.Notification.InvalidArgument.WithParam", info.ArgByIndex(2)));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.InvalidArgument.WithParam", info.ArgByIndex(2)));
            }
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
                }
                else
                {
                    if (player == null)
                    {
                        Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.SetMapCooldown.Failed.NoDatabaseConnection"));
                    }
                    else
                    {
                        player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.SetMapCooldown.Failed.NoDatabaseConnection"));
                    }
                }
            });
        });
    }

    [RequiresPermissions(@"css/root")]
    private void CommandSetGroupCooldown(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 3)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.SetGroupCooldown.Usage"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.SetGroupCooldown.Usage"));
            }
            return;
        }

        var groupSettings = _mcsInternalMapConfigProviderApi.GetGroupSettings();

        var setting = groupSettings.Where(gs => gs.Value.GroupName == info.ArgByIndex(1)).ToDictionary();

        if (setting.Count == 0)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("General.Notification.GroupNotFound"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.GroupNotFound"));
            }
            return;
        }

        if (!int.TryParse(info.ArgByIndex(2), out int cooldown))
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("General.Notification.InvalidArgument.WithParam", info.ArgByIndex(2)));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.InvalidArgument.WithParam", info.ArgByIndex(2)));
            }
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
                }
                else
                {
                    if (player == null)
                    {
                        Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.SetGroupCooldown.Failed.NoDatabaseConnection"));
                    }
                    else
                    {
                        player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.SetGroupCooldown.Failed.NoDatabaseConnection"));
                    }
                }
            });
        });
    }
}