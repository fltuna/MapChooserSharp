using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Modules.Menu;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.MapVote.Menus.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Models;

public class MapVoteContent(HashSet<int> voteParticipants, List<IMapVoteData> votingMaps, IMcsMapVoteUserInterface voteUi, bool isRtvVote) : IMapVoteContent
{
    private HashSet<int> VoteParticipants { get; } = voteParticipants;
    private List<IMapVoteData> VotingMaps { get; } = votingMaps;
    public IMcsMapVoteUserInterface VoteUi { get; private set; } = voteUi;
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