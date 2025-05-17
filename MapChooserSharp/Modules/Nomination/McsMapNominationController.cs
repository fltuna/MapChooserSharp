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
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.EventManager;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu;
using MapChooserSharp.Modules.McsMenu.NominationMenu;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;
using MapChooserSharp.Modules.Nomination.Interfaces;
using MapChooserSharp.Modules.Nomination.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Modules.Nomination;

internal sealed class McsMapNominationController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsInternalNominationApi
{
    public override string PluginModuleName => "McsMapNominationController";
    public override string ModuleChatPrefix => "Prefix.Nomination";
    protected override bool UseTranslationKeyInModuleChatPrefix => true;

    private IMcsInternalEventManager _mcsEventManager = null!;
    private IMcsInternalMapVoteControllerApi _mcsMapVoteController = null!;
    private IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = null!;
    private IMcsNominationMenuProvider _mcsNominationMenuProvider = null!;
    private IMcsInternalMapCycleControllerApi _mcsMapCycleController = null!;
    
    private readonly Dictionary<int, IMcsNominationUserInterface> _mcsActiveUserNominationMenu = new();
    
    
    public Dictionary<string, IMcsNominationData> NominatedMaps { get; } = new();

    public IReadOnlyDictionary<string, IMcsNominationData> GetNominatedMaps() => NominatedMaps;

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMcsInternalNominationApi>(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _mcsNominationMenuProvider = ServiceProvider.GetRequiredService<IMcsNominationMenuProvider>();
        _mcsMapCycleController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        
        _mcsEventManager.RegisterEventHandler<McsMapVoteInitiatedEvent>(OnVoteInitialized);
        _mcsEventManager.RegisterEventHandler<McsMapVoteFinishedEvent>(OnVoteFinished);
        _mcsEventManager.RegisterEventHandler<McsMapVoteCancelledEvent>(OnVoteCancelled);
        _mcsEventManager.RegisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        _mcsEventManager.RegisterEventHandler<McsMapExtendEvent>(OnMapExtended);
        
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        Plugin.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
    }

    protected override void OnUnloadModule()
    {
        _mcsEventManager.UnregisterEventHandler<McsMapVoteInitiatedEvent>(OnVoteInitialized);
        _mcsEventManager.UnregisterEventHandler<McsMapVoteFinishedEvent>(OnVoteFinished);
        _mcsEventManager.UnregisterEventHandler<McsMapVoteCancelledEvent>(OnVoteCancelled);
        _mcsEventManager.UnregisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        _mcsEventManager.UnregisterEventHandler<McsMapExtendEvent>(OnMapExtended);
    }

    private void OnVoteInitialized(McsMapVoteInitiatedEvent @event)
    {
        foreach (var (key, menu) in _mcsActiveUserNominationMenu)
        {
            menu.CloseMenu();
            _mcsActiveUserNominationMenu.Remove(key);
        }
    }

    private void OnMapNotChanged(McsMapNotChangedEvent @event)
    {
        ResetNominations();
    }

    private void OnMapExtended(McsMapExtendEvent @event)
    {
        ResetNominations();
    }

    private void OnVoteFinished(McsMapVoteFinishedEvent @event)
    {
        ResetNominations();
    }

    private void OnVoteCancelled(McsMapVoteCancelledEvent @event)
    {
        ResetNominations();
    }

    private void OnMapStart(string mapName)
    {
        ResetNominations();
    }


    private void OnClientDisconnect(int slot)
    {
        foreach (IMcsNominationData data in NominatedMaps.Values)
        {
            data.NominationParticipants.Remove(slot);
        }
    }
    

    public void NominateMap(CCSPlayerController player, IMapConfig mapConfig)
    {
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

                // If there is no nomination participants left, remove the nomination
                if (value.NominationParticipants.Count == 0)
                {
                    NominatedMaps.Remove(key);
                }
                
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

    public void AdminNominateMap(CCSPlayerController? player, IMapConfig mapConfig)
    {
        if (mapConfig.NominationConfig.ProhibitAdminNomination && player != null)
        {
            player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "NominationAddMap.Command.Notification.AdminNominationProhibited"));
            return;
        }

        bool isFirstNomination = NominatedMaps.TryGetValue(mapConfig.MapName, out var nominated);

        if (!isFirstNomination || nominated == null)
        {
            nominated = new McsNominationData(mapConfig);
        }
        
        // When this nomination is first nomination of the map
        // It will require to store, and takes no effect if already existed
        NominatedMaps[mapConfig.MapName] = nominated;
        
        nominated.IsForceNominated = true;

        var adminNominateEvent = new McsMapAdminNominatedEvent(player, nominated, GetTextWithModulePrefix(""));
        _mcsEventManager.FireEventNoResult(adminNominateEvent);


