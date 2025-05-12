using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
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
    private ITimeLeftUtil _timeLeftUtil = null!;

    protected override void OnAllPluginsLoaded()
    {
        _mcsInternalMapCycleExtendController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleExtendControllerApi>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        
        Plugin.AddCommand("css_extend", "Extend a current map", CommandExtendMap);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_extend", CommandExtendMap);
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
}