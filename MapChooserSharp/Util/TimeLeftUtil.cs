using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using MapChooserSharp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Util;

internal class TimeLeftUtil(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), ITimeLeftUtil
{
    public override string PluginModuleName => "TimeleftUtil";
    public override string ModuleChatPrefix => "unused";

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ITimeLeftUtil, TimeLeftUtil>();
    }

    private int InvalidTimeLeft { get; } = -1;
    
    private ConVar? mp_timelimit = null;

    public int TimeLeft
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


    public string GetFormattedTimeLeft(int timeLeft)
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


    public string GetFormattedTimeLeft(int timeLeft, CCSPlayerController? player)
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