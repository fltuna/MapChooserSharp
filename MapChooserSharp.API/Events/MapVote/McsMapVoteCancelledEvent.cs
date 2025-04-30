namespace MapChooserSharp.API.Events.MapVote;

/// <summary>
/// This event will be called when vote is cancelled.
/// </summary>
public class McsMapVoteCancelledEvent(string modulePrefix): McsEventParam(modulePrefix), IMcsEventNoResult;