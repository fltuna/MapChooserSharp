using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.Interfaces;

public interface ITimeLeftUtil
{
    public int TimeLeft { get; }

    public string GetFormattedTimeLeft(int timeLeft);
    public string GetFormattedTimeLeft(int timeLeft, CCSPlayerController? player);
}