using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Countdown.Chat;

public class McsChatCountdownUi: IMcsCountdownUi
{
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft)
    {
        player.PrintToChat($"TODO_TRANSLATE| Countdown: {secondsLeft}");
    }

    public void Close(CCSPlayerController player){}
}