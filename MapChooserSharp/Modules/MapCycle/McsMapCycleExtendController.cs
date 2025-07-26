using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.MapCycle;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Modules.MapCycle;

internal class McsMapCycleExtendController(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider), IMcsInternalMapCycleExtendControllerApi
{
    public override string PluginModuleName => "McsMapCycleExtendController";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private ITimeLeftUtil _timeLeftUtil = null!;
    private IMcsInternalMapConfigProviderApi _internalMapConfigProvider = null!;
    private IMcsPluginConfigProvider _pluginConfigProvider = null!;
    private IMcsInternalMapCycleControllerApi _internalMapCycleController = null!;
    private IMcsInternalEventManager _internalEventManager = null!;

    public readonly FakeConVar<float> ExtUserVoteStartThreshold = 
        new("mcs_ext_user_vote_threshold", 
            "How many percent to require extend a map by users?", 0.5F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1.0F));

    
    public ExtStatus ExtCommandStatus { get; private set; } = ExtStatus.Enabled;
    
    private int CountsRequiredToUserExt { get; set; } = 0;

    public float ExtCommandUnlockTime { get; private set; } = 0.0F;
    
    private readonly HashSet<int> _extCommandVoteParticipants = new();

    public int ExtUsageRemaining { get; private set; }

    public void SetExtUsageRemaining(int userExtsRemaining)
    {
        if (userExtsRemaining < 0)
            return;

        ExtUsageRemaining = userExtsRemaining;
        ExtCommandStatus = ExtStatus.Enabled;
    }

    protected override void OnInitialize()
    {
        TrackConVar(ExtUserVoteStartThreshold);
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMcsInternalMapCycleExtendControllerApi>(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
        _internalMapConfigProvider = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        _pluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        _internalMapCycleController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        _internalEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        
        Plugin.RegisterListener<Listeners.OnMapStart>((_) =>
        {
            ResetExtCmdStatus();
            
            Server.NextFrame(() =>
            {
                GetExtCountsFromConfig();
            });
        });
        
        Plugin.RegisterListener<Listeners.OnClientPutInServer>((slot) =>
        {
            var player = Utilities.GetPlayerFromSlot(slot);
            
            if (player == null || player.IsBot || player.IsHLTV)
                return;
            
            RefreshExtRequiredCounts();
        });
        
        Plugin.RegisterListener<Listeners.OnClientDisconnect>((slot) =>
        {
            // This listener will not verfy bot or real player, since Remove method is doesn't do anything when not contained.
            _extCommandVoteParticipants.Remove(slot);
            RefreshExtRequiredCounts();
        });


        if (hotReload)
        {
            GetExtCountsFromConfig();
            RefreshExtRequiredCounts();
        }
    }

    protected override void OnUnloadModule()
    {
    }

    public PlayerExtResult CastPlayerExtVote(CCSPlayerController player)
    {
        if (ExtCommandStatus == ExtStatus.Disabled)
            return PlayerExtResult.CommandDisabled;

        if (ExtCommandStatus == ExtStatus.InCooldown)
            return PlayerExtResult.CommandInCooldown;

        if (ExtCommandStatus == ExtStatus.ReachedLimit || ExtUsageRemaining <= 0)
            return PlayerExtResult.ReachedLimit;
            
        if (!_extCommandVoteParticipants.Add(player.Slot))
            return PlayerExtResult.AlreadyVoted;

        var cmdExecutedEvt = new McsExtCommandExecutedEvent(GetTextWithPluginPrefix(""), player);
        var result = _internalEventManager.FireEvent(cmdExecutedEvt);

        
        if (result >= McsEventResult.Stop)
        {
            DebugLogger.LogInformation("Player ext command cast is canncelled by Eternal API");
            return PlayerExtResult.NotAllowed;
        }

        PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Broadcast.VoteCast", player.PlayerName ,_extCommandVoteParticipants.Count, CountsRequiredToUserExt);

        if (_extCommandVoteParticipants.Count >= CountsRequiredToUserExt)
        {
            CastExtend();
        }
        
        return PlayerExtResult.Success;
    }

    private void CastExtend()
    {
        var currentMap = _internalMapCycleController.CurrentMap;

        int timesToExtend = 0;

        var extendType = _timeLeftUtil.ExtendType;
        
        if (currentMap == null)
        {
            switch (extendType)
            {
                case McsMapExtendType.Rounds:
                    timesToExtend = _pluginConfigProvider.PluginConfig.MapCycleConfig.FallbackExtendRoundsPerExtends;
                    break;
                    
                case McsMapExtendType.RoundTime:
                case McsMapExtendType.TimeLimit:
                    timesToExtend = _pluginConfigProvider.PluginConfig.MapCycleConfig.FallbackExtendTimePerExtends;
                    break;
            }
        }
        else
        {
            switch (extendType)
            {
                case McsMapExtendType.Rounds:
                    timesToExtend = currentMap.ExtendRoundsPerExtends;
                    break;
                    
                case McsMapExtendType.RoundTime:
                case McsMapExtendType.TimeLimit:
                    timesToExtend = currentMap.ExtendTimePerExtends;
                    break;
            }
        }
            
        // Ignore status return value, because shouldn't be failed.
        ExtendCurrentMap(timesToExtend);
        ExtUsageRemaining--;

        switch (extendType)
        {
            case McsMapExtendType.TimeLimit:
                PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Broadcast.MapExtended.TimeLeft", timesToExtend);
                break;
            
            case McsMapExtendType.RoundTime:
                PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Broadcast.MapExtended.RoundTime", timesToExtend);
                break;
            
            case McsMapExtendType.Rounds:
                PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Broadcast.MapExtended.Rounds", timesToExtend);
                break;
        }
            
        ResetExtCmdStatus();
            
            
        if (ExtUsageRemaining <= 0)
        {
            ExtCommandStatus = ExtStatus.ReachedLimit;
            PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Broadcast.MapExtended.NoExtsRemain");
        }
        else
        {
            PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Broadcast.MapExtended.ExtsRemain", ExtUsageRemaining);
        }
    }

    public McsMapCycleExtendResult ExtendCurrentMap(int extendTime)
    {
        _timeLeftUtil.ReDetermineExtendType();
        switch (_timeLeftUtil.ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                if (_timeLeftUtil.TimeLimit + extendTime < 1)
                    return McsMapCycleExtendResult.FailedTimeCannotBeZeroOrNegative;
                
                if (_timeLeftUtil.ExtendTimeLimit(extendTime))
                    return McsMapCycleExtendResult.Extended;
                
                break;
            
            case McsMapExtendType.Rounds:
                if (_timeLeftUtil.RoundsLeft + extendTime < 1)
                    return McsMapCycleExtendResult.FailedTimeCannotBeZeroOrNegative;

                if (_timeLeftUtil.ExtendRounds(extendTime))
                    return McsMapCycleExtendResult.Extended;
                
                break;
            
            case McsMapExtendType.RoundTime:
                if (_timeLeftUtil.RoundTimeLeft + extendTime < 1)
                    return McsMapCycleExtendResult.FailedTimeCannotBeZeroOrNegative;

                if (_timeLeftUtil.ExtendRoundTime(extendTime))
                    return McsMapCycleExtendResult.Extended;
                
                break;
        }

        return McsMapCycleExtendResult.FailedToExtend;
    }

    public void EnablePlayerExtCommand(CCSPlayerController? player = null, bool silently = false)
    {
        ExtCommandStatus = ExtStatus.Enabled;
        
        if(silently)
            return;
        
        string executorName = PlayerUtil.GetPlayerName(player);
        PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Admin.Broadcast.EnabledExt");
        Logger.LogInformation($"Admin {executorName} is enabled !ext command");
    }

    public void DisablePlayerExtCommand(CCSPlayerController? player = null, bool silently = false)
    {
        ExtCommandStatus = ExtStatus.Disabled;
        
        if(silently)
            return;
        
        string executorName = PlayerUtil.GetPlayerName(player);
        PrintLocalizedChatToAll("MapCycleExtend.ExtCommand.Admin.Broadcast.DisabledExt");
        Logger.LogInformation($"Admin {executorName} is disabled !ext command");
    }

    private void ResetExtCmdStatus()
    {
        ExtCommandStatus = ExtStatus.Enabled;
        _extCommandVoteParticipants.Clear();
    }

    private void RefreshExtRequiredCounts()
    {
        CountsRequiredToUserExt = (int)Math.Truncate(Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false }) * ExtUserVoteStartThreshold.Value);

        if (CountsRequiredToUserExt <= 0)
        {
            CountsRequiredToUserExt = 1;
        }
    }

    private void GetExtCountsFromConfig()
    {
        var currentMap = _internalMapCycleController.CurrentMap;

        if (currentMap != null)
        {
            ExtUsageRemaining = currentMap.MaxExtCommandUses;
        }
        else
        {
            ExtUsageRemaining = _pluginConfigProvider.PluginConfig.MapCycleConfig.FallbackMaxExtCommandUses;
        }
    }
}