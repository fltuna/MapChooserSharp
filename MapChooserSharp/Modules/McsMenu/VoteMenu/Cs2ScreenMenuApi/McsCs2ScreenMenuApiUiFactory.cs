using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.Cs2ScreenMenuApi;

public class McsCs2ScreenMenuApiUiFactory(IServiceProvider provider) : IMcsMapVoteUiFactory
{
    public int MaxMenuElements { get; } = 7;
    
    public IMcsMapVoteUserInterface Create(CCSPlayerController player)
    {
        return new McsCs2ScreenMenuApiUi(player, provider);
    }
}