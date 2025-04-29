using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapVote;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Modules.RockTheVote;

public class McsRtvCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsRtvCommands";
    public override string ModuleChatPrefix => _mcsRtvController.ModuleChatPrefix;
    protected override bool UseTranslationKeyInModuleChatPrefix => true;
    
    private McsRtvController _mcsRtvController = null!;
    private McsMapCycleController _mcsMapCycleController = null!;
    
    protected override void OnInitialize()
    {
        Plugin.AddCommand("css_rtv", "Rock The Vote", CommandRtv);
        Plugin.AddCommand("css_enablertv", "Enable RTV", CommandEnableRtv);
        Plugin.AddCommand("css_disablertv", "Disable RTV", CommandDisableRtv);
        Plugin.AddCommand("css_forcertv", "Force RTV", CommandForceRtv);
        
        Plugin.AddCommandListener("say", SayCommandListener, HookMode.Pre);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsRtvController = ServiceProvider.GetRequiredService<McsRtvController>();
        _mcsMapCycleController = ServiceProvider.GetRequiredService<McsMapCycleController>();
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_rtv", CommandRtv);
        Plugin.RemoveCommand("css_enablertv", CommandEnableRtv);
        Plugin.RemoveCommand("css_disablertv", CommandDisableRtv);
        Plugin.RemoveCommand("css_forcertv", CommandForceRtv);
        
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
                break;
            
            case McsRtvController.PlayerRtvResult.AlreadyInRtv:
                client.PrintToChat(LocalizeWithModulePrefixForPlayer(client, "RTV.Notification.AlreadyVoted"));
                break;
            
            case McsRtvController.PlayerRtvResult.CommandInCooldown:
                // TODO() Toggle verbose option in plugin config
                client.PrintToChat(LocalizeWithModulePrefixForPlayer(client, "RTV.Notification.IsInCooldown.Verbose", $"{_mcsRtvController.RtvCommandUnlockTime - Server.CurrentTime:F0}"));
                break;
            
            case McsRtvController.PlayerRtvResult.CommandDisabled:
                client.PrintToChat(LocalizeWithModulePrefixForPlayer(client, "RTV.Notification.Disabled"));
                break;
            
            case McsRtvController.PlayerRtvResult.AnotherVoteOngoing:
                client.PrintToChat(LocalizeWithModulePrefixForPlayer(client, "RTV.Notification.YouCantRtvWhileVote"));
                break;
        }
    }


    [RequiresPermissions(@"css/map")]
    private void CommandEnableRtv(CCSPlayerController? client, CommandInfo info)
    {
        if (_mcsRtvController.RtvCommandStatus == McsRtvController.RtvStatus.AnotherVoteOngoing)
        {
            if (client == null)
            {
                Server.PrintToConsole(LocalizeString("RTV.Notification.Admin.AnotherVoteInProgress"));
            }
            else
            {
                client.PrintToChat(LocalizeWithModulePrefixForPlayer(client, "RTV.Notification.Admin.AnotherVoteInProgress"));
            }
            return;
        }

        string executorName = PlayerUtil.GetPlayerName(client);
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.Admin.EnabledRtv", executorName);
        
        _mcsRtvController.EnableRtvCommand();
    }

    [RequiresPermissions(@"css/map")]
    private void CommandDisableRtv(CCSPlayerController? client, CommandInfo info)
    {
        if (_mcsRtvController.RtvCommandStatus == McsRtvController.RtvStatus.AnotherVoteOngoing)
        {
            if (client == null)
            {
                Server.PrintToConsole(LocalizeString("RTV.Notification.Admin.AnotherVoteInProgress"));
            }
            else
            {
                client.PrintToChat(LocalizeWithModulePrefixForPlayer(client, "RTV.Notification.Admin.AnotherVoteInProgress"));
            }
            return;
        }

        string executorName = PlayerUtil.GetPlayerName(client);
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.Admin.DisableRtv", executorName);
        
        _mcsRtvController.DisableRtvCommand();
    }

    [RequiresPermissions(@"css/map")]
    private void CommandForceRtv(CCSPlayerController? client, CommandInfo info)
    {
        if (_mcsRtvController.RtvCommandStatus == McsRtvController.RtvStatus.AnotherVoteOngoing)
        {
            if (client == null)
            {
                Server.PrintToConsole(LocalizeString("RTV.Notification.Admin.AnotherVoteInProgress"));
            }
            else
            {
                client.PrintToChat(LocalizeWithModulePrefixForPlayer(client, "RTV.Notification.Admin.AnotherVoteInProgress"));
            }
            return;
        }

    
        if (_mcsMapCycleController.IsNextMapConfirmed)
        {
            _mcsRtvController.ChangeToNextMap();
            return;
        }
        
        string executorName = PlayerUtil.GetPlayerName(client);
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.Admin.ForceRtv", executorName);
        
        _mcsRtvController.EnableRtvCommand();
        _mcsRtvController.InitiateRtvVote();
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