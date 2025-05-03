namespace MapChooserSharp.API.Events.MapVote;

/// <summary>
/// This event will be called when starting vote initiation. (starts a countdown)
/// </summary>
public class McsMapVoteInitiatedEvent(string modulePrefix): McsEventParam(modulePrefix), IMcsEventNoResult;