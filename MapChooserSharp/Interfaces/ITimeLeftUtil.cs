using CounterStrikeSharp.API.Core;
using MapChooserSharp.API.MapVoteController;

namespace MapChooserSharp.Interfaces;

public interface ITimeLeftUtil
{
    public int TimeLimit { get; }
    
    public int RoundsLeft { get; }
    
    public int RoundTimeLeft { get; }

    public McsMapExtendType ExtendType { get; }

    public void ReDetermineExtendType();
    
    public bool ExtendTimeLimit(int minutes);
    public bool ExtendRounds(int rounds);
    public bool ExtendRoundTime(int minutes);
    
    public string GetFormattedTimeLeft(int timeLeft);
    public string GetFormattedTimeLeft(int timeLeft, CCSPlayerController? player);

    public string GetFormattedRoundsLeft(int timeLeft);
    public string GetFormattedRoundsLeft(int timeLeft, CCSPlayerController? player);
}