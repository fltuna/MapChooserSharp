using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Timers;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.MapVote;
using MapChooserSharp.API.Events.RockTheVote;
using MapChooserSharp.API.RtvController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapCycle.Interfaces;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.RockTheVote.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace MapChooserSharp.Modules.RockTheVote;

internal sealed class McsRtvController(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider), IMcsInternalRtvControllerApi
{
    public override string PluginModuleName => "McsRtvController";
    public override string ModuleChatPrefix => "Prefix.RTV";
    protected override bool UseTranslationKeyInModuleChatPrefix => true;


    public readonly FakeConVar<float> RtvCommandUnlockTimeNextMapConfirmed =
        new("mcs_rtv_command_unlock_time_next_map_confirmed",
            "Seconds to take unlock RTV command after next map confirmed in vote", 60.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1200.0F));

    public readonly FakeConVar<float> RtvCommandUnlockTimeMapNotChanged =
        new("mcs_rtv_command_unlock_time_map_dont_change",
            "Seconds to take unlock RTV command after map is not changed in rtv vote", 240.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1200.0F));

    public readonly FakeConVar<float> RtvCommandUnlockTimeMapExtend =
        new("mcs_rtv_command_unlock_time_map_extend",
            "Seconds to take unlock RTV command after map is extended in vote", 120.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1200.0F));

    public readonly FakeConVar<float> RtvCommandUnlockTimeMapStart =
        new("mcs_rtv_command_unlock_time_map_start",
            "Seconds to take unlock RTV command after map started", 300.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1200.0F));

    public readonly FakeConVar<float> RtvVoteStartThreshold = 
        new("mcs_rtv_vote_start_threshold", 
            "How many percent to require start rtv vote?", 0.5F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1.0F));

    public readonly FakeConVar<float> MapChangeTimingAfterRtvSuccess =
        new("mcs_rtv_map_change_timing", 
            "Seconds to change map after RTV is success. Set 0.0 to change immediately", 3.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 60.0F));

    public readonly FakeConVar<int> MinimumRtvRequirements =
        new("mcs_rtv_minimum_requirements",
            "Minimum RTV requirements to start RTV vote. Set 0 to disable this requirement", 0, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 64));
    
    
    public RtvStatus RtvCommandStatus { get; private set; } = RtvStatus.Enabled;

    public float RtvCommandUnlockTime { get; private set; } = 0.0F;
    
    private Timer? RtvCommandUnlockTimer { get; set; }
    
    private IMcsInternalEventManager _mcsEventManager = null!;
    private IMcsInternalMapVoteControllerApi _mcsMapVoteController = null!;
    private IMcsInternalMapCycleControllerApi _mcsMapCycleController = null!;
    private IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = null!;


    private readonly HashSet<int> _rtvVoteParticipants = new();

    private int CountsRequiredToInitiateRtv { get; set; } = 0;

    protected override void OnInitialize()
    {
        TrackConVar(RtvCommandUnlockTimeNextMapConfirmed);
        TrackConVar(RtvCommandUnlockTimeMapNotChanged);
        TrackConVar(RtvCommandUnlockTimeMapExtend);
        TrackConVar(RtvCommandUnlockTimeMapStart);
        TrackConVar(RtvVoteStartThreshold);
        TrackConVar(MapChangeTimingAfterRtvSuccess);
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMcsInternalRtvControllerApi>(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _mcsMapVoteController = ServiceProvider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
        _mcsMapCycleController = ServiceProvider.GetRequiredService<IMcsInternalMapCycleControllerApi>();
        _mcsInternalMapConfigProviderApi = ServiceProvider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
        
        _mcsEventManager.RegisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.RegisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        _mcsEventManager.RegisterEventHandler<McsMapExtendEvent>(OnMapExtended);
            
        _mcsEventManager.RegisterEventHandler<McsMapVoteInitiatedEvent>(OnMapVoteInitialization);
        _mcsEventManager.RegisterEventHandler<McsMapVoteCancelledEvent>(OnVoteCancelled);
        
        
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        

        Plugin.RegisterListener<Listeners.OnClientPutInServer>((slot) =>
        {
            if (Utilities.GetPlayerFromSlot(slot)?.IsBot ?? true)
                return;
            
            if (Utilities.GetPlayerFromSlot(slot)?.IsHLTV ?? true)
                return;

            RefreshRtvRequirementCounts();
        });

        Plugin.RegisterListener<Listeners.OnClientDisconnect>((slot) =>
        {
            _rtvVoteParticipants.Remove(slot);
            RefreshRtvRequirementCounts();
        });

        if (hotReload)
        {
            RefreshRtvRequirementCounts();
        }
    }

    protected override void OnUnloadModule()
    {
        _mcsEventManager.UnregisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.UnregisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        _mcsEventManager.UnregisterEventHandler<McsMapExtendEvent>(OnMapExtended);
        
        _mcsEventManager.UnregisterEventHandler<McsMapVoteInitiatedEvent>(OnMapVoteInitialization);
        _mcsEventManager.UnregisterEventHandler<McsMapVoteCancelledEvent>(OnVoteCancelled);
    }
    
    
    #region API
    

    public PlayerRtvResult AddPlayerToRtv(CCSPlayerController player)
    {
        if (RtvCommandStatus == RtvStatus.Triggered)
            return PlayerRtvResult.RtvTriggeredAlready;
        
        if (RtvCommandStatus == RtvStatus.AnotherVoteOngoing)
            return PlayerRtvResult.AnotherVoteOngoing;
        
        if (RtvCommandStatus == RtvStatus.Disabled)
            return PlayerRtvResult.CommandDisabled;

        if (RtvCommandStatus == RtvStatus.InCooldown)
            return PlayerRtvResult.CommandInCooldown;
        
        if (!_rtvVoteParticipants.Add(player.Slot))
            return PlayerRtvResult.AlreadyInRtv;
        
        var rtvCastEvent = new McsPlayerRtvCastEvent(player, GetTextWithModulePrefix(""));
        var result = _mcsEventManager.FireEvent(rtvCastEvent);
        
        if (result > McsEventResult.Handled)
        {
            DebugLogger.LogInformation("Rtv cast event is cancelled by a another plugin.");
            return PlayerRtvResult.NotAllowed;
        }

        int requiredCount = GetMinimumRtvRequirementCounts();
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.PlayerCastRtv", player.PlayerName, _rtvVoteParticipants.Count, requiredCount);
        

        if (_rtvVoteParticipants.Count >= requiredCount)
        {
            if (_mcsMapCycleController.IsNextMapConfirmed)
            {
                ChangeToNextMap();
            }
            else
            {
                InitiateRtvVote();
            }
        }
        
        return PlayerRtvResult.Success;
    }

    public void InitiateRtvVote()
    {
        RtvCommandStatus = RtvStatus.Triggered;
        _mcsMapVoteController.InitiateVote(true);
    }

    public void InitiateForceRtvVote(CCSPlayerController? client)
    {
        var forceRtvEvent = new McsAdminForceRtvEvent(client, GetTextWithModulePrefix(""));
        var result = _mcsEventManager.FireEvent(forceRtvEvent);
        
        if (result > McsEventResult.Handled)
        {
            DebugLogger.LogInformation("Admin force rtv event is cancelled by a another plugin.");
            return;
        }
        
        if (_mcsMapCycleController.IsNextMapConfirmed)
        {
            ChangeToNextMap();
            return;
        }
        
        string executorName = PlayerUtil.GetPlayerName(client);
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.Admin.ForceRtv", executorName);
        Logger.LogInformation($"Admin {executorName} is forcefully triggered the RTV!");
        
        EnableRtvCommand(client, true);
        InitiateRtvVote();
    }

    public void EnableRtvCommand(CCSPlayerController? client, bool silently = false)
    {
        RtvCommandStatus = RtvStatus.Enabled;
        RtvCommandUnlockTimer?.Kill();

        if (silently)
            return;
        
        string executorName = PlayerUtil.GetPlayerName(client);
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.Admin.EnabledRtv", executorName);
        Logger.LogInformation($"Admin {executorName} is enabled RTV");
    }

    public void DisableRtvCommand(CCSPlayerController? client = null, bool silently = false)
    {
        RtvCommandStatus = RtvStatus.Disabled;
        RtvCommandUnlockTimer?.Kill();
        
        if (silently)
            return;

        string executorName = PlayerUtil.GetPlayerName(client);
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.Admin.DisableRtv", executorName);
        Logger.LogInformation($"Admin {executorName} is disabled RTV");
    }

    private void ChangeToNextMap()
    {
        RtvCommandStatus = RtvStatus.Triggered;
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.ChangeToNextMapImmediately", _mcsInternalMapConfigProviderApi.GetMapName(_mcsMapCycleController.NextMap!), MapChangeTimingAfterRtvSuccess.Value);
        _mcsMapCycleController.ChangeToNextMap(MapChangeTimingAfterRtvSuccess.Value);
    }

    #endregion
    
    private void OnNextMapConfirmed(McsNextMapConfirmedEvent @event)
    {
        ResetRtvStatus();
        CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeOverride.NextMapConfirm);
    }

    private void OnMapNotChanged(McsMapNotChangedEvent @event)
    {
        ResetRtvStatus();
        CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeOverride.MapNotChanged);
    }

    private void OnMapExtended(McsMapExtendEvent @event)
    {
        ResetRtvStatus();
        CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeOverride.MapExtended);
    }

    private void OnVoteCancelled(McsMapVoteCancelledEvent @event)
    {
        ResetRtvStatus();
        CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeOverride.MapNotChanged);
    }

    private void OnMapVoteInitialization(McsMapVoteInitiatedEvent @event)
    {
        RtvCommandStatus = RtvStatus.AnotherVoteOngoing;
    }

    private void OnMapStart(string _)
    {
        ResetRtvStatus();
        CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeOverride.MapStart);
    }

    private void CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeOverride timeOverride)
    {
        RtvCommandStatus = RtvStatus.InCooldown;
        
        switch (timeOverride)
        {
            case RtvCommandUnlockTimeOverride.NextMapConfirm:
                CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeNextMapConfirmed.Value);
                break;
            
            case RtvCommandUnlockTimeOverride.MapExtended:
                CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeMapExtend.Value);
                break;
            
            case RtvCommandUnlockTimeOverride.MapNotChanged:
                CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeMapNotChanged.Value);
                break;
            
            case RtvCommandUnlockTimeOverride.MapStart:
                CreateRtvCommandUnlockTimer(RtvCommandUnlockTimeMapStart.Value);
                break;
        }
        
    }

    private void CreateRtvCommandUnlockTimer(float timeSeconds)
    {
        RtvCommandUnlockTimer?.Kill();
        RtvCommandUnlockTime = Server.CurrentTime + timeSeconds;
        RtvCommandUnlockTimer = Plugin.AddTimer(timeSeconds, () =>
        {
            // Because RtvStatus.Disabled is can only be set from Admin command
            if (RtvCommandStatus == RtvStatus.Disabled)
                return;

            RtvCommandStatus = RtvStatus.Enabled;
        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    private void ResetRtvStatus()
    {
        RtvCommandStatus = RtvStatus.Enabled;
        RtvCommandUnlockTimer?.Kill();
        _rtvVoteParticipants.Clear();
    }
    
    private void RefreshRtvRequirementCounts()
    {
        CountsRequiredToInitiateRtv = (int)Math.Truncate(Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false }) * RtvVoteStartThreshold.Value);
    }

    private int GetMinimumRtvRequirementCounts()
    {
        int count = Math.Max(CountsRequiredToInitiateRtv, MinimumRtvRequirements.Value);
        
        if (count <= 0)
            return 1;
        
        return count;
    }
    
    
    private enum RtvCommandUnlockTimeOverride
    {
        NextMapConfirm = 0,
        MapExtended,
        MapNotChanged,
        MapStart,
    }
}

