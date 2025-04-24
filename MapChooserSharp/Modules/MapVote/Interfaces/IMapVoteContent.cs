using System.Collections.ObjectModel;
using CounterStrikeSharp.API.Modules.Menu;
using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapVote.Menus.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Interfaces;

public interface IMapVoteContent
{
    public IMcsMapVoteUserInterface VoteUi { get; }
    public bool IsRtvVote { get; }
    

    public bool IsPlayerInVoteParticipant(int slot);
    
    public HashSet<int> GetVoteParticipants();

    public List<IMapVoteData> GetVotingMaps();
}