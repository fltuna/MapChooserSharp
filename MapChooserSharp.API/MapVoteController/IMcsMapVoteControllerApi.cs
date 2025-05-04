using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.API.MapVoteController;

/// <summary>
/// MapVoteController API
/// </summary>
public interface IMcsMapVoteControllerApi
{
    /// <summary>
    /// Current state of vote <see cref="McsMapVoteState"/>
    /// </summary>
    public McsMapVoteState CurrentVoteState { get; }


    /// <summary>
    /// Initiate a map vote.
    /// </summary>
    /// <param name="isActivatedByRtv">If true, first option is "don't change", if false then "Extend Current Map"</param>
    /// <returns>McsMapVoteState.InitializeAccepted if successfully initiated, otherwise it's state of current vote</returns>
    public McsMapVoteState InitiateVote(bool isActivatedByRtv = false);


    /// <summary>
    /// Cancel the current vote.
    /// </summary>
    /// <returns>McsMapVoteState.Cancelling if successfully canceled, otherwise it's state of current vote</returns>
    public McsMapVoteState CancelVote(CCSPlayerController? player);


    /// <summary>
    /// Remove player's vote from the current vote.
    /// </summary>
    /// <param name="player">Player Controller</param>
    public void RemovePlayerVote(CCSPlayerController player);

    /// <summary>
    /// Remove player's vote from the current vote.
    /// </summary>
    /// <param name="slot">Player slot</param>
    public void RemovePlayerVote(int slot);


    /// <summary>
    /// Removes player vote and show vote menu to player
    /// </summary>
    /// <param name="player">Player Controller</param>
    public void PlayerReVote(CCSPlayerController player);
}