using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Menu;
using CS2ScreenMenuAPI;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu.Interfaces;
using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;
using TNCSSPluginFoundation.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.Cs2ScreenMenuApi;

public class McsCs2ScreenMenuApiUi(CCSPlayerController playerController, IServiceProvider provider): IMcsMapVoteUserInterface
{
    private IMcsGeneralMenuOption? _mcsGeneralMenuOption;
    
    private List<IMcsVoteOption> _voteOptions = new();

    private bool IsMenuShuffleEnabled { get; set; }

    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    private readonly IDebugLogger _debugLogger = provider.GetRequiredService<IDebugLogger>();
    
    private readonly IMcsInternalMapVoteControllerApi _voteController = provider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
    
    private readonly Dictionary<int, List<IMenuOption>> _chachedMenuOptions = new();

    private Menu? _currentMenu;

    public McsSupportedMenuType McsMenuType { get; } = McsSupportedMenuType.Cs2ScreenMenuApi;
    
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
            menuTitle.Append(_plugin.LocalizeStringForPlayer(playerController, _mcsGeneralMenuOption.MenuTitle));
        }
        else
        {
            menuTitle.Append(_plugin.LocalizeStringForPlayer(playerController,"General.Menu.Title"));
        }
        
        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Creating vote menu");
        _currentMenu = new Menu(playerController, _plugin);
        _currentMenu.Title = menuTitle.ToString();
        _currentMenu.ShowPageCount = false;
        _currentMenu.MenuType = MenuType.Both;
        _currentMenu.ShowResolutionOption = false;

        // If menu option is already exists (this is intended for !revote feature)
        if (_chachedMenuOptions.TryGetValue(playerController.Slot, out var menuOps))
        {
            _debugLogger.LogTrace($"[Player {playerController.PlayerName}] vote menu is already cached, reusing...");
            _currentMenu.Options.Clear();
            _currentMenu.Options.AddRange(menuOps);
            return;
        }

        

        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] has no cached menu, creating menu...");
        List<IMenuOption> menuOptions = new ();

        foreach (var (option, index) in _voteOptions.Select((value, i) => (value, i)))
        {
            string optionText = option.OptionText
                // If string contains Extend placeholder, then replace it.
                .Replace(_voteController.PlaceHolderExtendMap,
                    _plugin.LocalizeStringForPlayer(playerController, "Word.ExtendMap"))
                // If string contains Don't change placeholder, then replace it.
                .Replace(_voteController.PlaceHolderDontChangeMap,
                    _plugin.LocalizeStringForPlayer(playerController, "Word.DontChangeMap"));
            
            
            menuOptions.Add(new MenuOption
            {
                Text = optionText,
                Callback = (_, _) =>
                {
                    _voteOptions[(byte)index].VoteCallback.Invoke(playerController, (byte)index);
                }
            });
        }

        if (IsMenuShuffleEnabled)
        {
            _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Shuffling enabled... menu");
            Random random = Random.Shared;
            menuOptions = menuOptions.OrderBy(x => random.Next()).ToList();
        }
        
        _currentMenu.Options.Clear();
        _currentMenu.Options.AddRange(menuOptions);
        _chachedMenuOptions.TryAdd(playerController.Slot, menuOptions);
        _currentMenu.ShowResolutionOption = false;
        _currentMenu._config.Settings.ShowPageCount = false;
        _currentMenu.Display();
        
        
        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Menu init completed, opening...");
    }

    public void CloseMenu()
    {
        _currentMenu?.Close(playerController);
    }

    public void SetVoteOptions(List<IMcsVoteOption> voteOptions)
    {
        _voteOptions = voteOptions;
    }

    public void SetMenuOption(IMcsGeneralMenuOption option)
    {
        _mcsGeneralMenuOption = option;
    }

    
    // We don't implement this feature in this type, due to lack of UX
    public void RefreshTitleCountdown(int count) {}

    public void SetRandomShuffle(bool enableShuffle)
    {
        IsMenuShuffleEnabled = enableShuffle;
    }
}