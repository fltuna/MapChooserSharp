using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Util;

internal sealed class TimeLeftUtil(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider), ITimeLeftUtil
{
    public override string PluginModuleName => "TimeleftUtil";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
    public readonly FakeConVar<int> MapTimeTypeOverride = new("mcs_map_time_type_override",
        "Override map time type. 0 = automatic detection, 1 = mp_timelimit, 2 = mp_maxrounds, 3 = mp_roundtime", 0,
        ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 3));

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ITimeLeftUtil>(this);
    }

    protected override void OnInitialize()
    {
        TrackConVar(MapTimeTypeOverride);
        if (hotReload)
        {
            ReDetermineExtendType();
        }
    }


    public void ReDetermineExtendType()
    {
        ExtendType = DetermineExtendType();
    }

    public McsMapExtendType ExtendType { get; private set; } = McsMapExtendType.TimeLimit;

    private const int InvalidTime = -1;

    #region mp_timelimit
    
    private ConVar? mp_timelimit = null;

    public int TimeLimit
    {
        get
        {
            var gameRules = EntityUtil.GetGameRules();
            if (gameRules == null)
            {
                Logger.LogError("Failed to find the Game Rules entity!");
                return InvalidTime;
            }

            if (mp_timelimit == null)
            {
                mp_timelimit = ConVar.Find("mp_timelimit");
                if (mp_timelimit == null)
                {
                    Logger.LogWarning("Failed to find the mp_timelimit ConVar and try to find again.");
                    return InvalidTime;
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
    
    #endregion
    
    
    #region mp_maxrounds
    
    
    private ConVar? mp_maxrounds = null;

    public int RoundsLeft
    {
        get
        {
            var gameRules = EntityUtil.GetGameRules();
            if (gameRules == null)
            {
                Logger.LogError("Failed to find the Game Rules entity!");
                return InvalidTime;
            }

            if (mp_maxrounds == null)
            {
                mp_maxrounds = ConVar.Find("mp_maxrounds");
                if (mp_maxrounds == null)
                {
                    Logger.LogWarning("Failed to find the mp_maxrounds ConVar and try to find again.");
                    return InvalidTime;
                }
            }

            int maxRounds = mp_maxrounds.GetPrimitiveValue<int>();
            if (maxRounds <= 0)
            {
                return 0;
            }

            return maxRounds - gameRules.TotalRoundsPlayed;
        }
    }


    #endregion
    
    
    #region mp_roundtime
    
    private ConVar? mp_roundtime = null;
    
    /// <summary>
    /// Returns remaining round time in seconds
    /// </summary>
    public int RoundTimeLeft
    {
        get
        {
            var gameRules = EntityUtil.GetGameRules();
            if (gameRules == null)
            {
                Logger.LogError("Failed to find the Game Rules entity!");
                return InvalidTime;
            }
            
            return (int)((gameRules.GameStartTime + gameRules.RoundTime) - Server.CurrentTime);
        }
    }
    
    #endregion

    public bool ExtendTimeLimit(int minutes)
    {
        if (TimeLimit < 1 && MapTimeTypeOverride.Value == 0)
        {
            DebugLogger.LogWarning("TimeLeft util tried to extend a time limit, but looks like current game mode is not a time limit based! aborting...");
            return false;
        }
        
        // Just in case
        mp_timelimit ??= ConVar.Find("mp_timelimit");

        if (mp_timelimit == null)
        {
            DebugLogger.LogWarning("Failed to find the mp_timelimit ConVar.");
            return false;
        }

        float newTime = mp_timelimit.GetPrimitiveValue<float>() + minutes;
        DebugLogger.LogTrace($"New {mp_timelimit.Name} is {newTime}");
        
        mp_timelimit.SetValue(newTime);
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            player.ReplicateConVar(mp_timelimit.Name, $"{newTime}");
        }
        return true;
    }

    public bool ExtendRounds(int rounds)
    {
        if (RoundsLeft < 1 && MapTimeTypeOverride.Value == 0)
        {
            DebugLogger.LogWarning("TimeLeft util tried to extend a max rounds, but looks like current game mode is not a round based! aborting...");
            return false;
        }
        
        // Just in case
        mp_maxrounds ??= ConVar.Find("mp_maxrounds");

        if (mp_maxrounds == null)
        {
            DebugLogger.LogWarning("Failed to find the mp_maxrounds ConVar.");
            return false;
        }
        
        int newMaxRounds = mp_maxrounds.GetPrimitiveValue<int>() + rounds;
        DebugLogger.LogTrace($"New {mp_maxrounds.Name} is {newMaxRounds}");
        
        mp_maxrounds.SetValue(newMaxRounds);
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            player.ReplicateConVar(mp_maxrounds.Name, $"{newMaxRounds}");
        }
        return true;
    }

    public bool ExtendRoundTime(int minutes)
    {
        if (RoundTimeLeft < 1 && MapTimeTypeOverride.Value == 0)
        {
            DebugLogger.LogWarning("TimeLeft util tried to extend a round time limit, but looks like current game mode is not a round time limit based! aborting...");
            return false;
        }
        
        // Just in case
        mp_roundtime ??= ConVar.Find("mp_roundtime");

        if (mp_roundtime == null)
        {
            DebugLogger.LogWarning("Failed to find the mp_roundtime ConVar.");
            return false;
        }

        float currentTime = mp_roundtime.GetPrimitiveValue<float>();
        float newTime = currentTime + minutes;
        DebugLogger.LogTrace($"New {mp_roundtime.Name} is {newTime}");
        
        mp_roundtime.SetValue(newTime);
        GameRulesUtil.SetRoundTime(GameRulesUtil.GetRoundTime() + minutes*60);
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            player.ReplicateConVar(mp_roundtime.Name, $"{newTime}");
        }
        return true;
    }
    
    
    public bool Extend(int minutesOrRounds)
    {
        switch (ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                return ExtendTimeLimit(minutesOrRounds);
            case McsMapExtendType.Rounds:
                return ExtendRounds(minutesOrRounds);
            case McsMapExtendType.RoundTime:
                return ExtendRoundTime(minutesOrRounds);
            default:
                return false;
        }
    }

    public bool Set(int minutesOrRounds)
    {
        switch (ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                mp_timelimit?.SetValue(minutesOrRounds);
                return true;
            case McsMapExtendType.Rounds:
                mp_maxrounds?.SetValue(minutesOrRounds);
                return true;
            case McsMapExtendType.RoundTime:
                mp_roundtime?.SetValue(minutesOrRounds);
                return true;
            default:
                return false;
        }
    }

    public void ForceEndMatch()
    {
        Set(1);
        Server.NextFrame(() =>
        {
            if (GameRulesUtil.IsWarmup())
                Server.ExecuteCommand("mp_warmup_end");
            
            Server.NextFrame(() =>
            {
                GameRulesUtil.TerminateRound(0.0f, RoundEndReason.RoundDraw);
            });
        });
    }
    
    public string GetFormattedTimeLeft(int timeLeft)
    {
        if (timeLeft < 0)
        {
            return Plugin.LocalizeString("Word.LastRound");
        }
        
        
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

        
        if (timeLeft <= 0)
        {
            return Plugin.LocalizeStringForPlayer(player!, "Word.LastRound");
        }
        
        if (hours > 0)
        {
            return Plugin.LocalizeStringForPlayer(player!, "MapCycle.Command.Notification.TimeLeft.TimeFormat.Hours",
                hours, minutes, seconds);
        }

        if (minutes > 0)
        {
            return Plugin.LocalizeStringForPlayer(player!, "MapCycle.Command.Notification.TimeLeft.TimeFormat.Minutes",
                minutes, seconds);
        }
        
        return Plugin.LocalizeStringForPlayer(player!, "MapCycle.Command.Notification.TimeLeft.TimeFormat.Seconds",
            seconds);
    }


    public string GetFormattedRoundsLeft(int roundsLeft)
    {
        if (roundsLeft <= 0)
            return $"Last round!";
        
        return $"{roundsLeft} {(roundsLeft == 1 ? "round" : "rounds")}";
    }

    public string GetFormattedRoundsLeft(int roundsLeft, CCSPlayerController? player)
    {
        if (player == null)
            return GetFormattedRoundsLeft(roundsLeft);
        
        return Plugin.LocalizeStringForPlayer(player, "MapCycle.Command.Notification.TimeLeft.TimeFormat.Rounds", roundsLeft);
    }
    
    private McsMapExtendType DetermineExtendType()
    {
        if (MapTimeTypeOverride.Value > 0)
            return (McsMapExtendType)MapTimeTypeOverride.Value - 1;
        
        if (TimeLimit > 0 || mp_timelimit?.GetPrimitiveValue<float>() > 0.0)
            return McsMapExtendType.TimeLimit;
        
        if (RoundsLeft > 0 || mp_maxrounds?.GetPrimitiveValue<int>() > 0)
            return McsMapExtendType.Rounds;
        
        if (RoundTimeLeft > 0 || mp_roundtime?.GetPrimitiveValue<float>() > 0.0)
            return McsMapExtendType.RoundTime;

        throw new InvalidOperationException("Failed to determine extend type! the server is possibly misconfigured.");
    }
}