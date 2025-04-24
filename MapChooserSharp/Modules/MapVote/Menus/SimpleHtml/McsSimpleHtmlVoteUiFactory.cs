using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.MapVote.Menus.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Menus.SimpleHtml;

public class McsSimpleHtmlVoteUiFactory(IServiceProvider provider) : IMcsMapVoteUiFactory
{
    public int MaxMenuElements { get; } = 5;

    public IMcsMapVoteUserInterface Create()
    {
        return new McsSimpleHtmlVoteUi(provider);
    }
}