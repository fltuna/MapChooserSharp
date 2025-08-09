using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using MapChooserSharp.API.MapCycleController;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NativeVoteAPI;
using NativeVoteAPI.API;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;

namespace MapChooserSharp.Modules.MapCycle;

internal class McsMapCycleExtendVoteController(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider), IMcsInternalMapCycleExtendVoteControllerApi
{
    public override string PluginModuleName => "McsMapCycleExtendVoteController";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    public readonly FakeConVar<float> VoteExtendSuccessThreshold = 
        new("mcs_vote_extend_success_threshold", 
            "How many percent to require extend a map by votes?", 0.5F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1.0F));

    public readonly FakeConVar<float> VoteExtendVoteTime = 
        new("mcs_vote_extend_vote_time", 
            "How many seconds to wait vote ends", 15.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(10.0F, 60.0F));
    
    // I know this global variable is not good, but it requires for less complexity
    private int TimesToExtend { get; set; }

    private INativeVoteApi? _nativeVoteApi;
    private IMcsInternalMapCycleExtendControllerApi _mcsInternalMapCycleExtendControllerApi = null!;
    private ITimeLeftUtil _timeLeftUtil = null!;
    
    private const string NativeVoteIdentifier = "MapChooserSharp:ExtendVote";

    protected override void OnInitialize()
    {
        TrackConVar(VoteExtendSuccessThreshold);
        TrackConVar(VoteExtendVoteTime);
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMcsInternalMapCycleExtendVoteControllerApi>(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        try
        {
            _nativeVoteApi = INativeVoteApi.Capability.Get();
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Failed to find required dependency: NativeVoteAPI. Plugin load interrupted.");
        }

        if (_nativeVoteApi == null)
            throw new InvalidOperationException("Failed to find required dependency: NativeVoteAPI. Plugin load interrupted.");
        
        _nativeVoteApi.OnVotePass += OnVotePass;
        _nativeVoteApi.OnVoteFail += OnVoteFail;
        
        _mcsInternalMapCycleExtendControllerApi = ServiceProvider.GetRequiredService<IMcsInternalMapCycleExtendControllerApi>();
        _timeLeftUtil = ServiceProvider.GetRequiredService<ITimeLeftUtil>();
    }

    protected override void OnUnloadModule()
    {
        if (_nativeVoteApi != null)
        {
            _nativeVoteApi.OnVotePass -= OnVotePass;
            _nativeVoteApi.OnVoteFail -= OnVoteFail;
        }
    }
    
    public void StartExtendVote(CCSPlayerController? client, int extendTime)
    {
        // This is shouldn't be happened, but just in case.
        if (_nativeVoteApi == null)
            return;
        
        
        
        string executorName = PlayerUtil.GetPlayerName(client);

        DebugLogger.LogDebug($"[VoteExtend] [Admin {executorName}] trying to vote for extend.");

        if (_nativeVoteApi.GetCurrentVoteState() != NativeVoteState.NoActiveVote)
        {
            DebugLogger.LogDebug($"[VoteExtend] [Admin {executorName}] Already an active vote.");
            PrintMessageToServerOrPlayerChat(client, LocalizeWithPluginPrefix(client, "MapCycleVoteExtend.Command.Notification.AnotherVoteInProgress"));
            return;
        }

        TimesToExtend = extendTime;
        var potentialClients = Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }).ToList();
        var potentialClientsIndex = potentialClients.Select(p => p.Index).ToList();

        string detailsString = "";

        switch (_timeLeftUtil.ExtendType)
        {
            case McsMapExtendType.TimeLimit:
                detailsString = LocalizeString(null, "MapCycleVoteExtend.Vote.DetailsString.TimeLeft", TimesToExtend);
                break;
            
            case McsMapExtendType.RoundTime:
                detailsString = LocalizeString(null, "MapCycleVoteExtend.Vote.DetailsString.RoundTime", TimesToExtend);
                break;
            
            case McsMapExtendType.Rounds:
                detailsString = LocalizeString(null, "MapCycleVoteExtend.Vote.DetailsString.Rounds", TimesToExtend);
                break;
        }
        
        string displayString = "#SFUI_vote_passed_nextlevel_extend";

        // 99 means Server
        int slot = client?.Slot ?? 99;

        NativeVoteInfo nInfo = new NativeVoteInfo(NativeVoteIdentifier, displayString,
            detailsString, potentialClientsIndex, VoteThresholdType.Percentage,
            VoteExtendSuccessThreshold.Value, VoteExtendVoteTime.Value, initiator: slot);

        NativeVoteState state = _nativeVoteApi.InitiateVote(nInfo);


        if (state == NativeVoteState.InitializeAccepted)
        {
            DebugLogger.LogDebug($"[VoteExtend] [Admin {executorName}] extend vote initiated. Vote Identifier: {nInfo.voteIdentifier}");
            
            PrintLocalizedChatToAll("MapCycleVoteExtend.Vote.Broadcast.VoteInitiated", executorName);
        }
        else
        {
            DebugLogger.LogDebug($"[VoteExtend] [Admin {executorName}] extend vote initiation failed. Vote Identifier: {nInfo.voteIdentifier}");
            PrintMessageToServerOrPlayerChat(client, LocalizeWithPluginPrefix(client, "MapCycleVoteExtend.Command.Notification.FailedToInitiateVote"));
        }
    }
    
    private void OnVotePass(YesNoVoteInfo? info)
    {
        if (info == null)
            return;

        if (info.VoteInfo.voteIdentifier != NativeVoteIdentifier)
            return;
        
        McsMapCycleExtendResult result = _mcsInternalMapCycleExtendControllerApi.ExtendCurrentMap(TimesToExtend);

        if (result == McsMapCycleExtendResult.Extended)
        {
            switch (_timeLeftUtil.ExtendType)
            {
                case McsMapExtendType.TimeLimit:
                    PrintLocalizedChatToAll("MapCycleVoteExtend.Vote.Broadcast.Extended.TimeLeft", TimesToExtend);
                    break;
                
                case McsMapExtendType.RoundTime:
                    PrintLocalizedChatToAll("MapCycleVoteExtend.Vote.Broadcast.Extended.RoundTime", TimesToExtend);
                    break;
                
                case McsMapExtendType.Rounds:
                    PrintLocalizedChatToAll("MapCycleVoteExtend.Vote.Broadcast.Extended.Rounds", TimesToExtend);
                    break;
            }
        }
        else
        {
            PrintLocalizedChatToAll("MapCycleVoteExtend.Vote.Broadcast.ExtendFailed");
        }
    }

    private void OnVoteFail(YesNoVoteInfo? info)
    {
        if (info == null)
            return;

        if (info.VoteInfo.voteIdentifier != NativeVoteIdentifier)
            return;

        PrintLocalizedChatToAll("MapCycleVoteExtend.Vote.Broadcast.VoteFailed");
    }
}