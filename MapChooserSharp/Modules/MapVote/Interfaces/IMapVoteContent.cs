using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Interfaces;

public interface IMapVoteContent
{
    public Dictionary<int, IMcsMapVoteUserInterface> VoteUi { get; }
    public bool IsRtvVote { get; }
    

    public bool IsPlayerInVoteParticipant(int slot);
    
    public HashSet<int> GetVoteParticipants();

    public List<IMapVoteData> GetVotingMaps();
}