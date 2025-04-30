using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapVote;

public class McsMapVoteCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsMapVoteCommands";
    public override string ModuleChatPrefix => _mcsMapVoteController.ModuleChatPrefix;
    protected override bool UseTranslationKeyInModuleChatPrefix => true;
    
    private McsMapVoteController _mcsMapVoteController = null!;


    protected override void OnAllPluginsLoaded()
    {
        _mcsMapVoteController = ServiceProvider.GetRequiredService<McsMapVoteController>();
        
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

        _mcsMapVoteController.PlayerReVote(player);
    }
}