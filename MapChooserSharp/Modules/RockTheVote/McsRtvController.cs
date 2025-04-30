using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.MapVote;
using MapChooserSharp.API.Events.RockTheVote;
using MapChooserSharp.API.RtvController;
using MapChooserSharp.Interfaces;
using MapChooserSharp.Modules.MapCycle;
using MapChooserSharp.Modules.MapVote;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;
using TNCSSPluginFoundation.Utils.Entity;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace MapChooserSharp.Modules.RockTheVote;

internal class McsRtvController(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider), IMcsRtvControllerApi
{
    public override string PluginModuleName => "McsRtvController";
    public override string ModuleChatPrefix => "Prefix.RTV";
    protected override bool UseTranslationKeyInModuleChatPrefix => true;


    public FakeConVar<float> RtvCommandUnlockTimeNextMapConfirmed =
        new("mcs_rtv_command_unlock_time_next_map_confirmed",
            "Seconds to take unlock RTV command after next map confirmed in vote", 60.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1200.0F));

    public FakeConVar<float> RtvCommandUnlockTimeMapNotChanged =
        new("mcs_rtv_command_unlock_time_map_dont_change",
            "Seconds to take unlock RTV command after map is not changed in rtv vote", 240.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1200.0F));

    public FakeConVar<float> RtvCommandUnlockTimeMapExtend =
        new("mcs_rtv_command_unlock_time_map_extend",
            "Seconds to take unlock RTV command after map is extended in vote", 120.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1200.0F));

    public FakeConVar<float> RtvVoteStartThreshold = 
        new("mcs_rtv_vote_start_threshold", 
            "How many percent to require start rtv vote?", 0.5F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1.0F));

    public FakeConVar<float> MapChangeTimingAfterRtvSuccess =
        new("mcs_rtv_map_change_timing", 
            "Seconds to change map after RTV is success. Set 0.0 to change immediately", 3.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 60.0F));

    public FakeConVar<bool> MapChangeTimingShouldRoundEnd =
        new("mcs_rtv_map_change_timing_should_round_end",
            "Map change should be round end? If true, ignores mcs_rtv_map_change_timing setting", true);

    
    
    internal RtvStatus RtvCommandStatus { get; private set; } = RtvStatus.Enabled;

    internal float RtvCommandUnlockTime { get; private set; } = 0.0F;
    
    private Timer? RtvCommandUnlockTimer { get; set; }
    
    private IMcsInternalEventManager _mcsEventManager = null!;
    private McsMapVoteController _mcsMapVoteController = null!;
    private McsMapCycleController _mcsMapCycleController = null!;


    private readonly HashSet<int> _rtvVoteParticipants = new();

    private int CountsRequiredToInitiateRtv { get; set; } = 0;
    
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnAllPluginsLoaded()
    {
        _mcsEventManager = ServiceProvider.GetRequiredService<IMcsInternalEventManager>();
        _mcsMapVoteController = ServiceProvider.GetRequiredService<McsMapVoteController>();
        _mcsMapCycleController = ServiceProvider.GetRequiredService<McsMapCycleController>();
        
        _mcsEventManager.RegisterEventHandler<McsNextMapConfirmedEvent>(OnNextMapConfirmed);
        _mcsEventManager.RegisterEventHandler<McsMapNotChangedEvent>(OnMapNotChanged);
        _mcsEventManager.RegisterEventHandler<McsMapExtendEvent>(OnMapExtended);
            
        _mcsEventManager.RegisterEventHandler<McsMapVoteInitiatedEvent>(OnMapVoteInitialization);
        _mcsEventManager.RegisterEventHandler<McsMapVoteCancelledEvent>(OnVoteCancelled);
        
        
        Plugin.RegisterListener<Listeners.OnMapStart>((mapName) =>
        {
            RtvCommandStatus = RtvStatus.Enabled;
            ResetRtvStatus();
        });
        

        Plugin.RegisterListener<Listeners.OnClientPutInServer>((slot) =>
        {
            if (Utilities.GetPlayerFromSlot(slot)?.IsBot ?? true)
                return;
            
            if (Utilities.GetPlayerFromSlot(slot)?.IsHLTV ?? true)
                return;
            
            CountsRequiredToInitiateRtv = (int)Math.Truncate(Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false }) * RtvVoteStartThreshold.Value);
        });

        Plugin.RegisterListener<Listeners.OnClientDisconnect>((slot) =>
        {
            _rtvVoteParticipants.Remove(slot);
            CountsRequiredToInitiateRtv = (int)Math.Truncate(Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false }) * RtvVoteStartThreshold.Value);
        });

        if (hotReload)
        {
            CountsRequiredToInitiateRtv = (int)Math.Truncate(Utilities.GetPlayers().Count(p => p is { IsBot: false, IsHLTV: false }) * RtvVoteStartThreshold.Value);
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
    
    
    #region Internal API
    

    internal PlayerRtvResult AddPlayerToRtv(CCSPlayerController player)
    {
        var rtvCastEvent = new McsPlayerRtvCastEvent(player, GetTextWithModulePrefix(""));
        var result = _mcsEventManager.FireEvent(rtvCastEvent);
        
        if (result > McsEventResult.Handled)
        {
            DebugLogger.LogInformation("Rtv cast event is cancelled by a another plugin.");
            return PlayerRtvResult.NotAllowed;
        }
        
        
        if (RtvCommandStatus == RtvStatus.AnotherVoteOngoing)
            return PlayerRtvResult.AnotherVoteOngoing;
        
        if (RtvCommandStatus == RtvStatus.Disabled)
            return PlayerRtvResult.CommandDisabled;

        if (RtvCommandStatus == RtvStatus.InCooldown)
            return PlayerRtvResult.CommandInCooldown;
        
        if (!_rtvVoteParticipants.Add(player.Slot))
            return PlayerRtvResult.AlreadyInRtv;


        // CountsRequiredToInitiateRtv is possibly 0, so if 0 visual required is set to 1, otherwise actual count.
        int visualRequiredCount = CountsRequiredToInitiateRtv > 0 ? CountsRequiredToInitiateRtv : 1;
        
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.PlayerCastRtv", player.PlayerName, _rtvVoteParticipants.Count, visualRequiredCount);
        

        if (_rtvVoteParticipants.Count >= CountsRequiredToInitiateRtv)
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

    internal void InitiateRtvVote()
    {
        _mcsMapVoteController.InitiateVote(true);
    }

    internal void InitiateForceRtvVote(CCSPlayerController? client)
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
        
        EnableRtvCommand(client, true);
        InitiateRtvVote();
    }

    internal void EnableRtvCommand(CCSPlayerController? client, bool silently = false)
    {
        RtvCommandStatus = RtvStatus.Enabled;
        RtvCommandUnlockTimer?.Kill();

        if (silently)
            return;
        
        string executorName = PlayerUtil.GetPlayerName(client);
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.Admin.EnabledRtv", executorName);
    }

    internal void DisableRtvCommand(CCSPlayerController? client = null)
    {
        RtvCommandStatus = RtvStatus.Disabled;
        RtvCommandUnlockTimer?.Kill();

        string executorName = PlayerUtil.GetPlayerName(client);
        PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.Admin.DisableRtv", executorName);
    }

    internal void ChangeToNextMap()
    {
        _mcsMapCycleController.ChangeMapOnNextRoundEnd = MapChangeTimingShouldRoundEnd.Value;

        if (MapChangeTimingShouldRoundEnd.Value)
        {
            PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.ChangeToNextMapNextRound");
        }
        else
        {
            PrintLocalizedChatToAllWithModulePrefix("RTV.Broadcast.ChangeToNextMapImmediately", MapChangeTimingAfterRtvSuccess.Value);
            _mcsMapCycleController.ChangeToNextMap(MapChangeTimingAfterRtvSuccess.Value);
        }
    }

    internal enum PlayerRtvResult
    {
        Success = 0,
        AlreadyInRtv,
        CommandInCooldown,
        CommandDisabled,
        AnotherVoteOngoing,
        NotAllowed
    }

    internal enum RtvStatus
    {
        Enabled = 0,
        Disabled,
        InCooldown,
        AnotherVoteOngoing,
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
    }

    private void OnMapVoteInitialization(McsMapVoteInitiatedEvent @event)
    {
        RtvCommandStatus = RtvStatus.AnotherVoteOngoing;
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
    
    
    private enum RtvCommandUnlockTimeOverride
    {
        NextMapConfirm = 0,
        MapExtended,
        MapNotChanged
    }
}

