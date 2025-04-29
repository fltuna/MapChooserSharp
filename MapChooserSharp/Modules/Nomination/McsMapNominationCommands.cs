using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.Nomination;

internal sealed class McsMapNominationCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsNominationCommands";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
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
            
            _mapNominationController.ShowNominationMenu(player);
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
            
            _mapNominationController.ShowNominationMenu(player, matchedMaps);
            return;
        }
        
        _mapNominationController.NominateMap(player, matchedMaps.First().Value);
    }


}