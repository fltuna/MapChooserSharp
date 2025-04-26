using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Countdown.CenterHud;

public class McsCenterHudCountdownUi: IMcsCountdownUi
{
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft)
    {
        player.PrintToCenter($"TODO_TRANSLATE| Countdown: {secondsLeft}");
    }

    public void Close(CCSPlayerController player)
    {
        player.PrintToCenter("");
    }
}