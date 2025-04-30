using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapCycle;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapVote;

public class McsMapVoteCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsMapVoteCommands";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
    private McsMapVoteController _mcsMapVoteController = null!;
    private McsMapCycleController _mapCycleController = null!;


    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<McsMapVoteController>();
        _mapCycleController = ServiceProvider.GetRequiredService<McsMapCycleController>();
        
        Plugin.AddCommand("css_revote", "Revote command", CommandRevote);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_revote", CommandRevote);
    }

    private void CommandRevote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        if (_mcsMapVoteController.CurrentVoteState == McsMapVoteState.NextMapConfirmed)
        {
            player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycle.Command.Notification.NextMap", _mapCycleController.NextMap!.MapName));
            return;
        }

        if (_mcsMapVoteController.CurrentVoteState != McsMapVoteState.Voting && _mcsMapVoteController.CurrentVoteState != McsMapVoteState.RunoffVoting)
        {
            player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapVote.Command.Notification.Revote.NoActiveVote"));
            return;
        }
        
        _mcsMapVoteController.PlayerReVote(player);
    }
}