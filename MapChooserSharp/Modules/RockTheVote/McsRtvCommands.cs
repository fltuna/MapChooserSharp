using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.API.RtvController;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using MapChooserSharp.Modules.RockTheVote.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.RockTheVote;

public sealed class McsRtvCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsRtvCommands";
    public override string ModuleChatPrefix => _mcsRtvController.ModuleChatPrefix;
    protected override bool UseTranslationKeyInModuleChatPrefix => true;
    
    private IMcsInternalRtvControllerApi _mcsRtvController = null!;
    private IMcsPluginConfigProvider _mcsPluginConfigProvider = null!;
    
    protected override void OnAllPluginsLoaded()
    {
        _mcsRtvController = ServiceProvider.GetRequiredService<IMcsInternalRtvControllerApi>();
        _mcsPluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        
        Plugin.AddCommand("css_rtv", "Rock The Vote", CommandRtv);
        Plugin.AddCommand("css_enablertv", "Enable RTV", CommandEnableRtv);
        Plugin.AddCommand("css_disablertv", "Disable RTV", CommandDisableRtv);
        Plugin.AddCommand("css_forcertv", "Force RTV", CommandForceRtv);
        
        Plugin.AddCommandListener("say", SayCommandListener, HookMode.Pre);
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
            case PlayerRtvResult.Success:
                break;
            
            case PlayerRtvResult.AlreadyInRtv:
                client.PrintToChat(LocalizeWithModulePrefix(client, "RTV.Notification.AlreadyVoted"));
                break;
            
            case PlayerRtvResult.CommandInCooldown:
                if (_mcsPluginConfigProvider.PluginConfig.GeneralConfig.VerboseCooldownPrint)
                {
                    client.PrintToChat(LocalizeWithModulePrefix(client, "RTV.Notification.IsInCooldown.Verbose", $"{_mcsRtvController.RtvCommandUnlockTime - Server.CurrentTime:F0}"));
                }
                else
                {
                    client.PrintToChat(LocalizeWithModulePrefix(client, "RTV.Notification.IsInCooldown.Normal"));
                }
                
                break;
            
            case PlayerRtvResult.CommandDisabled:
                client.PrintToChat(LocalizeWithModulePrefix(client, "RTV.Notification.Disabled"));
                break;
            
            case PlayerRtvResult.AnotherVoteOngoing:
                client.PrintToChat(LocalizeWithModulePrefix(client, "RTV.Notification.YouCantRtvWhileVote"));
                break;
            
            case PlayerRtvResult.NotAllowed:
                // Do nothing, because this only happen when cancelled through API, so APIs responsibility
                break;
            
            case PlayerRtvResult.RtvTriggeredAlready:
                client.PrintToChat(LocalizeWithModulePrefix(client, "RTV.Notification.AlreadyTriggered"));
                break;
        }
    }


    [RequiresPermissions(@"css/map")]
    private void CommandEnableRtv(CCSPlayerController? client, CommandInfo info)
    {
        if (IsAnotherVOteOngoing())
        {
            NotifyAnotherVoteOnGoing(client);
            return;
        }

        if (CheckRtvAlreadyTriggered())
        {
            NotifyRtvAlreadyTriggered(client);
            return;
        }
        
        _mcsRtvController.EnableRtvCommand(client);
    }

    [RequiresPermissions(@"css/map")]
    private void CommandDisableRtv(CCSPlayerController? client, CommandInfo info)
    {
        if (IsAnotherVOteOngoing())
        {
            NotifyAnotherVoteOnGoing(client);
            return;
        }

        if (CheckRtvAlreadyTriggered())
        {
            NotifyRtvAlreadyTriggered(client);
            return;
        }
        
        _mcsRtvController.DisableRtvCommand(client);
    }

    [RequiresPermissions(@"css/map")]
    private void CommandForceRtv(CCSPlayerController? client, CommandInfo info)
    {
        if (IsAnotherVOteOngoing())
        {
            NotifyAnotherVoteOnGoing(client);
            return;
        }

        if (CheckRtvAlreadyTriggered())
        {
            NotifyRtvAlreadyTriggered(client);
            return;
        }

        _mcsRtvController.InitiateForceRtvVote(client);
    }

    
    private void NotifyAnotherVoteOnGoing(CCSPlayerController? client)
    {
        PrintMessageToServerOrPlayerChat(client, LocalizeWithPluginPrefix(client, "RTV.Notification.Admin.AnotherVoteInProgress"));
    }
    
    private bool IsAnotherVOteOngoing()
    {
        if (_mcsRtvController.RtvCommandStatus == RtvStatus.AnotherVoteOngoing)
            return true;
        
        return false;
    }


    private void NotifyRtvAlreadyTriggered(CCSPlayerController? client)
    {
        PrintMessageToServerOrPlayerChat(client, LocalizeWithPluginPrefix(client, "RTV.Notification.Admin.AlreadyTriggered"));
    }
    
    private bool CheckRtvAlreadyTriggered()
    {
        if (_mcsRtvController.RtvCommandStatus == RtvStatus.Triggered)
            return true;
        
        return false;
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