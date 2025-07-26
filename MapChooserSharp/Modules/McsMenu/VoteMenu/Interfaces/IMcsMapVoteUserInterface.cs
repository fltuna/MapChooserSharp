using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;

public interface IMcsMapVoteUserInterface
{
    public McsSupportedMenuType McsMenuType { get; }
    
    public int VoteOptionCount { get; }
    
    public void OpenMenu();

    public void CloseMenu();

    public void SetVoteOptions(List<IMcsVoteOption> voteOptions);
    
    public void SetMenuOption(IMcsGeneralMenuOption option);

    public void RefreshTitleCountdown(int count);

    public void SetRandomShuffle(bool enableShuffle);
}