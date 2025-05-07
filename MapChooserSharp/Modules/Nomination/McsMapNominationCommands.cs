using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.API.Nomination;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;
using MapChooserSharp.Modules.Nomination.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

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
    
    
    private readonly Dictionary<int, float> _playerNextCommandAvaiableTime = new();

    
    public readonly FakeConVar<float> NominationCommandCooldown = new ("mcs_nomination_command_cooldown", "Cooldown for nomination command", 10.0F);

    protected override void OnInitialize()
    {
        TrackConVar(NominationCommandCooldown);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mapNominationController = ServiceProvider.GetRequiredService<IMcsInternalNominationApi>();
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _mcsMapVoteController = ServiceProvider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
        _mapCycleController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        
        Plugin.AddCommand("css_nominate", "Nominate a map", CommandNominateMap);
        Plugin.AddCommand("css_nomlist", "Shows nomination list", CommandNomList);
        Plugin.AddCommand("css_nominate_addmap", "Insert a map to nomination", CommandNominateAddMap);
        Plugin.AddCommand("css_nominate_removemap", "Remove a map from nomination", CommandNominateRemoveMap);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_nominate", CommandNominateMap);
        Plugin.RemoveCommand("css_nomlist", CommandNomList);
        Plugin.RemoveCommand("css_nominate_addmap", CommandNominateAddMap);
        Plugin.RemoveCommand("css_nominate_removemap", CommandNominateRemoveMap);
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
            player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.NextMap", _mapCycleController.NextMap!.MapName));
            return;
        }

        if (_playerNextCommandAvaiableTime.TryGetValue(player.Slot, out float nextCommandAvaiableTime) && nextCommandAvaiableTime - Server.CurrentTime > 0.0)
        {
            float time = (float)Math.Ceiling(nextCommandAvaiableTime - Server.CurrentTime);
            player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.CommandCooldown", $"{time:F0}"));
            return;
        }

        _playerNextCommandAvaiableTime[player.Slot] = Server.CurrentTime + NominationCommandCooldown.Value;
        
        if (info.ArgCount < 2)
        {
            player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.Usage"));
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

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName)).Select(kv => kv.Value) .ToList();

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
            player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.NotMapsFound", mapName));

            _mapNominationController.ShowNominationMenu(player);
            return;
        }
        
        if (filteredMaps.Count > 1)
        {
            player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));

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
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Notification.NextMap", _mapCycleController.NextMap!.MapName));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.NextMap", _mapCycleController.NextMap!.MapName));
            }
            return;
        }
        
        if (info.ArgCount < 2)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("NominationAddMap.Command.Notification.Usage"));
            }
            else
            {
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "NominationAddMap.Command.Notification.Usage"));
                _mapNominationController.ShowNominationMenu(player, true);
            }
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

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName)).ToDictionary();
        
        if (!matchedMaps.Any())
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("Nomination.Command.Notification.NotMapsFound", mapName));
            }
            else
            {
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.NotMapsFound", mapName));

                _mapNominationController.ShowNominationMenu(player, true);
            }
            return;
        }

        if (matchedMaps.Count > 1)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));
            }
            else
            {
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));

                _mapNominationController.ShowNominationMenu(player, matchedMaps.Select(kv => kv.Value).ToList(), true);
            }
            return;
        }

        _mapNominationController.AdminNominateMap(player, matchedMaps.First().Value);
    }

    [RequiresPermissions(@"css/map")]
    private void CommandNominateRemoveMap(CCSPlayerController? player, CommandInfo info)
    {
        if (_mcsMapVoteController.CurrentVoteState == McsMapVoteState.NextMapConfirmed)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycle.Command.Notification.NextMap", _mapCycleController.NextMap!.MapName));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.NextMap", _mcsInternalMapConfigProviderApi.GetMapName(_mapCycleController.NextMap!)));
            }
            return;
        }
        
        if (info.ArgCount < 2)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("NominationRemoveMap.Command.Notification.Usage"));
            }
            else
            {
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "NominationRemoveMap.Command.Notification.Usage"));
                _mapNominationController.ShowRemoveNominationMenu(player);
            }
            return;
        }

        string mapName = info.ArgByIndex(1);
        var mapConfigs = _mapNominationController.NominatedMaps;

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName)).ToDictionary();
        
        if (!matchedMaps.Any())
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("Nomination.Command.Notification.NotMapsFound", mapName));

            }
            else
            {
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.NotMapsFound", mapName));

                _mapNominationController.ShowRemoveNominationMenu(player);
            }
            return;
        }

        if (matchedMaps.Count > 1)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));
            }
            else
            {
                player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));

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
            player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "NominationList.Command.Notification.ThereIsNoNomination"));
            return;
        }
        
        
        player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "NominationList.Command.Notification.ListHeader"));

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
                nominators.Append(LocalizeStringForPlayer(player, "NominationList.Command.Notification.AdminNomination"));
            }
            else
            {
                foreach (int participantSlot in nominationData.NominationParticipants)
                {
                    var target = Utilities.GetPlayerFromSlot(participantSlot);
                    
                    if (target == null)
                        continue;
                    
                    nominators.Append($"{LocalizeStringForPlayer(player, "NominationList.Command.Notification.Verbose.PlayerName", target.PlayerName)}, ");
                }
            }
            
            
                
                
            nominatedText.AppendLine(LocalizeStringForPlayer(player, "NominationList.Command.Notification.Verbose", index, _mcsInternalMapConfigProviderApi.GetMapName(nominationData.MapConfig), nominators.ToString()));
        }
        else
        {
            if (nominationData.IsForceNominated)
            {
                nominatedText.AppendLine(LocalizeStringForPlayer(player, "NominationList.Command.Notification.Verbose", index, _mcsInternalMapConfigProviderApi.GetMapName(nominationData.MapConfig), LocalizeStringForPlayer(player, "NominationList.Command.Notification.AdminNomination")));
            }
            else
            {
                nominatedText.AppendLine(LocalizeStringForPlayer(player, "NominationList.Command.Notification.Content", index, _mcsInternalMapConfigProviderApi.GetMapName(nominationData.MapConfig), nominationData.NominationParticipants.Count));
            }
        }
        
        player.PrintToChat(GetTextWithModulePrefixForPlayer(player, nominatedText.ToString()));
    }

    private IMapConfig? FindConfigByExactName(string mapName)
    {
        _mcsInternalMapConfigProviderApi.GetMapConfigs().TryGetValue(mapName, out var mapConfig);
        return mapConfig;
    }
}