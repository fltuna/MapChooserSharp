using MapChooserSharp.API.MapVoteController;

namespace MapChooserSharp.Modules.MapVote.Interfaces;

internal interface IMcsInternalMapVoteControllerApi: IMcsMapVoteControllerApi
{
    public string PlaceHolderExtendMap { get; }
    public string PlaceHolderDontChangeMap { get; }
    
    public int VoteEndTime { get; }
}