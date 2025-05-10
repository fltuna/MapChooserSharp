using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu.Interfaces;
using MapChooserSharp.Modules.McsMenu.VoteMenu.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;
using TNCSSPluginFoundation.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.VoteMenu.Cs2MenuManager.ScreenMenu;

public class McsCs2MenuManagerScreenMenuUi(CCSPlayerController playerController, IServiceProvider provider): IMcsMapVoteUserInterface
{
    private IMcsGeneralMenuOption? _mcsGeneralMenuOption;
    
    private List<IMcsVoteOption> _voteOptions = new();

    private bool IsMenuShuffleEnabled { get; set; }

    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    private readonly IDebugLogger _debugLogger = provider.GetRequiredService<IDebugLogger>();
    
    private readonly IMcsInternalMapVoteControllerApi _voteController = provider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
    
    private readonly Dictionary<int, List<ItemOption>> _chachedMenuOptions = new();

    public McsSupportedMenuType McsMenuType { get; } = McsSupportedMenuType.Cs2MenuManagerScreen;

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
        CS2MenuManager.API.Menu.ScreenMenu menu = new CS2MenuManager.API.Menu.ScreenMenu(menuTitle.ToString(), _plugin)
        {
            ShowResolutionsOption = false,
            MenuType = MenuType.Both,
        };

        // If menu option is already exists (this is intended for !revote feature)
        if (_chachedMenuOptions.TryGetValue(playerController.Slot, out var menuOps))
        {
            _debugLogger.LogTrace($"[Player {playerController.PlayerName}] vote menu menu is already cached, reusing...");
            menu.ItemOptions.Clear();
            menu.ItemOptions.AddRange(menuOps);
            DisplayMenu(playerController, menu);
            return;
        }


        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] has no cached menu, creating menu...");
        List<ItemOption> menuOptions = new();

        foreach (var (option, index) in _voteOptions.Select((value, i) => (value, i)))
        {
            string optionText = option.OptionText
                // If string contains Extend placeholder, then replace it.
                .Replace(_voteController.PlaceHolderExtendMap,
                    _plugin.LocalizeStringForPlayer(playerController, "Word.ExtendMap"))
                // If string contains Don't change placeholder, then replace it.
                .Replace(_voteController.PlaceHolderDontChangeMap,
                    _plugin.LocalizeStringForPlayer(playerController, "Word.DontChangeMap"));
            
            menuOptions.Add(new ItemOption(optionText, DisableOption.None, (_, _) =>
            {
                _voteOptions[(byte)index].VoteCallback.Invoke(playerController, (byte)index);
            }));
        }

        if (IsMenuShuffleEnabled)
        {
            _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Shuffling enabled...");
            Random random = Random.Shared;
            menuOptions = menuOptions.OrderBy(x => random.Next()).ToList();
        }
        
        menu.ItemOptions.Clear();
        menu.ItemOptions.AddRange(menuOptions);
        _chachedMenuOptions.TryAdd(playerController.Slot, menuOptions);
        
        
        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Menu init completed, opening...");
        
        
        // Open blank menu before actual menu opened
        // Because sometimes screen menu is not appear
        CS2MenuManager.API.Menu.ScreenMenu blankMenu =
            new CS2MenuManager.API.Menu.ScreenMenu(menuTitle.ToString(), _plugin);
        blankMenu.Display(playerController, 1);
        
        Server.NextFrame(() =>
        {
            DisplayMenu(playerController, menu);
        });
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

    // We don't implement this feature in this type, due to lack of UX
    public void RefreshTitleCountdown(int count) {}

    public void SetRandomShuffle(bool enableShuffle)
    {
        IsMenuShuffleEnabled = enableShuffle;
    }

    private void DisplayMenu(CCSPlayerController player, CS2MenuManager.API.Menu.ScreenMenu menu)
    {
        menu.Display(playerController, _voteController.VoteEndTime);
    }
}