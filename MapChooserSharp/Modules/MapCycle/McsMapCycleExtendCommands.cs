using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Modules.MapCycle;

internal sealed class McsMapCycleExtendCommands(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsMapCycleExtendCommands";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private IMcsInternalMapCycleExtendControllerApi _mcsInternalMapCycleExtendController = null!;
    private IMcsInternalMapCycleExtendVoteControllerApi _mcsInternalExtendVoteController = null!;
    private IMcsPluginConfigProvider _mcsPluginConfigProvider = null!;
    private ITimeLeftUtil _timeLeftUtil = null!;

    protected override void OnAllPluginsLoaded()
    {
        _mcsInternalMapCycleExtendController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleExtendControllerApi>();
        _mcsInternalExtendVoteController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleExtendVoteControllerApi>();
        _mcsPluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        
        
        Plugin.AddCommand("css_ext", "Vote to extend", CommandExtendMapUser);
        
        Plugin.AddCommand("css_extend", "Extend a current map", CommandExtendMap);
        Plugin.AddCommand("css_enableext", "Enable !ext command", CommandEnableExt);
        Plugin.AddCommand("css_disableext", "Disable !ext command", CommandDisableExt);
        
        Plugin.AddCommand("css_setext", "Set ext count", CommandSetExtCounts);
        
        Plugin.AddCommand("css_ve", "Starts a vote extend", CommandVoteExtend);
        Plugin.AddCommand("css_voteextend", "Starts a vote extend", CommandVoteExtend);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_ext", CommandExtendMapUser);
        
        Plugin.RemoveCommand("css_extend", CommandExtendMap);
        Plugin.RemoveCommand("css_enableext", CommandEnableExt);
        Plugin.RemoveCommand("css_disableext", CommandDisableExt);
        
        Plugin.RemoveCommand("css_setext", CommandSetExtCounts);
        
        Plugin.RemoveCommand("css_ve", CommandVoteExtend);
        Plugin.RemoveCommand("css_voteextend", CommandVoteExtend);
    }


    private void CommandExtendMapUser(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            Server.PrintToConsole("You cannot use this command from console, use css_extend instead.");
            return;
        }
        
        
        PlayerExtResult result = _mcsInternalMapCycleExtendController.CastPlayerExtVote(player);


        switch (result)
        {
            case PlayerExtResult.Success:
                break;
            
            case PlayerExtResult.AlreadyVoted:
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.ExtCommand.Notification.AlreadyVoted"));
                break;
            
            case PlayerExtResult.CommandInCooldown:
                if (_mcsPluginConfigProvider.PluginConfig.GeneralConfig.VerboseCooldownPrint)
                {
                    player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.ExtCommand.Notification.InCooldown.Verbose"));
                }
                else
                {
                    player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.ExtCommand.Notification.InCooldown"));
                }
                break;
            
            case PlayerExtResult.CommandDisabled:
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.ExtCommand.Notification.Disabled"));
                break;
            
            case PlayerExtResult.NotAllowed:
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.ExtCommand.Notification.NotAllowed"));
                break;
            
            case PlayerExtResult.ReachedLimit:
                NotifyNoExtsLeft(player);
                break;
        }
    }


    [RequiresPermissions("css/map")]
    private void CommandEnableExt(CCSPlayerController? player, CommandInfo info)
    {
        if (_mcsInternalMapCycleExtendController.UserExtsRemaining <= 0)
        {
            NotifyNoExtsLeft(player);
            return;
        }
        
        _mcsInternalMapCycleExtendController.EnablePlayerExtendCommand(player);
    }


    [RequiresPermissions("css/map")]
    private void CommandDisableExt(CCSPlayerController? player, CommandInfo info)
    {
        if (_mcsInternalMapCycleExtendController.UserExtsRemaining <= 0)
        {
            NotifyNoExtsLeft(player);
            return;
        }
        
        _mcsInternalMapCycleExtendController.DisablePlayerExtendCommand(player);
    }

    [RequiresPermissions("css/root")]
    private void CommandExtendMap(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycleExtend.Command.Admin.Notification.MapExtended.Usage"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.Command.Admin.Notification.MapExtended.Usage"));
            }
            return;
        }

        string arg1 = info.ArgByIndex(1);
        
        if (!int.TryParse(info.ArgByIndex(1), out var extendTime))
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("General.Notification.InvalidArgument.WithParam", arg1));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.InvalidArgument.WithParam", arg1));
            }
            return;
        }
        
        McsMapCycleExtendResult result = _mcsInternalMapCycleExtendController.ExtendCurrentMap(extendTime);


        if (result == McsMapCycleExtendResult.FailedToExtend)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycleExtend.Command.Admin.Notification.MapExtended.FailedToExtend"));
            }
            else
            {
                player.PrintToConsole(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.Command.Admin.Notification.MapExtended.FailedToExtend"));
            }
            
            Logger.LogWarning("Failed to extend current map, this may be caused by a misconfigured map time type.");
            return;
        }
        else if (result == McsMapCycleExtendResult.FailedTimeCannotBeZeroOrNegative)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycleExtend.Command.Admin.Notification.MapExtended.FailedToExtend.FailedTimeCannotBeZeroOrNegative"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.Command.Admin.Notification.MapExtended.FailedToExtend.FailedTimeCannotBeZeroOrNegative"));
            }
            return;
        }

        string executorName = PlayerUtil.GetPlayerName(player);
        int extendTimeAbs = Math.Abs(extendTime);

        bool isExtend = extendTime > 0;

        switch (_timeLeftUtil.ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                if (isExtend)
                {
                    PrintLocalizedChatToAll("MapCycleExtend.Command.Admin.Broadcast.MapExtended.TimeLeft", executorName, extendTimeAbs);
                    Logger.LogInformation($"Admin {executorName} extended mp_timelimit by {extendTimeAbs} minutes");
                }
                else
                {
                    PrintLocalizedChatToAll("MapCycleExtend.Command.Admin.Broadcast.MapShortened.TimeLeft", executorName, extendTimeAbs);
                    Logger.LogInformation($"Admin {executorName} shortened mp_timelimit by {extendTimeAbs} minutes");
                }
                break;
            
            case McsMapExtendType.Rounds:
                if (isExtend)
                {
                    PrintLocalizedChatToAll("MapCycleExtend.Command.Admin.Broadcast.MapExtended.Rounds", executorName, extendTimeAbs);
                    Logger.LogInformation($"Admin {executorName} extended mp_maxrounds by {extendTimeAbs} rounds");
                }
                else
                {
                    PrintLocalizedChatToAll("MapCycleExtend.Command.Admin.Broadcast.MapShortened.Rounds", executorName, extendTimeAbs);
                    Logger.LogInformation($"Admin {executorName} shortened mp_maxrounds by {extendTimeAbs} rounds");
                }
                break;
            
            case McsMapExtendType.RoundTime:
                if (isExtend)
                {
                    PrintLocalizedChatToAll("MapCycleExtend.Command.Admin.Broadcast.MapExtended.RoundTime", executorName, extendTimeAbs);
                    Logger.LogInformation($"Admin {executorName} extended mp_roundtime by {extendTimeAbs} minutes");
                }
                else
                {
                    PrintLocalizedChatToAll("MapCycleExtend.Command.Admin.Broadcast.MapShortened.RoundTime", executorName, extendTimeAbs);
                    Logger.LogInformation($"Admin {executorName} shortened mp_roundtime by {extendTimeAbs} minutes");
                }
                break;
        }
    }


    [RequiresPermissions(@"css/root")]
    private void CommandSetExtCounts(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycleExtend.ExtCommand.Admin.Broadcast.ChangeExtCount.Usage"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.ExtCommand.Admin.Broadcast.ChangeExtCount.Usage"));
            }
            return;
        }
        
        
        string arg1 = info.ArgByIndex(1);

        if (!int.TryParse(arg1, out int count))
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("General.Notification.InvalidArgument.WithParam", arg1));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.InvalidArgument.WithParam", arg1));
            }
            return;
        }


        _mcsInternalMapCycleExtendController.SetUserExtsRemaining(count);

        string executorName = PlayerUtil.GetPlayerName(player);
        
        PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Admin.Broadcast.ChangeExtCount.ChangedExtCount", executorName, count);
    }

    [RequiresPermissions(@"css/root")]
    private void CommandVoteExtend(CCSPlayerController? player, CommandInfo info)
    {
        if (info.ArgCount < 2)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycleVoteExtend.Command.Notification.Usage"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleVoteExtend.Command.Notification.Usage"));
            }
            return;
        }
        
        
        string arg1 = info.ArgByIndex(1);

        if (!int.TryParse(arg1, out int count))
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("General.Notification.InvalidArgument.WithParam", arg1));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "General.Notification.InvalidArgument.WithParam", arg1));
            }
            return;
        }


        if (count < 1)
        {
            if (player == null)
            {
                Server.PrintToConsole(LocalizeString("MapCycleVoteExtend.Command.Notification.CannotBeZeroOrNegative"));
            }
            else
            {
                player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleVoteExtend.Command.Notification.CannotBeZeroOrNegative"));
            }
            return;
        }
        
        _mcsInternalExtendVoteController.StartExtendVote(player, count);

        string executorName = PlayerUtil.GetPlayerName(player);
        Logger.LogInformation($"Admin {executorName} executed vote extend");
    }
    
    

    private void NotifyNoExtsLeft(CCSPlayerController? player)
    {
        if (player == null)
        {
            Server.PrintToConsole(LocalizeString("MapCycleExtend.ExtCommand.Notification.NoExtsRemain"));
        }
        else
        {
            player.PrintToChat(LocalizeWithPluginPrefixForPlayer(player, "MapCycleExtend.ExtCommand.Notification.NoExtsRemain"));
        }
    }
}