        string executorName = PlayerUtil.GetPlayerName(player);

        if (isFirstNomination)
        {
            PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.Admin.ChangedToAdminNomination", executorName, _mcsInternalMapConfigProviderApi.GetMapName(mapConfig));
        }
        else
        {
            PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.Admin.Nominated", executorName, _mcsInternalMapConfigProviderApi.GetMapName(mapConfig));
        }
        Logger.LogInformation($"Admin {executorName} is inserted {mapConfig.MapName} to nomination.");
    }

    /// <summary>
    /// Show nomination menu to player, this method is show maps in given config list
    /// </summary>
    /// <param name="player">Player controller</param>
    /// <param name="configs">Map configs to show</param>
    /// <param name="isAdminNomination">This nomination menu is should be admin nomination menu?</param>
    public void ShowNominationMenu(CCSPlayerController player, List<IMapConfig> configs, bool isAdminNomination = false)
    {
        if(configs.Count == 0)
            return;
        
        var ui = _mcsNominationMenuProvider.CreateNewNominationUi(player);

        List<IMcsNominationMenuOption> menuOptions = new();
        
        foreach (IMapConfig config in configs)
        {
            // TODO() More menu disablation check
            bool isMenuDisabled = config.IsDisabled;
            
            menuOptions.Add(new McsNominationMenuOption(new McsNominationOption(config, isAdminNomination), OnPlayerCastNominationMenu, isMenuDisabled));
        }
        
        ui.SetNominationOption(menuOptions);
        ui.SetMenuOption(new McsGeneralMenuOption("Nomination.Menu.MenuTitle", true));
        ui.OpenMenu();
        _mcsActiveUserNominationMenu[player.Slot] = ui;
    }

    /// <summary>
    /// Show nomination menu to player, this method is shows all maps in map config provider
    /// </summary>
    /// <param name="player">Player controller</param>
    /// <param name="isAdminNomination">This nomination menu is should be admin nomination menu?</param>
    public void ShowNominationMenu(CCSPlayerController player, bool isAdminNomination = false)
    {
        ShowNominationMenu(player, _mcsInternalMapConfigProviderApi.GetMapConfigs().Select(kv => kv.Value).ToList(), isAdminNomination);
    }
    
    private void OnPlayerCastNominationMenu(CCSPlayerController client, IMcsNominationOption option)
    {
        if (option.IsAdminNomination)
        {
            AdminNominateMap(client, option.MapConfig);
        }
        else
        {
            NominateMap(client, option.MapConfig);
        }

        if (_mcsActiveUserNominationMenu.TryGetValue(client.Slot, out var ui))
        {
            ui.CloseMenu();
            _mcsActiveUserNominationMenu.Remove(client.Slot);
        }
    }
    
    
    
    public void ShowRemoveNominationMenu(CCSPlayerController player, List<IMcsNominationData> nominationData)
    {
        if (nominationData.Count == 0)
            return;
        
        var ui = _mcsNominationMenuProvider.CreateNewNominationUi(player);

        List<IMcsNominationMenuOption> menuOptions = new();
        
        foreach (IMcsNominationData data in nominationData)
        {
            menuOptions.Add(new McsNominationMenuOption(new McsNominationOption(data.MapConfig), OnPlayerCastRemoveNominationMenu, false));
        }
        
        ui.SetNominationOption(menuOptions);
        ui.SetMenuOption(new McsGeneralMenuOption("NominationRemoveMap.Menu.MenuTitle", true));
        ui.OpenMenu();
        _mcsActiveUserNominationMenu[player.Slot] = ui;
    }
    
    public void ShowRemoveNominationMenu(CCSPlayerController player)
    {
        ShowRemoveNominationMenu(player, NominatedMaps.Select(kv => kv.Value).ToList());
    }
    
    private void OnPlayerCastRemoveNominationMenu(CCSPlayerController client, IMcsNominationOption option)
    {
        client.ExecuteClientCommandFromServer($"css_nominate_removemap {option.MapConfig.MapName}");

        if (_mcsActiveUserNominationMenu.TryGetValue(client.Slot, out var ui))
        {
            ui.CloseMenu();
            _mcsActiveUserNominationMenu.Remove(client.Slot);
        }
    }


    public void RemoveNomination(CCSPlayerController? player, IMapConfig mapConfig)
    {
        if (!NominatedMaps.Remove(mapConfig.MapName))
            return;
        
        string executorName = PlayerUtil.GetPlayerName(player);
        PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.Admin.RemovedNomiantion", executorName, _mcsInternalMapConfigProviderApi.GetMapName(mapConfig));
        Logger.LogInformation($"Admin {executorName} is removed {mapConfig.MapName} from nomination.");
    }

    private void PrintNominationResult(CCSPlayerController player, IMapConfig mapConfig, bool isFirstNomination)
    {
        string executorName = player.PlayerName;

        if (isFirstNomination)
        {
            PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.Nominated", executorName, _mcsInternalMapConfigProviderApi.GetMapName(mapConfig));
        }
        else
        {
            PrintLocalizedChatToAllWithModulePrefix("Nomination.Broadcast.NominationChanged", executorName, _mcsInternalMapConfigProviderApi.GetMapName(mapConfig));
        }
    }

    public NominationCheck PlayerCanNominateMap(CCSPlayerController player, IMapConfig mapConfig)
    {
        if (_mcsMapCycleController.CurrentMap?.MapName == mapConfig.MapName)
            return NominationCheck.SameMap;
        
        if (NominatedMaps.TryGetValue(mapConfig.MapName, out var nominated))
        {
            if (nominated.NominationParticipants.Contains(player.Slot))
                return NominationCheck.AlreadyNominated;

            if (nominated.IsForceNominated)
                return NominationCheck.NominatedByAdmin;
        }
        
        if (_mcsMapVoteController.CurrentVoteState != McsMapVoteState.NoActiveVote)
            return NominationCheck.DisabledAtThisTime;

        
        if (GetHighestCooldown(mapConfig) > 0)
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
        
        if (nominationConfig.MinPlayers > 0 && nominationConfig.MinPlayers > Utilities.GetPlayers().Count(p => p is {IsBot: false ,IsHLTV: false}))
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
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.Generic.WithMapName", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig)));
                return false;
            
            case NominationCheck.NotEnoughPermissions:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.NotEnoughPermission", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig)));
                return false;
            
            case NominationCheck.TooMuchPlayers:
                int playerCountCurrently = Utilities.GetPlayers().Select(p => p is { IsBot: false, IsHLTV: false }).Count();
                int maxPlayers = mapConfig.NominationConfig.MaxPlayers;
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.TooMuchPlayers", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig), playerCountCurrently, maxPlayers));
                return false;
            
            case NominationCheck.NotEnoughPlayers:
                playerCountCurrently = Utilities.GetPlayers().Select(p => p is { IsBot: false, IsHLTV: false }).Count();
                int minPlayers = mapConfig.NominationConfig.MinPlayers;
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.NotEnoughPlayers", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig), playerCountCurrently, minPlayers));
                return false;
            
            case NominationCheck.NotAllowed:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.NotAllowed", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig)));
                return false;
            
            case NominationCheck.DisabledAtThisTime:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.DisableAtThisTime"));
                return false;
            
            case NominationCheck.OnlySpecificDay:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.OnlySpecificDay", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig)));
                player.PrintToChat(GetTextWithModulePrefixForPlayer(player, LocalizeStringForPlayer(player, "Nomination.Notification.Failure.OnlySpecificDay.Days", string.Join(", ", mapConfig.NominationConfig.DaysAllowed))));
                return false;
            
            case NominationCheck.OnlySpecificTime:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.OnlySpecificTime", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig)));
                player.PrintToChat(GetTextWithModulePrefixForPlayer(player, LocalizeStringForPlayer(player, "Nomination.Notification.Failure.OnlySpecificTime.Times", string.Join(", ", mapConfig.NominationConfig.AllowedTimeRanges))));
                return false;
            
            case NominationCheck.MapIsInCooldown:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.MapIsInCooldown", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig), GetHighestCooldown(mapConfig)));
                return false;
            
            case NominationCheck.AlreadyNominated:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.AlreadyNominatedSameMap", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig)));
                return false;
            
            case NominationCheck.NominatedByAdmin:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.AlreadyNominatedByAdmin", _mcsInternalMapConfigProviderApi.GetMapName(mapConfig)));
                return false;
            
            case NominationCheck.SameMap:
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Notification.Failure.SameMap"));
                return false;
        }
        
        return false;
    }

    
    private int GetHighestCooldown(IMapConfig mapConfig)
    {
        int highestCooldown = mapConfig.MapCooldown.CurrentCooldown;
        
        foreach (IMapGroupSettings groupSetting in mapConfig.GroupSettings)
        {
            if (groupSetting.GroupCooldown.CurrentCooldown > highestCooldown)
            {
                highestCooldown = groupSetting.GroupCooldown.CurrentCooldown;
            }
        }

        return highestCooldown;
    }
    
    private void ResetNominations()
    {
        NominatedMaps.Clear();
    }

    internal enum NominationCheck
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
        SameMap,
    }
}