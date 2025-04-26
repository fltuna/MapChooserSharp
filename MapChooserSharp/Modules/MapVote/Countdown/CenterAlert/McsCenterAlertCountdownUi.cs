using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Countdown.CenterAlert;

public class McsCenterAlertCountdownUi: IMcsCountdownUi
{
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft)
    {
        player.PrintToCenterAlert($"TODO_TRANSLATE| Countdown: {secondsLeft}");
    }

    public void Close(CCSPlayerController player)
    {
        player.PrintToCenterAlert(" ");
    }
}