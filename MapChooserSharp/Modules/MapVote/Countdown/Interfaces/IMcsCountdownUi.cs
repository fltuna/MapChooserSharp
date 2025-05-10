using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.Modules.MapVote.Countdown.Interfaces;

public interface IMcsCountdownUi
{
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft, McsCountdownType countdownType);

    public void Close(CCSPlayerController player);
}