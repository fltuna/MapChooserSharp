using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;

namespace MapChooserSharp.API.Events.MapVote;

/// <summary>
/// This event will be called when map vote is finished
/// </summary>
/// <param name="modulePrefix">Module Prefix</param>
public class McsMapVoteFinishedEvent(string modulePrefix): McsEventParam(modulePrefix), IMcsEventNoResult;