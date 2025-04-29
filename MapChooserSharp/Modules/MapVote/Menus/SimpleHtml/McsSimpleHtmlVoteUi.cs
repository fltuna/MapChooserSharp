using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Menu;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.MapVote.Menus.Interfaces;
using MapChooserSharp.Modules.MapVote.Models;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;
using TNCSSPluginFoundation.Interfaces;

namespace MapChooserSharp.Modules.MapVote.Menus.SimpleHtml;

public class McsSimpleHtmlVoteUi(IServiceProvider provider) : IMcsMapVoteUserInterface
{
    private List<IMcsVoteOption> _voteOptions = new();
    
    private bool IsMenuShuffleEnabled { get; set; }

    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    private readonly IDebugLogger _debugLogger = provider.GetRequiredService<IDebugLogger>();
    
    private readonly McsMapVoteController _voteController = provider.GetRequiredService<McsMapVoteController>();
    
    private readonly Dictionary<int, List<ChatMenuOption>> _chachedMenuOptions = new();

    public int VoteOptionCount => _voteOptions.Count;

    public void OpenMenu(CCSPlayerController player)
    {
        if (_voteController.CurrentVoteState != McsMapVoteState.Voting && _voteController.CurrentVoteState != McsMapVoteState.RunoffVoting)
            return;

        // Unused variable, but it required to decide what language should use in menu.
        using var tempLang = new WithTemporaryCulture(player.GetLanguage());
        
        
        _debugLogger.LogTrace($"[Player {player.PlayerName}] Creating vote menu");
        CenterHtmlMenu menu = new(_plugin.LocalizeStringForPlayer(player, "MapVote.Menu.MenuTitle"), _plugin);

        // If menu option is already exists (this is intended for !revote feature)
        if (_chachedMenuOptions.TryGetValue(player.Slot, out var menuOps))
        {
            _debugLogger.LogTrace($"[Player {player.PlayerName}] vote menu menu is already cached, reusing...");
            menu.MenuOptions.Clear();
            menu.MenuOptions.AddRange(menuOps);
            MenuManager.OpenCenterHtmlMenu(_plugin, player, menu);
            return;
        }


        _debugLogger.LogTrace($"[Player {player.PlayerName}] has no cached menu, creating menu...");
        List<ChatMenuOption> options = new();

        foreach (var (option, index) in _voteOptions.Select((value, i) => (value, i)))
        {
            options.Add(new ChatMenuOption(option.OptionText, false, (controller, menuOption) =>
            {
                _voteOptions[(byte)index].VoteCallback.Invoke(player, (byte)index);
            }));
        }

        if (IsMenuShuffleEnabled)
        {
            _debugLogger.LogTrace($"[Player {player.PlayerName}] Shuffling enabled... menu");
            Random random = new Random();
            options = options.OrderBy(x => random.Next()).ToList();
        }
        
        menu.MenuOptions.Clear();
        menu.MenuOptions.AddRange(options);
        _chachedMenuOptions.TryAdd(player.Slot, options);
        
        
        _debugLogger.LogTrace($"[Player {player.PlayerName}] Menu init completed, opening...");
        MenuManager.OpenCenterHtmlMenu(_plugin, player, menu);
    }

    public void CloseMenu(CCSPlayerController player)
    {
        MenuManager.CloseActiveMenu(player);
    }

    public void SetVoteOptions(List<IMcsVoteOption> voteOptions)
    {
        _voteOptions = voteOptions;
    }

    public void SetRandomShuffle(bool enableShuffle)
    {
        IsMenuShuffleEnabled = enableShuffle;
    }
}