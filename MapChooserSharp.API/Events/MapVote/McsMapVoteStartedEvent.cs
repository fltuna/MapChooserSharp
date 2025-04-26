namespace MapChooserSharp.API.Events.MapVote;

/// <summary>
/// This event will be called when vote starting.
/// </summary>
public class McsMapVoteStartedEvent(string modulePrefix): McsEventParam(modulePrefix), IMcsEventNoResult;