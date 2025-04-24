using MapChooserSharp.API.MapConfig;
using MapChooserSharp.API.MapVoteController;

namespace MapChooserSharp.API.Events.MapVote;

/// <summary>
/// This event will be called when after RTV vote with "don't change map"
/// </summary>
/// <param name="modulePrefix">Module Prefix</param>
public class McsMapNotChangedEvent(string modulePrefix): McsEventParam(modulePrefix), IMcsEventNoResult
{
}