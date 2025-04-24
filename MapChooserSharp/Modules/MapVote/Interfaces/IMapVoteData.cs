using MapChooserSharp.API.MapConfig;

namespace MapChooserSharp.Modules.MapVote.Interfaces;

public interface IMapVoteData
{
    string MapName { get; }
    
    public IMapConfig? MapConfig { get; }

    public IReadOnlyCollection<int> GetVoters();

    public void AddVoter(int voterSlot);
    
    public void RemoveVoter(int voterSlot);
    
    public bool IsPlayerVoted(int voterSlot);
}