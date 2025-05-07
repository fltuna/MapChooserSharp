using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.BuiltInHtml;

public class McsBuiltInHtmlVoteUiFactory(IServiceProvider provider) : IMcsMapVoteUiFactory
{
    public int MaxMenuElements { get; } = 5;

    public IMcsMapVoteUserInterface Create(CCSPlayerController player)
    {
        return new McsBuiltInHtmlVoteUi(player, provider);
    }
}