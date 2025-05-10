using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.CenterAlert;
using MapChooserSharp.Modules.MapVote.Countdown.CenterHtml;
using MapChooserSharp.Modules.MapVote.Countdown.CenterHud;
using MapChooserSharp.Modules.MapVote.Countdown.Chat;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapVote.Countdown;

internal sealed class McsCountdownUiController(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsCountdownUiController";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    
    // player slot - Countdown type
    private readonly Dictionary<int, McsCountdownUiType> _mcsCountdownTypes = new();
    
    private readonly Dictionary<McsCountdownUiType, IMcsCountdownUi> _countdownUis = new();
    
    private IMcsPluginConfigProvider _mcsPluginConfigProvider = null!;
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnInitialize()
    {
        Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        _countdownUis[McsCountdownUiType.CenterHud] = new McsCenterHudCountdownUi(ServiceProvider);
        _countdownUis[McsCountdownUiType.CenterAlert] = new McsCenterAlertCountdownUi(ServiceProvider);
        _countdownUis[McsCountdownUiType.CenterHtml] = new McsCenterHtmlCountdownUi(ServiceProvider);
        _countdownUis[McsCountdownUiType.Chat] = new McsChatCountdownUi(ServiceProvider);

        _mcsPluginConfigProvider = ServiceProvider.GetRequiredService<IMcsPluginConfigProvider>();
        
        if (hotReload)
        {
            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {
                if (player.IsBot || player.IsHLTV)
                    continue;

                PlayerConnectFull(player);
            }
        }
    }

    protected override void OnUnloadModule()
    {
        Plugin.DeregisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
    }
    
    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null)
            return HookResult.Continue;

        PlayerConnectFull(player);
        
        return HookResult.Continue;
    }

    private void PlayerConnectFull(CCSPlayerController player)
    {
        McsCountdownUiType uiType = _mcsPluginConfigProvider.PluginConfig.VoteConfig.CurrentCountdownUiType;
            
        // TODO() Player preference from DB
        
        UpdateCountdownType(player, uiType);
    }
    
    

    private void UpdateCountdownType(CCSPlayerController player, McsCountdownUiType uiType)
    {
        _mcsCountdownTypes[player.Slot] = uiType;
    }

    internal void CloseCountdownUiAll()
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (player.IsBot || player.IsHLTV)
                continue;

            CloseCountdownUi(player);
        }
    }

    internal void ShowCountdownToAll(int secondsLeft, McsCountdownType countdownType)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (player.IsBot || player.IsHLTV)
                continue;

            ShowCountdown(player, secondsLeft, countdownType);
        }
    }

    private void ShowCountdown(CCSPlayerController player, int secondsLeft, McsCountdownType countdownType)
    {
        var type = GetPlayerCountdownUiType(player);
        ShowCountdown(player, secondsLeft, type, countdownType);
    }

    private void ShowCountdown(CCSPlayerController player, int secondsLeft ,McsCountdownUiType uiType, McsCountdownType countdownType)
    {
        switch (countdownType)
        {
            case McsCountdownType.VoteStart:
                if (uiType.HasFlag(McsCountdownUiType.CenterHud))
                {
                    _countdownUis[McsCountdownUiType.CenterHud].ShowCountdownToPlayer(player, secondsLeft, countdownType);
                }
        
                if (uiType.HasFlag(McsCountdownUiType.CenterAlert))
                {
                    _countdownUis[McsCountdownUiType.CenterAlert].ShowCountdownToPlayer(player, secondsLeft, countdownType);
                }
        
                if (uiType.HasFlag(McsCountdownUiType.CenterHtml))
                {
                    _countdownUis[McsCountdownUiType.CenterHtml].ShowCountdownToPlayer(player, secondsLeft, countdownType);
                }
        
                if (uiType.HasFlag(McsCountdownUiType.Chat))
                {
                    _countdownUis[McsCountdownUiType.Chat].ShowCountdownToPlayer(player, secondsLeft, countdownType);
                }
                break;
            
            case McsCountdownType.Voting:
                _countdownUis[McsCountdownUiType.CenterAlert].ShowCountdownToPlayer(player, secondsLeft, countdownType);
                break;
        }
    }

    private void CloseCountdownUi(CCSPlayerController player)
    {
        var type = GetPlayerCountdownUiType(player);
        CloseCountdownUi(player, type);
    }

    private void CloseCountdownUi(CCSPlayerController player, McsCountdownUiType uiType)
    {
        if (uiType.HasFlag(McsCountdownUiType.CenterHud))
        {
            _countdownUis[McsCountdownUiType.CenterHud].Close(player);
        }
        
        if (uiType.HasFlag(McsCountdownUiType.CenterAlert))
        {
            _countdownUis[McsCountdownUiType.CenterAlert].Close(player);
        }
        
        if (uiType.HasFlag(McsCountdownUiType.CenterHtml))
        {
            _countdownUis[McsCountdownUiType.CenterHtml].Close(player);
        }
        
        if (uiType.HasFlag(McsCountdownUiType.Chat))
        {
            _countdownUis[McsCountdownUiType.Chat].Close(player);
        }
    }


    private McsCountdownUiType GetPlayerCountdownUiType(CCSPlayerController player)
    {
        if (!_mcsCountdownTypes.TryGetValue(player.Slot, out var type))
        {
            type = McsCountdownUiType.CenterHtml;
        }

        return type;
    }
}