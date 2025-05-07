using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.API.RtvController;

/// <summary>
/// RTV Controller API
/// </summary>
public interface IMcsRtvControllerApi
{
    /// <summary>
    /// Describes current status of RTV
    /// </summary>
    public RtvStatus RtvCommandStatus { get; }


    /// <summary>
    /// Time to be RTV command unlocked to players <br/>
    /// You can get remaining time in seconds by `RtvCommandUnlockTime - Server.CurrentTime`
    /// </summary>
    public float RtvCommandUnlockTime { get; }

    /// <summary>
    /// Add player to RTV participant
    /// </summary>
    /// <param name="player"></param>
    /// <returns>Player's RTV result</returns>
    public PlayerRtvResult AddPlayerToRtv(CCSPlayerController player);

    /// <summary>
    /// Initiate a rtv vote
    /// </summary>
    public void InitiateRtvVote();

    /// <summary>
    /// Enables RTV
    /// </summary>
    /// <param name="client">Player who enabled, if null then treated as Console</param>
    /// <param name="silently">If true, method will not print the broadcast message</param>
    public void EnableRtvCommand(CCSPlayerController? client, bool silently = false);


    /// <summary>
    /// Disables RTV
    /// </summary>
    /// <param name="client">Player who disabled, if null then treated as Console</param>
    /// <param name="silently">If true, method will not print the broadcast message</param>
    public void DisableRtvCommand(CCSPlayerController? client = null, bool silently = false);


    /// <summary>
    /// Initiate a force RTV
    /// </summary>
    /// <param name="client">Player who triggered, if null then treated as Console</param>
    public void InitiateForceRtvVote(CCSPlayerController? client);
}