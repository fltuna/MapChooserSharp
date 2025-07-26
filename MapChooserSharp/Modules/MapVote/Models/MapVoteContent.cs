using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Models;

public class MapVoteContent(HashSet<int> voteParticipants, List<IMapVoteData> votingMaps, Dictionary<int, IMcsMapVoteUserInterface> voteUi, bool isRtvVote) : IMapVoteContent
{
    private HashSet<int> VoteParticipants { get; } = voteParticipants;
    private List<IMapVoteData> VotingMaps { get; } = votingMaps;
    public Dictionary<int, IMcsMapVoteUserInterface> VoteUi { get; private set; } = voteUi;
    public bool IsRtvVote { get; } = isRtvVote;
    
    
    public bool IsPlayerInVoteParticipant(int slot)
    {
        return VoteParticipants.Contains(slot);
    }

    public HashSet<int> GetVoteParticipants()
    {
        return VoteParticipants;
    }

    public List<IMapVoteData> GetVotingMaps()
    {
        return VotingMaps;
    }
}