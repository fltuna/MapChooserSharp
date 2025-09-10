using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;

namespace MapChooserSharp.Modules.MapVote.Countdown.CenterHud;

public class McsCenterHudCountdownUi(IServiceProvider provider): IMcsCountdownUi
{
    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft, McsCountdownType countdownType)
    {
        player.PrintToCenter(_plugin.LocalizeStringForPlayer(player, "MapVote.Broadcast.Countdown", secondsLeft));
    }

    public void Close(CCSPlayerController player)
    {
        player.PrintToCenter(" ");
    }
}