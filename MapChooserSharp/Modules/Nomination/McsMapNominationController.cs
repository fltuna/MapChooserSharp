using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.MapVote;
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
    public override string ModuleChatPrefix => "Prefix.Nomination";
    protected override bool UseTranslationKeyInModuleChatPrefix => true;

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
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
        
        
        _mcsEventManager.RegisterEventHandler<McsMapVoteFinishedEvent>(OnVoteFinished);
        
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<McsMapVoteController>();
    }

    protected override void OnUnloadModule()
    {
    }


    private void OnVoteFinished(McsMapVoteFinishedEvent @event)
    {
        ResetNominations();
    }

    private void OnMapStart(string mapName)
    {
        ResetNominations();
    }
    

    internal void NominateMap(CCSPlayerController? player, IMapConfig mapConfig, bool isForce = false)
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
        }
        
        var nominationBegin = new McsNominationBeginEvent(player, nominated, GetTextWithModulePrefix(""));
        McsEventResult result = _mcsEventManager.FireEvent(nominationBegin);
        
        if (result > McsEventResult.Handled)
        {
            DebugLogger.LogInformation("Nomination begin event cancelled by a another plugin.");
            return;
        }
        
        // When this nomination is first nomination of the map
        // It will require to store, and takes no effect if already existed
        NominatedMaps[mapConfig.MapName] = nominated;
        
        bool isFirstNomination = true;
        foreach (var (key, value) in NominatedMaps)
        {
            if (value.NominationParticipants.Contains(player.Slot))
            {
                value.NominationParticipants.Remove(player.Slot);
                isFirstNomination = false;
                // break because player should be in 1 nomination, not multiple nomination.
                break;
            }
        }

        nominated.NominationParticipants.Add(player.Slot);
        
        PrintNominationResult(player, mapConfig, isFirstNomination);

        if (isFirstNomination)
        {
            var eventNominated = new McsMapNominatedEvent(player, nominated, GetTextWithModulePrefix(""));
            _mcsEventManager.FireEventNoResult(eventNominated);
        }
        else
        {
            var eventNominationChanged = new McsMapNominationChangedEvent(player, nominated, GetTextWithModulePrefix(""));
            _mcsEventManager.FireEventNoResult(eventNominationChanged);
        }
    }

    internal void AdminNominateMap()
    {
        
    }
    
    
    internal void ShowNominationMenu(CCSPlayerController player, Dictionary<string, IMapConfig>? mapCfgArg = null)
    {
        // TODO() Implement later
        player.PrintToChat("TODO_MENU| Nomination menu");

        Dictionary<string, IMapConfig>? mapConfigs = mapCfgArg ?? _mapConfigProvider.GetMapConfigs();
    }
    
    internal void ShowAdminNominationMenu(CCSPlayerController player, Dictionary<string, IMapConfig>? mapCfgArg = null)
    {
        // TODO() Implement later
        player.PrintToChat("TODO_MENU| Admin nomination menu");

        Dictionary<string, IMapConfig>? mapConfigs = mapCfgArg ?? _mapConfigProvider.GetMapConfigs();
    }



    private void PrintNominationResult(CCSPlayerController player, IMapConfig mapConfig, bool isFirstNomination)
    {
        string executorName = player.PlayerName;

        if (isFirstNomination)
        {
            if (mapConfig.MapNameAlias != string.Empty)
            {
                PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.Nominated", executorName, mapConfig.MapNameAlias);
            }
            else
            {
                PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.Nominated", executorName, mapConfig.MapName);
            }
        }
        else
        {
            if (mapConfig.MapNameAlias != string.Empty)
            {
                PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.NominationChanged", executorName, mapConfig.MapNameAlias);
            }
            else
            {
                PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.NominationChanged", executorName, mapConfig.MapName);
            }
        }
        
    }

    private NominationCheck PlayerCanNominateMap(CCSPlayerController player, IMapConfig mapConfig)
    {
        if (NominatedMaps.TryGetValue(mapConfig.MapName, out var nominated))
        {
            if (nominated.NominationParticipants.Contains(player.Slot))
                return NominationCheck.AlreadyNominated;

            if (nominated.IsForceNominated)
                return NominationCheck.NominatedByAdmin;
        }
        
        if (_mcsMapVoteController.CurrentVoteState != McsMapVoteState.NoActiveVote)
            return NominationCheck.DisabledAtThisTime;

        if (mapConfig.MapCooldown.CurrentCooldown > 0)
            return NominationCheck.MapIsInCooldown;

        INominationConfig nominationConfig = mapConfig.NominationConfig;

        SteamID? steamId = player.AuthorizedSteamID;
        
        if (steamId == null)
            return NominationCheck.Failed;

        
        // Bypasses admin check
        if (nominationConfig.AllowedSteamIds.Contains(steamId.SteamId64))
            return NominationCheck.Success;
        
        if (nominationConfig.RestrictToAllowedUsersOnly)
            return NominationCheck.NotAllowed;
        
        // Bypasses admin check too
        if (nominationConfig.DisallowedSteamIds.Contains(steamId.SteamId64))
            return NominationCheck.NotAllowed;
        
        if (nominationConfig.RequiredPermissions.Any() && !AdminManager.PlayerHasPermissions(player, nominationConfig.RequiredPermissions.ToArray()))
            return NominationCheck.NotEnoughPermissions;


        if (nominationConfig.MaxPlayers > 0 && nominationConfig.MaxPlayers < Utilities.GetPlayers().Count(p => p is {IsBot: false ,IsHLTV: false}))
            return NominationCheck.TooMuchPlayers;
        
        if (nominationConfig.MaxPlayers > 0 && nominationConfig.MinPlayers > Utilities.GetPlayers().Count(p => p is {IsBot: false ,IsHLTV: false}))
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
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.Generic.WithMapName", mapConfig.MapName));
                return false;
            
            case NominationCheck.NotEnoughPermissions:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.NotEnoughPermission", mapConfig.MapName));
                return false;
            
            case NominationCheck.TooMuchPlayers:
                int playerCountCurrently = Utilities.GetPlayers().Select(p => p is { IsBot: false, IsHLTV: false }).Count();
                int maxPlayers = mapConfig.NominationConfig.MaxPlayers;
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.TooMuchPlayers", mapConfig.MapName, playerCountCurrently, maxPlayers));
                return false;
            
            case NominationCheck.NotEnoughPlayers:
                playerCountCurrently = Utilities.GetPlayers().Select(p => p is { IsBot: false, IsHLTV: false }).Count();
                int minPlayers = mapConfig.NominationConfig.MaxPlayers;
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.NotEnoughPlayers", mapConfig.MapName, playerCountCurrently, minPlayers));
                return false;
            
            case NominationCheck.NotAllowed:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.NotAllowed", mapConfig.MapName));
                return false;
            
            case NominationCheck.DisabledAtThisTime:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.DisableAtThisTime"));
                return false;
            
            case NominationCheck.OnlySpecificDay:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.OnlySpecificDay", mapConfig.MapName));
                player.PrintToChat(GetTextWithModulePrefixForPlayer(player, LocalizeStringForPlayer(player, "Nomination.Notification.Failure.OnlySpecificDay.Days", string.Join(", ", mapConfig.NominationConfig.DaysAllowed))));
                return false;
            
            case NominationCheck.OnlySpecificTime:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.OnlySpecificTime", mapConfig.MapName));
                player.PrintToChat(GetTextWithModulePrefixForPlayer(player, LocalizeStringForPlayer(player, "Nomination.Notification.Failure.OnlySpecificTime.Times", string.Join(", ", mapConfig.NominationConfig.AllowedTimeRanges))));
                return false;
            
            case NominationCheck.MapIsInCooldown:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.MapIsInCooldown", mapConfig.MapName, mapConfig.MapCooldown.CurrentCooldown));
                return false;
            
            case NominationCheck.AlreadyNominated:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.AlreadyNominatedSameMap", mapConfig.MapName));
                return false;
            
            case NominationCheck.NominatedByAdmin:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.AlreadyNominatedByAdmin", mapConfig.MapName));
                return false;
        }
        
        return false;
    }
    
    
    private void ResetNominations()
    {
        NominatedMaps.Clear();
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
        AlreadyNominated,
        NominatedByAdmin,
    }
}