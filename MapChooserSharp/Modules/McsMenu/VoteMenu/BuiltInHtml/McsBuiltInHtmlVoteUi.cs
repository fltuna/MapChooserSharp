using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Menu;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu.Interfaces;
using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;
using TNCSSPluginFoundation.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.BuiltInHtml;

public class McsBuiltInHtmlVoteUi(CCSPlayerController playerController, IServiceProvider provider) : IMcsMapVoteUserInterface
{
    private IMcsGeneralMenuOption? _mcsGeneralMenuOption;
    
    private List<IMcsVoteOption> _voteOptions = new();

    private bool IsMenuShuffleEnabled { get; set; }

    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    private readonly IDebugLogger _debugLogger = provider.GetRequiredService<IDebugLogger>();
    
    private readonly IMcsInternalMapVoteControllerApi _voteController = provider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
    
    private readonly Dictionary<int, List<ChatMenuOption>> _chachedMenuOptions = new();

    public int VoteOptionCount => _voteOptions.Count;

    public void OpenMenu()
    {
        if (_voteController.CurrentVoteState != McsMapVoteState.Voting && _voteController.CurrentVoteState != McsMapVoteState.RunoffVoting)
            return;

        // Unused variable, but it required to decide what language should use in menu.
        using var tempLang = new WithTemporaryCulture(playerController.GetLanguage());
        
        StringBuilder menuTitle = new();

        if (_mcsGeneralMenuOption != null && _mcsGeneralMenuOption.MenuTitle != string.Empty)
        {
            // TODO() If map countdown setting is verbose, then use verbose title.
            menuTitle.Append(_plugin.LocalizeStringForPlayer(playerController, _mcsGeneralMenuOption.MenuTitle + ".Html"));
        }
        else
        {
            menuTitle.Append(_plugin.LocalizeStringForPlayer(playerController,"General.Menu.Title" + ".Html"));
        }
        
        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Creating vote menu");
        CenterHtmlMenu menu = new(menuTitle.ToString(), _plugin);

        // If menu option is already exists (this is intended for !revote feature)
        if (_chachedMenuOptions.TryGetValue(playerController.Slot, out var menuOps))
        {
            _debugLogger.LogTrace($"[Player {playerController.PlayerName}] vote menu menu is already cached, reusing...");
            menu.MenuOptions.Clear();
            menu.MenuOptions.AddRange(menuOps);
            MenuManager.OpenCenterHtmlMenu(_plugin, playerController, menu);
            return;
        }


        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] has no cached menu, creating menu...");
        List<ChatMenuOption> options = new();

        foreach (var (option, index) in _voteOptions.Select((value, i) => (value, i)))
        {
            string optionText = option.OptionText
                // If string contains Extend placeholder, then replace it.
                .Replace(_voteController.PlaceHolderExtendMap,
                    _plugin.LocalizeStringForPlayer(playerController, "Word.ExtendMap"))
                // If string contains Don't change placeholder, then replace it.
                .Replace(_voteController.PlaceHolderDontChangeMap,
                    _plugin.LocalizeStringForPlayer(playerController, "Word.DontChangeMap"));
            
            options.Add(new ChatMenuOption(optionText, false, (controller, menuOption) =>
            {
                _voteOptions[(byte)index].VoteCallback.Invoke(playerController, (byte)index);
            }));
        }

        if (IsMenuShuffleEnabled)
        {
            _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Shuffling enabled... menu");
            Random random = Random.Shared;
            options = options.OrderBy(x => random.Next()).ToList();
        }
        
        menu.MenuOptions.Clear();
        menu.MenuOptions.AddRange(options);
        _chachedMenuOptions.TryAdd(playerController.Slot, options);
        
        
        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Menu init completed, opening...");
        MenuManager.OpenCenterHtmlMenu(_plugin, playerController, menu);
    }

    public void CloseMenu()
    {
        MenuManager.CloseActiveMenu(playerController);
    }

    public void SetVoteOptions(List<IMcsVoteOption> voteOptions)
    {
        _voteOptions = voteOptions;
    }

    public void SetMenuOption(IMcsGeneralMenuOption option)
    {
        _mcsGeneralMenuOption = option;
    }

    public void SetRandomShuffle(bool enableShuffle)
    {
        IsMenuShuffleEnabled = enableShuffle;
    }
}