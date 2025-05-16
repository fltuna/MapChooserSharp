using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.API.MapCycleController;

/// <summary>
/// Map extending with CS2's native vote UI
/// </summary>
public interface IMcsMapCycleExtendVoteControllerApi
{
    /// <summary>
    /// Start a extend vote with CS2's native vote UI<br/>
    /// if another native vote is ongoing, it will silently ignored.
    /// </summary>
    /// <param name="player">Player who executed, if null treated as CONSOLE</param>
    /// <param name="extendTime">Specify the extend time. specify minutes if timelimit or roundtime based. specify rounds if round based.</param>
    public void StartExtendVote(CCSPlayerController? player, int extendTime);
}