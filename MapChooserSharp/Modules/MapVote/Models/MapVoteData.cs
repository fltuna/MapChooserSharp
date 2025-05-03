using MapChooserSharp.API.MapConfig;
using MapChooserSharp.Modules.MapVote.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Models;

public class MapVoteData(IMapConfig? mapConfig, string mapName) : IMapVoteData
{
    public string MapName { get; } = mapName;

    private HashSet<int> SlotsWhoVoted { get; } = new();
    public IMapConfig? MapConfig { get; } = mapConfig;
    
    
    public IReadOnlyCollection<int> GetVoters()
    {
        return SlotsWhoVoted;
    }

    public void AddVoter(int voterSlot)
    {
        SlotsWhoVoted.Add(voterSlot);
    }

    public void RemoveVoter(int voterSlot)
    {
        SlotsWhoVoted.Remove(voterSlot);
    }

    public bool IsPlayerVoted(int voterSlot)
    {
        return SlotsWhoVoted.Contains(voterSlot);
    }
}