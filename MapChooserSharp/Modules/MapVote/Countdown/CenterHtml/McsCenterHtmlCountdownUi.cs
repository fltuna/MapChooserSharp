using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;

namespace MapChooserSharp.Modules.MapVote.Countdown.CenterHtml;

public class McsCenterHtmlCountdownUi(IServiceProvider provider): IMcsCountdownUi
{
    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft, McsCountdownType countdownType)
    {
        player.PrintToCenterHtml(_plugin.LocalizeStringForPlayer(player, "MapVote.Broadcast.CenterHtml", secondsLeft));
    }

    public void Close(CCSPlayerController player)
    {
        player.PrintToCenterHtml("");
    }
}