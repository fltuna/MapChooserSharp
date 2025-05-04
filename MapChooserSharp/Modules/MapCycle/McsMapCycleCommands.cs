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
    
    private IMcsMapCycleControllerApi _mapCycleController = null!;
    private IMapConfigProvider _mapConfigProvider = null!;
    private ITimeLeftUtil _timeLeftUtil = null!;


    protected override void OnAllPluginsLoaded()
    {
        _mapCycleController = ServiceProvider.GetRequiredService<McsMapCycleController>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        Plugin.AddCommand("css_timeleft", "Show timeleft", CommandTimeLeft);
        Plugin.AddCommand("css_nextmap", "Show next map", CommandNextMap);
        Plugin.AddCommand("css_setnextmap", "Set next map", CommandSetNextMap);
        Plugin.AddCommand("css_removenextmap", "Remove next map", CommandRemoveNextMap);
        Plugin.AddCommand("css_currentmap", "Show current map", CommandCurrentMap);
        Plugin.AddCommand("css_mapinfo", "Show current map's information if available", CommandMapInfo);
        Plugin.AddCommand("css_extends", "Shows remaining extends", CommandExtendsLeft);
        
        Plugin.AddCommandListener("say", SayCommandListener, HookMode.Pre);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_timeleft", CommandTimeLeft);
        Plugin.RemoveCommand("css_nextmap", CommandNextMap);
        Plugin.RemoveCommand("css_setnextmap", CommandSetNextMap);
        Plugin.RemoveCommand("css_removenextmap", CommandRemoveNextMap);
        Plugin.RemoveCommand("css_currentmap", CommandCurrentMap);
        Plugin.RemoveCommand("css_mapinfo", CommandMapInfo);
        Plugin.RemoveCommand("css_extends", CommandExtendsLeft);
        
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
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Notification.NextMap", _mapConfigProvider.GetMapName(nextMap)));
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
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.NextMap", _mapConfigProvider.GetMapName(nextMap)));
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
        
        IMapConfig? newNextMap = _mapConfigProvider.GetMapConfig(mapName);

        if (newNextMap == null)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.SetNextMap.MapNotFound", mapName));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.SetNextMap.MapNotFound", mapName));
            }
            
            return;
        }
        
        
        var previousNextMapConfig = _mapCycleController.NextMap;

        string executorName = PlayerUtil.GetPlayerName(player);
        
        
        if (!_mapCycleController.SetNextMap(newNextMap!))
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Admin.Notification.SetNextMap.Failed", _mapConfigProvider.GetMapName(newNextMap)));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Admin.Notification.SetNextMap.Failed", _mapConfigProvider.GetMapName(newNextMap)));
            }
            
            return;
        }
        
        
        if (previousNextMapConfig != null)
        {
            PrintLocalizedChatToAll("MapCycle.Broadcast.Admin.ChangedNextMap", executorName, _mapConfigProvider.GetMapName(newNextMap));
        }
        else
        {
            PrintLocalizedChatToAll("MapCycle.Broadcast.Admin.SetNextMap", executorName, _mapConfigProvider.GetMapName(newNextMap));
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
            string nextMapName = _mapConfigProvider.GetMapName(_mapCycleController.NextMap);
            
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
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Notification.CurrentMap", _mapConfigProvider.GetMapName(currentMap)));
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
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.CurrentMap", _mapConfigProvider.GetMapName(currentMap)));
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
}