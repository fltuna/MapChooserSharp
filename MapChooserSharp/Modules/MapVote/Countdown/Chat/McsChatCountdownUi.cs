using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;

namespace MapChooserSharp.Modules.MapVote.Countdown.Chat;

public class McsChatCountdownUi(IServiceProvider provider): IMcsCountdownUi
{
    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();

    private bool _isFirstNotificationNotified = false;
    
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft, McsCountdownType countdownType)
    {
        if (!_isFirstNotificationNotified || secondsLeft <= 10)
        {
            player.PrintToChat(_plugin.LocalizeStringForPlayer(player, "MapVote.Broadcast.Countdown", secondsLeft));
            _isFirstNotificationNotified = true;
        }
    }

    public void Close(CCSPlayerController player){}
}