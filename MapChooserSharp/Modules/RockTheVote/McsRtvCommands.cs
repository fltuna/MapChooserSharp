using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.RockTheVote;

public class McsRtvCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsRtvCommands";
    public override string ModuleChatPrefix => $" {ChatColors.Green}[RTV]{ChatColors.Default}";
    
    private McsRtvController _mcsRtvController = null!;
    
    protected override void OnInitialize()
    {
        Plugin.AddCommand("css_rtv", "Rock The Vote", CommandRtv);
        Plugin.AddCommand("css_enablertv", "Enable RTV", CommandEnableRtv);
        Plugin.AddCommand("css_disablertv", "Disable RTV", CommandDisableRtv);
        
        Plugin.AddCommandListener("say", SayCommandListener, HookMode.Pre);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsRtvController = ServiceProvider.GetRequiredService<McsRtvController>();
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_rtv", CommandRtv);
        Plugin.RemoveCommand("css_enablertv", CommandEnableRtv);
        Plugin.RemoveCommand("css_disablertv", CommandDisableRtv);
        
        Plugin.RemoveCommandListener("say", SayCommandListener, HookMode.Pre);
    }


    private void CommandRtv(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;
        
        
        var status = _mcsRtvController.AddPlayerToRtv(client);


        switch (status)
        {
            case McsRtvController.PlayerRtvResult.Success:
                client.PrintToConsole("TODO_TRANSLATE| RTV OK");
                break;
            
            case McsRtvController.PlayerRtvResult.AlreadyInRtv:
                client.PrintToChat("TODO_TRANSLATE| YOU'VE ALREADY VOTED");
                break;
            
            case McsRtvController.PlayerRtvResult.CommandInCooldown:
                client.PrintToChat($"TODO_TRANSLATE| RTV IS IN COOLDOWN {(_mcsRtvController.RtvCommandUnlockTime - Server.CurrentTime):F0}s");
                break;
            
            case McsRtvController.PlayerRtvResult.CommandDisabled:
                client.PrintToChat("TODO_TRANSLATE| RTV IS DISABLED");
                break;
            
            case McsRtvController.PlayerRtvResult.AnotherVoteOngoing:
                client.PrintToChat("TODO_TRANSLATE| YOU CANNOT RTV WHILE IN VOTE");
                break;
        }
    }


    [RequiresPermissions(@"css/map")]
    private void CommandEnableRtv(CCSPlayerController? client, CommandInfo info)
    {
        Server.PrintToChatAll("TODO_TRANSLATE| ADMIN Enabled RTV");
        _mcsRtvController.EnableRtvCommand();
    }

    [RequiresPermissions(@"css/map")]
    private void CommandDisableRtv(CCSPlayerController? client, CommandInfo info)
    {
        Server.PrintToChatAll("TODO_TRANSLATE| ADMIN Disabled RTV");
        _mcsRtvController.DisableRtvCommand();
    }
    

    private HookResult SayCommandListener(CCSPlayerController? player, CommandInfo info)
    {
        if(player == null)
            return HookResult.Continue;

        if (info.ArgCount < 2)
            return HookResult.Continue;
        
        string arg1 = info.ArgByIndex(1);

        bool commandFound = false;


        if (arg1.Equals("rtv", StringComparison.OrdinalIgnoreCase))
        {
            player.ExecuteClientCommandFromServer("css_rtv");
            commandFound = true;
        }

        return commandFound ? HookResult.Handled : HookResult.Continue;
    }
}