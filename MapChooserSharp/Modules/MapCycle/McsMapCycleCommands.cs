﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using MapChooserSharp.API.MapCycleController;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Modules.MapCycle;

public sealed class McsMapCycleCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsMapCycleCommands";
    public override string ModuleChatPrefix => "unused";
    
    private IMcsMapCycleControllerApi _mapCycleController = null!;
    
    public int InvalidTimeLeft { get; } = -1;
    
    private ConVar? mp_timelimit = null;

    private int TimeLeft
    {
        get
        {
            var gameRules = EntityUtil.GetGameRules();
            if (gameRules == null)
            {
                Logger.LogError("Failed to find the Game Rules entity!");
                return InvalidTimeLeft;
            }

            if (mp_timelimit == null)
            {
                mp_timelimit = ConVar.Find("mp_timelimit");
                if (mp_timelimit == null)
                {
                    Logger.LogWarning("Failed to find the mp_timelimit ConVar and try to find again.");
                    return InvalidTimeLeft;
                }
            }

            var timeLimit = mp_timelimit.GetPrimitiveValue<float>();
            if (timeLimit < 0.001f)
            {
                return 0;
            }

            return (int)((gameRules.GameStartTime + timeLimit * 60.0f) - Server.CurrentTime);
        }
    }


    protected override void OnInitialize()
    {
        _mapCycleController = ServiceProvider.GetRequiredService<McsMapCycleController>();
        Plugin.AddCommand("css_timeleft", "Show timeleft", CommandTimeLeft);
        Plugin.AddCommand("css_nextmap", "Show next map", CommandNextMap);
        Plugin.AddCommand("css_currentmap", "Show current map", CommandCurrentMap);
        Plugin.AddCommandListener("say", SayCommandListener);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_timeleft", CommandTimeLeft);
        Plugin.RemoveCommand("css_nextmap", CommandNextMap);
        Plugin.RemoveCommand("css_currentmap", CommandCurrentMap);
    }




    private HookResult SayCommandListener(CCSPlayerController? player, CommandInfo info)
    {
        if(player == null)
            return HookResult.Continue;

        if (info.ArgCount < 2)
            return HookResult.Continue;
        
        string arg1 = info.ArgByIndex(1);


        if (arg1.Equals("timeleft", StringComparison.OrdinalIgnoreCase))
        {
            player.ExecuteClientCommandFromServer("css_timeleft");
        }
        else if (arg1.Equals("nextmap", StringComparison.OrdinalIgnoreCase))
        {
            player.ExecuteClientCommandFromServer("css_nextmap");
        }
        else if (arg1.Equals("currentmap", StringComparison.OrdinalIgnoreCase))
        {
            player.ExecuteClientCommandFromServer("css_currentmap");
        }
            
        return HookResult.Continue;
    }
    
    
    private void CommandTimeLeft(CCSPlayerController? player, CommandInfo info)
    {
        info.ReplyToCommand(GetFormattedTimeLeft(TimeLeft, player));
    }


    private void CommandNextMap(CCSPlayerController? player, CommandInfo info)
    {
        var nextMap = _mapCycleController.NextMap;
        if (player == null)
        {
            if (nextMap != null)
            {
                Server.PrintToConsole($"Next map: {nextMap}");
            }
            else
            {
                Server.PrintToConsole("Next map: TODO_TRANSLATE_VOTE_PENDING");
            }
        }
        else
        {
            if (nextMap != null)
            {
                player.PrintToChat($"Next map: {nextMap}");
            }
            else
            {
                player.PrintToChat($"Next map: TODO_TRANSLATE_VOTE_PENDING");
            }
        }
    }
    
    private void CommandCurrentMap(CCSPlayerController? player, CommandInfo info)
    {
        var currentMap = _mapCycleController.CurrentMap;
        if (player == null)
        {
            if (currentMap != null)
            {
                Server.PrintToConsole($"Current map: {currentMap}");
            }
            else
            {
                Server.PrintToConsole("Current map: NULL!");
            }
        }
        else
        {
            if (currentMap != null)
            {
                player.PrintToChat($"Next map: {currentMap}");
            }
            else
            {
                player.PrintToChat($"Current map: NULL!");
            }
        }
    }

    private string GetFormattedTimeLeft(int timeLeft)
    {
        int hours = timeLeft / 3600;
        int minutes = (timeLeft % 3600) / 60;
        int seconds = timeLeft % 60;

        if (hours > 0)
        {
            return $"{hours} {(hours == 1 ? "hour" : "hours")} " +
                   $"{minutes} {(minutes == 1 ? "minute" : "minutes")} " +
                   $"{seconds} {(seconds == 1 ? "second" : "seconds")}";
        }

        if (minutes > 0)
        {
            return $"{minutes} {(minutes == 1 ? "minute" : "minutes")} " +
                   $"{seconds} {(seconds == 1 ? "second" : "seconds")}";
        }

        return $"{seconds} {(seconds == 1 ? "second" : "seconds")}";
    }


    private string GetFormattedTimeLeft(int timeLeft, CCSPlayerController? player)
    {
        SteamID? steamId = player?.AuthorizedSteamID;
        if (steamId == null)
            return GetFormattedTimeLeft(timeLeft);
        
        int hours = timeLeft / 3600;
        int minutes = (timeLeft % 3600) / 60;
        int seconds = timeLeft % 60;

        var playerCulture = PlayerLanguageManager.Instance.GetLanguage(steamId);
        using var tempCulture = new WithTemporaryCulture(playerCulture);

        if (hours > 0)
        {
            return "TODO_TRANSLATE| HOURS";
        }

        if (minutes > 0)
        {
            return "TODO_TRANSLATE| MINUETS";
        }

        return "TODO_TRANSLATE| SECONDS";
    }
    
}