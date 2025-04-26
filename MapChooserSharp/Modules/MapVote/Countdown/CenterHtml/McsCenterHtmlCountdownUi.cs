using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Countdown.CenterHtml;

public class McsCenterHtmlCountdownUi: IMcsCountdownUi
{
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft)
    {
        player.PrintToCenterHtml($"TODO_TRANSLATE| Countdown: {secondsLeft}");
    }

    public void Close(CCSPlayerController player)
    {
        player.PrintToCenterHtml("");
    }
}