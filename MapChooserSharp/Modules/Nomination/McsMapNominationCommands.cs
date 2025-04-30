using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapVote;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.Nomination;

internal sealed class McsMapNominationCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsNominationCommands";
    public override string ModuleChatPrefix => _mapNominationController.ModuleChatPrefix;
    protected override bool UseTranslationKeyInModuleChatPrefix => true;
    
    private McsMapNominationController _mapNominationController = null!;
    private IMapConfigProvider _mapConfigProvider = null!;
    private McsMapVoteController _mcsMapVoteController = null!;
    private McsMapCycleController _mapCycleController = null!;


    protected override void OnAllPluginsLoaded()
    {
        _mapNominationController = ServiceProvider.GetRequiredService<McsMapNominationController>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
        _mcsMapVoteController = ServiceProvider.GetRequiredService<McsMapVoteController>();
        _mapCycleController = ServiceProvider.GetRequiredService<McsMapCycleController>();
        
        Plugin.AddCommand("css_nominate", "Nominate a map", CommandNominateMap);
        Plugin.AddCommand("css_nominate_addmap", "Insert a map to nomination", CommandNominateAddMap);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_nominate", CommandNominateMap);
        Plugin.RemoveCommand("css_nominate_addmap", CommandNominateAddMap);
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
        
        if (info.ArgCount < 2)
        {
            player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.Usage"));

            return;
        }

        string mapName = info.ArgByIndex(1);
        var mapConfigs = _mapConfigProvider.GetMapConfigs();

        var matchedMaps = mapConfigs.Where(mp => mp.Key.Contains(mapName)).ToDictionary();
        
        if (!matchedMaps.Any())
        {
            player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.NotMapsFound", mapName));

            _mapNominationController.ShowNominationMenu(player);
            return;
        }

        if (matchedMaps.Count > 1)
        {
            player.PrintToChat(LocalizeWithModulePrefixForPlayer(player, "Nomination.Command.Notification.MultipleResult", matchedMaps.Count, mapName));

            _mapNominationController.ShowNominationMenu(player, matchedMaps);
            return;
        }
        
        _mapNominationController.NominateMap(player, matchedMaps.First().Value);
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
            }
            return;
        }

        string mapName = info.ArgByIndex(1);
        var mapConfigs = _mapConfigProvider.GetMapConfigs();

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

                _mapNominationController.ShowAdminNominationMenu(player);
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

                _mapNominationController.ShowNominationMenu(player, matchedMaps);
            }
            return;
        }

        _mapNominationController.AdminNominateMap(player, matchedMaps.First().Value);
    }
}