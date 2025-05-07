using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Menu;
using CS2ScreenMenuAPI;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu.Interfaces;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;
using TNCSSPluginFoundation.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Cs2ScreenMenuApi;

public class McsCs2ScreenMenuApiNominationUi(CCSPlayerController playerController, IServiceProvider provider): IMcsNominationUserInterface
{
    private IMcsGeneralMenuOption? _generalMenuOption;
    
    private List<IMcsNominationMenuOption> _nominationMenuOptions = new();

    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    private readonly IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = provider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
    
    private readonly IDebugLogger _debugLogger = provider.GetRequiredService<IDebugLogger>();
    
    private readonly IMcsInternalMapVoteControllerApi _voteController = provider.GetRequiredService<IMcsInternalMapVoteControllerApi>();

    private Menu? _currentMenu;
    
    public int NominationOptionCount => _nominationMenuOptions.Count;
    
    
    public void OpenMenu()
    {
        if (_voteController.CurrentVoteState != McsMapVoteState.NoActiveVote)
            return;
        
        // Unused variable, but it required to decide what language should use in menu.
        using var tempLang = new WithTemporaryCulture(playerController.GetLanguage());
        
        
        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Creating nomination menu");
        
        StringBuilder menuTitle = new();

        if (_generalMenuOption != null && _generalMenuOption.MenuTitle != string.Empty)
        {
            menuTitle.Append(_plugin.LocalizeStringForPlayer(playerController, _generalMenuOption.MenuTitle + ".Html"));
        }
        else
        {
            menuTitle.Append(_plugin.LocalizeStringForPlayer(playerController, "General.Menu.Title.Html"));
        }
        
        _currentMenu = new Menu(playerController, _plugin);
        _currentMenu.Title = menuTitle.ToString();
        _currentMenu.ShowPageCount = false;
        _currentMenu.MenuType = MenuType.Both;
        _currentMenu.ShowResolutionOption = false;
        
        List<IMenuOption> menuOptions = new ();
        
        foreach (var (option, index) in _nominationMenuOptions.Select((value, i) => (value, i)))
        {
            StringBuilder builder = new();
            
            // TODO() Use Alias name if enabled and available
            // TODO() Truncate MapName if too long
            builder.Append(_mcsInternalMapConfigProviderApi.GetMapName(option.NominationOption.MapConfig));
            
            menuOptions.Add(new MenuOption
            {
                Text = builder.ToString(),
                Callback = (_, _) =>
                {
                    _nominationMenuOptions[index].SelectionCallback.Invoke(playerController, option.NominationOption);
                },
                IsDisabled = option.MenuDisabled
            });
        }
        
        _currentMenu.Options.Clear();
        _currentMenu.Options.AddRange(menuOptions);
        
        
        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Menu init completed, opening...");
    }

    public void CloseMenu()
    {
        _currentMenu?.Close(playerController);
    }

    public void SetNominationOption(List<IMcsNominationMenuOption> mcsNominationMenuOptions)
    {
        _nominationMenuOptions = mcsNominationMenuOptions;
    }

    public void SetMenuOption(IMcsGeneralMenuOption generalMenuOption)
    {
        _generalMenuOption = generalMenuOption;
    }
}