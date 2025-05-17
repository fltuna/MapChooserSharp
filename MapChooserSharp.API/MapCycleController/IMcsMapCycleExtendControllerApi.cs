using CounterStrikeSharp.API.Core;

namespace MapChooserSharp.API.MapCycleController;

/// <summary>
/// Map extending management
/// </summary>
public interface IMcsMapCycleExtendControllerApi
{
    /// <summary>
    /// Describes current status of !ext
    /// </summary>
    public ExtStatus ExtCommandStatus { get; }
    
    /// <summary>
    /// How many user exts remaining in this map
    /// </summary>
    public int ExtUsageRemaining { get; }
    
    /// <summary>
    /// Set remainig ext count
    /// </summary>
    /// <param name="userExtsRemaining">exts count to set</param>
    public void SetExtUsageRemaining(int userExtsRemaining);
    
    /// <summary>
    /// Extends a current map by specified time.
    /// </summary>
    /// <param name="extendTime">extends a round, timelimit or roundtime. It depends on a map type</param>
    /// <returns>McsMapCycleExtendResult.Extended if map is extended successfully</returns>
    public McsMapCycleExtendResult ExtendCurrentMap(int extendTime);


    /// <summary>
    /// Cast ext vote
    /// </summary>
    /// <param name="player">Player who voted</param>
    /// <returns>PlayerExtResult.Success if voted successfully</returns>
    public PlayerExtResult CastPlayerExtVote(CCSPlayerController player);

    /// <summary>
    /// Enables player !ext command
    /// </summary>
    /// <param name="player">Player who enabled. it will treated as CONSOLE when null</param>
    /// <param name="silently">if true, change is not notified to players.</param>
    public void EnablePlayerExtCommand(CCSPlayerController? player = null, bool silently = false);

    /// <summary>
    /// Disables player !ext command
    /// </summary>
    /// <param name="player">Player who disabled. it will treated as CONSOLE when null</param>
    /// <param name="silently">if true, change is not notified to players.</param>
    public void DisablePlayerExtCommand(CCSPlayerController? player = null, bool silently = false);
}