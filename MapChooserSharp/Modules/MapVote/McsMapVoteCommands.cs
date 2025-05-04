using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapVote.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapVote;

public class McsMapVoteCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsMapVoteCommands";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;
    
    private IMcsInternalMapVoteControllerApi _mcsMapVoteController = null!;
    private McsMapCycleController _mapCycleController = null!;


    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
        _mapCycleController = ServiceProvider.GetRequiredService<McsMapCycleController>();
        
        Plugin.AddCommand("css_revote", "Revote command", CommandRevote);
        Plugin.AddCommand("css_cancelvote", "Cancel the current vote", CommandCancelVote);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_revote", CommandRevote);
        Plugin.RemoveCommand("css_cancelvote", CommandCancelVote);
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
    
    [RequiresPermissions(@"css/map")]
    private void CommandCancelVote(CCSPlayerController? player, CommandInfo info)
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

        if (_mcsMapVoteController.CurrentVoteState != McsMapVoteState.Voting && _mcsMapVoteController.CurrentVoteState != McsMapVoteState.RunoffVoting && _mcsMapVoteController.CurrentVoteState != McsMapVoteState.Initializing)
        {
            Server.PrintToChatAll($"AA {_mcsMapVoteController.CurrentVoteState}");
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapVote.Command.Notification.Revote.NoActiveVote"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapVote.Command.Notification.Revote.NoActiveVote"));
            }
            return;
        }

        _mcsMapVoteController.CancelVote(player);
    }
}