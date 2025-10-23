using System.Collections.Concurrent;
using System.Text;
using System.Xml.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsDatabase.Entities;
using MapChooserSharp.Modules.McsDatabase.Interfaces;
using MapChooserSharp.Modules.Nomination.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using ZLinq;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace MapChooserSharp.Modules.Nomination;

internal sealed class McsMapNominationCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsNominationCommands";
    public override string ModuleChatPrefix => _mapNominationController.ModuleChatPrefix;
    protected override bool UseTranslationKeyInModuleChatPrefix => true;
    
    private IMcsInternalNominationApi _mapNominationController = null!;
    private IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = null!;
    private IMcsInternalMapVoteControllerApi _mcsMapVoteController = null!;
    private IMcsInternalMapCycleControllerApi _mapCycleController = null!;
    private IMcsPluginConfigProvider _pluginConfigProvider = null!;
    private IMcsDatabaseProvider _mcsDatabaseProvider = null!;
    
    private ConcurrentDictionary<int, McsUserInformation> _userInformationCache = new();
    private Dictionary<int, Timer> _sessionTimeTimers = new();
    
    
    private readonly Dictionary<int, float> _playerNextCommandAvaiableTime = new();

    
    public readonly FakeConVar<float> NominationCommandCooldown = new ("mcs_nomination_command_cooldown", "Cooldown for nomination command", 10.0F);
    
    public readonly FakeConVar<bool> PreventSpectatorsNomination = new("mcs_nomination_command_prevent_spectators", "Prevent spectators nomination", false);

    protected override void OnInitialize()
    {
        TrackConVar(NominationCommandCooldown);
        TrackConVar(PreventSpectatorsNomination);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mapNominationController = ServiceProvider.GetRequiredService<IMcsInternalNominationApi>();
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _mcsMapVoteController = ServiceProvider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
        _mapCycleController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        _mcsDatabaseProvider = ServiceProvider.GetRequiredService<IMcsDatabaseProvider>();
        _pluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        
        Plugin.RegisterListener<Listeners.OnMapEnd>(() =>
        {
            _playerNextCommandAvaiableTime.Clear();
        });
        
        Plugin.AddCommand("css_nominate", "Nominate a map", CommandNominateMap);
        Plugin.AddCommand("css_nomlist", "Shows nomination list", CommandNomList);
        Plugin.AddCommand("css_nominate_addmap", "Insert a map to nomination", CommandNominateAddMap);
        Plugin.AddCommand("css_nominate_removemap", "Remove a map from nomination", CommandNominateRemoveMap);
        
        Plugin.AddCommandListener("say", SayCommandListener, HookMode.Pre);
        
        Plugin.RegisterListener<Listeners.OnClientAuthorized>(OnClientAuthorized);
        Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnClientDisconnect);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_nominate", CommandNominateMap);
        Plugin.RemoveCommand("css_nomlist", CommandNomList);
        Plugin.RemoveCommand("css_nominate_addmap", CommandNominateAddMap);
        Plugin.RemoveCommand("css_nominate_removemap", CommandNominateRemoveMap);
        
        Plugin.RemoveCommandListener("say", SayCommandListener, HookMode.Pre);
    }

    // TODO() This method will be removed after migrated to ModSharp implementation.
    private void OnClientAuthorized(int slot, SteamID steamId)
    {
        Task.Run(() =>
        {
            var info = _mcsDatabaseProvider.UserInfoRepository.GetUserInformationAsync(steamId.SteamId32).Result;

            if (info == null)
            {
                var newInfo = new McsUserInformation
                {
                    SteamId = steamId.SteamId32,
                    SessionTime = 0,
                    LastLoggedInAt = DateTime.Now,
                    UserSessionStartedAt = DateTime.Now
                };

                _mcsDatabaseProvider.UserInfoRepository.UpsertUserInformationAsync(steamId.SteamId32, newInfo).Wait();
                _userInformationCache[slot] = newInfo;
            }
            else
            {
                _userInformationCache[slot] = info;
            }
            
            Server.NextFrame(() =>
            {
                Logger.LogInformation("Starting session time timer for slot {Slot}.", slot);
                _sessionTimeTimers[slot] = Plugin.AddTimer(60.0F, () =>
                {
                    foreach (var mcsUserInformation in _userInformationCache)
                    {
                        UpdateUserSessionTime(slot, mcsUserInformation.Value);
                    }
                }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
            });
        });
    }

    private void UpdateUserSessionTime(int slot, McsUserInformation mcsUserInformation)
    {
        mcsUserInformation.SessionTime++;
        _mcsDatabaseProvider.UserInfoRepository.IncrementUserSessionTimeAsync(mcsUserInformation.SteamId);
        Logger.LogInformation("Incrementing session time for SteamID32: {SteamID32}, Current Session Time: {SessionTime} minutes.", mcsUserInformation.SteamId, mcsUserInformation.SessionTime);
                        
                        
        var loginExpiringTime = _pluginConfigProvider.PluginConfig.NominationConfig.LoginSessionExpiringTime;
                        

        DateTime now = DateTime.Now;
        TimeSpan resetTimeOfDay = loginExpiringTime.TimeOfDay;
        DateTime todayReset = DateTime.Today.Add(resetTimeOfDay);
        DateTime lastReset = now >= todayReset ? todayReset : todayReset.AddDays(-1);

        // If the user's session started before the last reset boundary, reset session time.
        if (mcsUserInformation.UserSessionStartedAt < lastReset)
        {
            mcsUserInformation.SessionTime = 0;
            mcsUserInformation.UserSessionStartedAt = now;
            _mcsDatabaseProvider.UserInfoRepository.UpsertUserInformationAsync(mcsUserInformation.SteamId, mcsUserInformation).Wait();
            _userInformationCache[slot] = mcsUserInformation;
        }
    }

    // TODO() This method will be removed after migrated to ModSharp implementation.
    private HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var cl = @event.Userid;
        
        if (cl == null)
            return HookResult.Continue;
        
        if (cl.IsBot || cl.IsHLTV)
            return HookResult.Continue;

        _userInformationCache.TryRemove(cl.AuthorizedSteamID!.SteamId32, out var value);
        _sessionTimeTimers.Remove(cl.Slot);
        return HookResult.Continue;
    }
    
    private void CommandNominateMap(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            Server.PrintToConsole("Please use css_nominate_addmap instead.");
            return;
        }

        if (_mcsMapVoteController.CurrentVoteState == McsMapVoteState.NextMapConfirmed)
        {
            player.PrintToChat(LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.NextMap", _mapCycleController.NextMap!.MapName));
            return;
        }

        if (_playerNextCommandAvaiableTime.TryGetValue(player.Slot, out float nextCommandAvaiableTime) && nextCommandAvaiableTime - Server.CurrentTime > 0.0)
        {
            float time = (float)Math.Ceiling(nextCommandAvaiableTime - Server.CurrentTime);
            player.PrintToChat(LocalizeWithPluginPrefix(player, "General.Notification.CommandCooldown", $"{time:F0}"));
            return;
        }

        _playerNextCommandAvaiableTime[player.Slot] = Server.CurrentTime + NominationCommandCooldown.Value;
        
        // If spectators nomination is prohibited, and player is not in CT or T.
        if (PreventSpectatorsNomination.Value && player.Team != CsTeam.CounterTerrorist && player.Team != CsTeam.Terrorist)
        {
            player.PrintToChat(LocalizeWithModulePrefix(player, "Nomination.Notification.Failure.SpectatorsCannotNominate"));
            return;
        }
        
        // Check Session time and login status, reset if needed, and auto-login if below threshold
        if (player.AuthorizedSteamID != null)
        {
            var config = _pluginConfigProvider.PluginConfig.NominationConfig;
            var requiredTimeToLogin = config.RequiredTimeToLogin;

            int steamId = player.AuthorizedSteamID.SteamId32;
            if (!_userInformationCache.TryGetValue(player.Slot, out var userInfo))
            {
                userInfo = _mcsDatabaseProvider.UserInfoRepository.GetUserInformationAsync(steamId).Result;
                if (userInfo == null)
                {
                    userInfo = new McsUserInformation
                    {
                        SteamId = steamId,
                        SessionTime = 0,
                        LastLoggedInAt = DateTime.Now,
                        UserSessionStartedAt = DateTime.Now
                    };

                    _mcsDatabaseProvider.UserInfoRepository.UpsertUserInformationAsync(steamId, userInfo).Wait();
                }

                _userInformationCache[player.Slot] = userInfo;
            }

            // If user's session time is below required threshold, consider them "not logged in" yet.
            if (userInfo.SessionTime < requiredTimeToLogin)
            {
                Logger.LogInformation("Player {PlayerName} (SteamID: {SteamID}) attempted to nominate a map but is not logged in. SessionTime: {SessionTime} minutes.", player.PlayerName, player.AuthorizedSteamID.SteamId64, userInfo.SessionTime);
                player.PrintToChat(LocalizeWithModulePrefix(player, "Nomination.Notification.Failure.NotLoggedIn", requiredTimeToLogin - userInfo.SessionTime, requiredTimeToLogin));
                return;
            }
        }
        
        if (info.ArgCount < 2)
        {
            player.PrintToChat(LocalizeWithModulePrefix(player, "Nomination.Command.Notification.Usage"));
            _mapNominationController.ShowNominationMenu(player);

            return;
        }

        string mapName = info.ArgByIndex(1);
        
        IMapConfig? exactMatchedConfig = FindConfigByExactName(mapName);

        if (exactMatchedConfig != null)
        {
            _mapNominationController.NominateMap(player, exactMatchedConfig);
            return;
        }

        var mapConfigs = _mcsInternalMapConfigProviderApi.GetMapConfigs();

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Value) .ToList();

        List<IMapConfig> filteredMaps = new();

        foreach (IMapConfig map in matchedMaps)
        {
            if (map.IsDisabled || map.NominationConfig.RestrictToAllowedUsersOnly)
                continue;

            if (map.NominationConfig.RequiredPermissions.Any() &&
                !AdminManager.PlayerHasPermissions(player, map.NominationConfig.RequiredPermissions.ToArray()))
                continue;
            
            filteredMaps.Add(map);
        }
        
        if (!filteredMaps.Any())
        {
            player.PrintToChat(LocalizeWithModulePrefix(player, "Nomination.Command.Notification.NotMapsFound", mapName));

            _mapNominationController.ShowNominationMenu(player);
            return;
        }
        
        if (filteredMaps.Count > 1)
        {
            player.PrintToChat(LocalizeWithModulePrefix(player, "Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));

            _mapNominationController.ShowNominationMenu(player, filteredMaps);
            return;
        }
        
        _mapNominationController.NominateMap(player, filteredMaps.First());
    }

    [RequiresPermissions(@"css/map")]
    private void CommandNominateAddMap(CCSPlayerController? player, CommandInfo info)
    {
        if (_mcsMapVoteController.CurrentVoteState == McsMapVoteState.NextMapConfirmed)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.NextMap", _mapCycleController.NextMap!.MapName));
            return;
        }
        
        if (info.ArgCount < 2)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "NominationAddMap.Command.Notification.Usage"));
            return;
        }

        string mapName = info.ArgByIndex(1);
        
        IMapConfig? exactMatchedConfig = FindConfigByExactName(mapName);

        if (exactMatchedConfig != null)
        {
            _mapNominationController.AdminNominateMap(player, exactMatchedConfig);
            return;
        }
        
        var mapConfigs = _mcsInternalMapConfigProviderApi.GetMapConfigs();

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName, StringComparison.OrdinalIgnoreCase)).ToDictionary();
        
        if (!matchedMaps.Any())
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "Nomination.Command.Notification.NotMapsFound", mapName));
            return;
        }

        if (matchedMaps.Count > 1)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));
            return;
        }

        _mapNominationController.AdminNominateMap(player, matchedMaps.First().Value);
    }

    [RequiresPermissions(@"css/map")]
    private void CommandNominateRemoveMap(CCSPlayerController? player, CommandInfo info)
    {
        if (_mcsMapVoteController.CurrentVoteState == McsMapVoteState.NextMapConfirmed)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "MapCycle.Command.Notification.NextMap", _mapCycleController.NextMap!.MapName));
            return;
        }
        
        if (info.ArgCount < 2)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "NominationRemoveMap.Command.Notification.Usage"));
            if (player != null)
            {
                _mapNominationController.ShowRemoveNominationMenu(player);
            }
            return;
        }

        string mapName = info.ArgByIndex(1);
        var mapConfigs = _mapNominationController.NominatedMaps;

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName, StringComparison.OrdinalIgnoreCase)).ToDictionary();
        
        if (!matchedMaps.Any())
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "Nomination.Command.Notification.NotMapsFound", mapName));
            if (player != null)
            {
                _mapNominationController.ShowRemoveNominationMenu(player);
            }
            return;
        }

        if (matchedMaps.Count > 1)
        {
            PrintMessageToServerOrPlayerChat(player, LocalizeWithPluginPrefix(player, "Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));
            if (player != null)
            {
                _mapNominationController.ShowRemoveNominationMenu(player, matchedMaps.Select(kv => kv.Value).ToList());
            }
            return;
        }

        _mapNominationController.RemoveNomination(player, matchedMaps.First().Value.MapConfig);
    }


    private void CommandNomList(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        if (_mapNominationController.NominatedMaps.Count < 1)
        {
            player.PrintToChat(LocalizeWithModulePrefix(player, "NominationList.Command.Notification.ThereIsNoNomination"));
            return;
        }
        
        
        player.PrintToChat(LocalizeWithModulePrefix(player, "NominationList.Command.Notification.ListHeader"));

        bool isVerbose = false;

        if (info.ArgCount > 1)
        {
            if (info.ArgByIndex(1).Equals("full") && AdminManager.PlayerHasPermissions(player, "css/map"))
            {
                isVerbose = true;
            }
        }
        
        int index = 1;
        foreach (var (key, value) in _mapNominationController.NominatedMaps)
        {
            PrintNominatedMap(player, index, value, isVerbose);
            index++;
        }
    }

    private void PrintNominatedMap(CCSPlayerController player, int index, IMcsNominationData nominationData, bool isVerbose = false)
    {
        StringBuilder nominatedText = new StringBuilder();

        if (isVerbose)
        {
            StringBuilder nominators = new StringBuilder();

            if (nominationData.IsForceNominated)
            {
                nominators.Append(LocalizeString(player, "NominationList.Command.Notification.AdminNomination"));
            }
            else
            {
                foreach (int participantSlot in nominationData.NominationParticipants)
                {
                    var target = Utilities.GetPlayerFromSlot(participantSlot);
                    
                    if (target == null)
                        continue;
                    
                    nominators.Append($"{LocalizeString(player, "NominationList.Command.Notification.Verbose.PlayerName", target.PlayerName)}, ");
                }
            }
            
            
                
                
            nominatedText.AppendLine(LocalizeString(player, "NominationList.Command.Notification.Verbose", index, _mcsInternalMapConfigProviderApi.GetMapName(nominationData.MapConfig), nominators.ToString()));
        }
        else
        {
            if (nominationData.IsForceNominated)
            {
                nominatedText.AppendLine(LocalizeString(player, "NominationList.Command.Notification.Verbose", index, _mcsInternalMapConfigProviderApi.GetMapName(nominationData.MapConfig), LocalizeString(player, "NominationList.Command.Notification.AdminNomination")));
            }
            else
            {
                nominatedText.AppendLine(LocalizeString(player, "NominationList.Command.Notification.Content", index, _mcsInternalMapConfigProviderApi.GetMapName(nominationData.MapConfig), nominationData.NominationParticipants.Count));
            }
        }
        
        player.PrintToChat(GetTextWithModulePrefix(player, nominatedText.ToString()));
    }

    private IMapConfig? FindConfigByExactName(string mapName)
    {
        _mcsInternalMapConfigProviderApi.GetMapConfigs().TryGetValue(mapName, out var mapConfig);
        return mapConfig;
    }
    
    
    private HookResult SayCommandListener(CCSPlayerController? player, CommandInfo info)
    {
        if(player == null)
            return HookResult.Continue;

        if (info.ArgCount < 2)
            return HookResult.Continue;
        
        string arg1 = info.ArgByIndex(1);

        bool commandFound = false;


        if (arg1.Equals("nominate", StringComparison.OrdinalIgnoreCase))
        {
            player.ExecuteClientCommandFromServer("css_nominate");
            commandFound = true;
        }

        return commandFound ? HookResult.Handled : HookResult.Continue;
    }
}