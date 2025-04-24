using MapChooserSharp.Modules.MapVote.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Menus.Interfaces;

public interface IMcsMapVoteUiFactory
{
    int MaxMenuElements { get; }
    
    IMcsMapVoteUserInterface Create();
}