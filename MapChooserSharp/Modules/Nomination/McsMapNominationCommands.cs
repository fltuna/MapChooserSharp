using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.Modules.MapConfig.Interfaces;
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


    protected override void OnInitialize()
    {
        _mapNominationController = ServiceProvider.GetRequiredService<McsMapNominationController>();
        _mapConfigProvider = ServiceProvider.GetRequiredService<IMapConfigProvider>();
        
        Plugin.AddCommand("css_nominate", "Nominate a map", CommandNominateMap);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_nominate", CommandNominateMap);
    }
    
    
    private void CommandNominateMap(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            Server.PrintToConsole("Please use css_nominate_addmap instead.");
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


}