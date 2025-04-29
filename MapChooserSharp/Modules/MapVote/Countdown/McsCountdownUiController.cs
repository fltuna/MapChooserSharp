using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.CenterAlert;
using MapChooserSharp.Modules.MapVote.Countdown.CenterHtml;
using MapChooserSharp.Modules.MapVote.Countdown.CenterHud;
using MapChooserSharp.Modules.MapVote.Countdown.Chat;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation.Models.Plugin;

namespace MapChooserSharp.Modules.MapVote.Countdown;

internal sealed class McsCountdownUiController(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "McsCountdownUiController";
    public override string ModuleChatPrefix => "unused";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    
    // player slot - Countdown type
    private readonly Dictionary<int, McsCountdownType> _mcsCountdownTypes = new();
    
    private readonly Dictionary<McsCountdownType, IMcsCountdownUi> _countdownUis = new();
    
    
    
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(this);
    }

    protected override void OnInitialize()
    {
        Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        _countdownUis[McsCountdownType.CenterHud] = new McsCenterHudCountdownUi();
        _countdownUis[McsCountdownType.CenterAlert] = new McsCenterAlertCountdownUi();
        _countdownUis[McsCountdownType.CenterHtml] = new McsCenterHtmlCountdownUi();
        _countdownUis[McsCountdownType.Chat] = new McsChatCountdownUi();

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
        // TODO() Player preference from DB

        McsCountdownType type = McsCountdownType.CenterHtml;
        
        UpdateCountdownType(player, type);
    }
    
    

    private void UpdateCountdownType(CCSPlayerController player, McsCountdownType type)
    {
        _mcsCountdownTypes[player.Slot] = type;
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

    internal void ShowCountdownToAll(int secondsLeft)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (player.IsBot || player.IsHLTV)
                continue;

            ShowCountdown(player, secondsLeft);
        }
    }

    private void ShowCountdown(CCSPlayerController player, int secondsLeft)
    {
        var type = GetPlayerCountdownType(player);
        ShowCountdown(player, secondsLeft, type);
    }

    private void ShowCountdown(CCSPlayerController player, int secondsLeft ,McsCountdownType type)
    {
        if (type.HasFlag(McsCountdownType.CenterHud))
        {
            _countdownUis[McsCountdownType.CenterHud].ShowCountdownToPlayer(player, secondsLeft);
        }
        
        if (type.HasFlag(McsCountdownType.CenterAlert))
        {
            _countdownUis[McsCountdownType.CenterAlert].ShowCountdownToPlayer(player, secondsLeft);
        }
        
        if (type.HasFlag(McsCountdownType.CenterHtml))
        {
            _countdownUis[McsCountdownType.CenterHtml].ShowCountdownToPlayer(player, secondsLeft);
        }
        
        if (type.HasFlag(McsCountdownType.Chat))
        {
            _countdownUis[McsCountdownType.Chat].ShowCountdownToPlayer(player, secondsLeft);
        }
    }

    private void CloseCountdownUi(CCSPlayerController player)
    {
        var type = GetPlayerCountdownType(player);
        CloseCountdownUi(player, type);
    }

    private void CloseCountdownUi(CCSPlayerController player, McsCountdownType type)
    {
        if (type.HasFlag(McsCountdownType.CenterHud))
        {
            _countdownUis[McsCountdownType.CenterHud].Close(player);
        }
        
        if (type.HasFlag(McsCountdownType.CenterAlert))
        {
            _countdownUis[McsCountdownType.CenterAlert].Close(player);
        }
        
        if (type.HasFlag(McsCountdownType.CenterHtml))
        {
            _countdownUis[McsCountdownType.CenterHtml].Close(player);
        }
        
        if (type.HasFlag(McsCountdownType.Chat))
        {
            _countdownUis[McsCountdownType.Chat].Close(player);
        }
    }


    private McsCountdownType GetPlayerCountdownType(CCSPlayerController player)
    {
        if (!_mcsCountdownTypes.TryGetValue(player.Slot, out var type))
        {
            type = McsCountdownType.CenterHtml;
        }

        return type;
    }
}