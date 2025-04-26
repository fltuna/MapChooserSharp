using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.Nomination;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.API.Nomination.Interfaces;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.Nomination.Models;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Modules.Nomination;

internal sealed class McsMapNominationController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsNominationApi
{
    public override string PluginModuleName => "McsMapNominationController";
    public override string ModuleChatPrefix => $" {ChatColors.Green}[Nomination]{ChatColors.Default}";

    private IMcsInternalEventManager _mcsEventManager = null!;
    private McsMapVoteController _mcsMapVoteController = null!;
    private IMapConfigProvider _mapConfigProvider = null!;
    
    
    internal Dictionary<string, IMcsNominationData> NominatedMaps { get; } = new();

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnInitialize()
    {
        Plugin.AddCommand("css_nominate", "", CommandNominateMap);
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<McsMapVoteController>();
        
        _mcsEventManager.RegisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsEventManager.RegisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_nominate", CommandNominateMap);
        _mcsEventManager.UnregisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsEventManager.UnregisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
    }






    private void CommandNominateMap(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            info.ReplyToCommand("TODO_TRANSLATE| Usage: !nominate <MapName>");
            return;
        }

        string mapName = info.ArgByIndex(1);
        var mapConfigs = _mapConfigProvider.GetMapConfigs();

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName)).ToDictionary();
        
        if (!matchedMaps.Any())
        {
            info.ReplyToCommand($"TODO_TRANSLATE| No map(s) found with {mapName}");

            if (player == null)
                return;
            
            ShowNominationMenu(player);
            return;
        }

        if (matchedMaps.Count > 1)
        {
            info.ReplyToCommand($"TODO_TRANSLATE| {matchedMaps.Count} maps found with {mapName}");

            if (player == null)
            {
                info.ReplyToCommand("Please specify the identical name.");
                return;
            }
            
            ShowNominationMenu(player, matchedMaps);
            return;
        }
        
        NominateMap(player, matchedMaps.First().Value);
    }



    private void NominateMap(CCSPlayerController? player, IMapConfig mapConfig, bool isForce = false)
    {
        if (player == null)
        {
            Server.PrintToConsole("Please use css_nominate_addmap instead.");
            return;
        }

        NominationCheck check = PlayerCanNominateMap(player, mapConfig);

        bool processed = ProcessNominationCheckResult(player, mapConfig, check);
        
        if (!processed)
            return;

        if (!NominatedMaps.TryGetValue(mapConfig.MapName, out var nominated))
        {
            nominated = new McsNominationData(mapConfig);
            NominatedMaps[mapConfig.MapName] = nominated;
        }

        nominated.NominationParticipants.Add(player.Slot);
        
        var nominationBegin = new McsNominationBeginEvent(player, nominated, ModuleChatPrefix);
        McsEventResult result = _mcsEventManager.FireEvent(nominationBegin);
        
        if (result > McsEventResult.Handled)
        {
            DebugLogger.LogInformation("Nomination begin event cancelled by a another plugin.");
            return;
        }

        string executorName = PlayerUtil.GetPlayerName(player);
        if (mapConfig.MapNameAlias != string.Empty)
        {
            Server.PrintToChatAll($"TODO_TRANSLATE| {executorName} nominated {mapConfig.MapNameAlias}");
        }
        else
        {
            Server.PrintToChatAll($"TODO_TRANSLATE| {executorName} nominated {mapConfig.MapName}");
        }
        
        
        
        var eventNominated = new McsMapNominatedEvent(player, nominated, executorName);
        _mcsEventManager.FireEventNoResult(eventNominated);
    }



    private NominationCheck PlayerCanNominateMap(CCSPlayerController player, IMapConfig mapConfig)
    {
        if (_mcsMapVoteController.CurrentVoteState != McsMapVoteState.NoActiveVote)
            return NominationCheck.DisabledAtThisTime;

        if (mapConfig.MapCooldown.CurrentCooldown > 0)
            return NominationCheck.MapIsInCooldown;

        INominationConfig nominationConfig = mapConfig.NominationConfig;

        SteamID? steamId = player.AuthorizedSteamID;
        
        if (steamId == null)
            return NominationCheck.Failed;


        if (nominationConfig.RestrictToAllowedUsersOnly)
            return NominationCheck.NotAllowed;
        
        // Bypasses admin check
        if (nominationConfig.AllowedSteamIds.Contains(steamId.SteamId64))
            return NominationCheck.Success;
        
        // Bypasses admin check too
        if (nominationConfig.DisallowedSteamIds.Contains(steamId.SteamId64))
            return NominationCheck.NotAllowed;
        
        if (!AdminManager.PlayerHasPermissions(player, nominationConfig.RequiredPermissions.ToArray()))
            return NominationCheck.NotEnoughPermissions;


        if (nominationConfig.MaxPlayers < Utilities.GetPlayers().Count(p => p is {IsBot: false ,IsHLTV: false}))
            return NominationCheck.TooMuchPlayers;
        
        if (nominationConfig.MinPlayers > Utilities.GetPlayers().Count(p => p is {IsBot: false ,IsHLTV: false}))
            return NominationCheck.NotEnoughPlayers;


        if (nominationConfig.DaysAllowed.Any() && !nominationConfig.DaysAllowed.Contains(DateTime.Today.DayOfWeek))
            return NominationCheck.OnlySpecificDay;

        if (nominationConfig.AllowedTimeRanges.Any() && nominationConfig.AllowedTimeRanges.Count(range => range.IsInRange(TimeOnly.FromDateTime(DateTime.Now))) < 1)
            return NominationCheck.OnlySpecificTime;
        
        return NominationCheck.Success;
    }

    private bool ProcessNominationCheckResult(CCSPlayerController player, IMapConfig mapConfig, NominationCheck check)
    {
        switch (check)
        {
            case NominationCheck.Success:
                return true;
            
            case NominationCheck.Failed:
                player.PrintToChat("TODO_TRANSLATE| Failed to nominate map");
                return false;
            
            case NominationCheck.NotEnoughPermissions:
                player.PrintToChat("TODO_TRANSLATE| You don't have enough permissions to nominate this map");
                return false;
            
            case NominationCheck.TooMuchPlayers:
                player.PrintToChat("TODO_TRANSLATE| Too much players to nominate this map");
                return false;
            
            case NominationCheck.NotEnoughPlayers:
                player.PrintToChat("TODO_TRANSLATE| Not enough players to nominate this map");
                return false;
            
            case NominationCheck.NotAllowed:
                player.PrintToChat("TODO_TRANSLATE| You are not allowed to nominate this map");
                return false;
            
            case NominationCheck.DisabledAtThisTime:
                player.PrintToChat("TODO_TRANSLATE| Nomination is disabled at this time");
                return false;
            
            case NominationCheck.OnlySpecificDay:
                player.PrintToChat($"TODO_TRANSLATE| Only Specific day(s) is allowed to nominate this map ({string.Join(", ", mapConfig.NominationConfig.DaysAllowed)}");
                return false;
            
            case NominationCheck.OnlySpecificTime:
                player.PrintToChat($"TODO_TRANSLATE| Only Specific time(s) is allowed to nominate this map ({string.Join(", ", mapConfig.NominationConfig.AllowedTimeRanges)}");
                return false;
            
            case NominationCheck.MapIsInCooldown:
                player.PrintToChat($"TODO_TRANSLATE| Map is in cooldown: {mapConfig.MapCooldown.CurrentCooldown} left");
                return false;
        }
        
        return false;
    }
    
    
    private void ShowNominationMenu(CCSPlayerController player, Dictionary<string, IMapConfig>? mapCfgArg = null)
    {
        // TODO() Implement later
        player.PrintToChat("TODO_MENU| Nomination menu");

        Dictionary<string, IMapConfig>? mapConfigs = mapCfgArg ?? _mapConfigProvider.GetMapConfigs();
    }
    
    
    

    private McsEventResultWithCallback OnMapNominationBegin(McsNominationBeginEvent @event)
    {
        if (@event.Player != null && @event.Player.PlayerPawn.Value != null && @event.Player.PlayerPawn.Value.Health < 80)
        {
            return McsEventResultWithCallback.Stop(result =>
            {
                @event.Player!.PrintToChat($"[McsMapNominationBeginEvent Listener] Your health is not enough to nominate!!! Cancelling nomination. Status: {result}");
                @event.Player.PrintToChat($"{@event.ModulePrefix} You can use MCS module's prefix!");
            });
        }

        return McsEventResult.Continue;
    }

    private void OnMapNominated(McsMapNominatedEvent @event)
    {
        var player = @event.Player;

        string executorName = PlayerUtil.GetPlayerName(@event.Player);
        
        if (player == null)
        {
            Server.PrintToChatAll($"[McsMapNominatedEvent Listener] detected {executorName} nominated {@event.NominationData.MapConfig.MapName}");
        }
        else
        {
            Server.PrintToChatAll($"[McsMapNominatedEvent Listener] detected {executorName} nominated {@event.NominationData.MapConfig.MapName}");
        }
    }

    private enum NominationCheck
    {
        Success,
        Failed,
        NotEnoughPermissions,
        TooMuchPlayers,
        NotEnoughPlayers,
        NotAllowed,
        DisabledAtThisTime,
        OnlySpecificDay,
        OnlySpecificTime,
        MapIsInCooldown,
    }
}