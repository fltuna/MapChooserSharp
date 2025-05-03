using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;

namespace MapChooserSharp.Modules.MapVote.Countdown.Chat;

public class McsChatCountdownUi(IServiceProvider provider): IMcsCountdownUi
{
    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft)
    {
        player.PrintToChat(_plugin.LocalizeStringForPlayer(player, "MapVote.Broadcast.Countdown", secondsLeft));
    }

    public void Close(CCSPlayerController player){}
}