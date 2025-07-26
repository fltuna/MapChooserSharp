using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using CS2ScreenMenuAPI;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapConfig.Interfaces;
using MapChooserSharp.Modules.MapVote.Interfaces;
using MapChooserSharp.Modules.McsMenu.Interfaces;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;
using MapChooserSharp.Modules.Nomination;
using MapChooserSharp.Modules.Nomination.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;
using TNCSSPluginFoundation.Interfaces;
using MenuType = CS2MenuManager.API.Enum.MenuType;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.Cs2MenuManager.ScreenMenu;

public class McsCs2MenuManagerScreenMenuNominationUi(CCSPlayerController playerController, IServiceProvider provider): IMcsNominationUserInterface
{
    private IMcsGeneralMenuOption? _generalMenuOption;
    
    private List<IMcsNominationMenuOption> _nominationMenuOptions = new();

    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    private readonly IMcsInternalMapConfigProviderApi _mcsInternalMapConfigProviderApi = provider.GetRequiredService<IMcsInternalMapConfigProviderApi>();
    
    private readonly IDebugLogger _debugLogger = provider.GetRequiredService<IDebugLogger>();
    
    private readonly IMcsInternalMapVoteControllerApi _voteController = provider.GetRequiredService<IMcsInternalMapVoteControllerApi>();
    
    private readonly IMcsInternalNominationApi _nominationApi = provider.GetRequiredService<IMcsInternalNominationApi>();
    
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
        
        CS2MenuManager.API.Menu.ScreenMenu menu = new CS2MenuManager.API.Menu.ScreenMenu(menuTitle.ToString(), _plugin)
        {
            ScreenMenu_MenuType = MenuType.Both,
            ScreenMenu_ShowResolutionsOption = false
        };
        
        List<ItemOption> menuOptions = new ();
        
        foreach (var (option, index) in _nominationMenuOptions.Select((value, i) => (value, i)))
        {
            StringBuilder builder = new();
            
            builder.Append(_mcsInternalMapConfigProviderApi.GetMapName(option.NominationOption.MapConfig));

            bool cannotNominate = _nominationApi.PlayerCanNominateMap(playerController, option.NominationOption.MapConfig) != McsMapNominationController.NominationCheck.Success;
            
            menuOptions.Add(new ItemOption(builder.ToString(), cannotNominate ? DisableOption.DisableShowNumber : DisableOption.None, (_, _) =>
            {
                _nominationMenuOptions[index].SelectionCallback.Invoke(playerController, option.NominationOption);
            }));
        }
        
        menu.ItemOptions.Clear();
        menu.ItemOptions.AddRange(menuOptions);
        
        
        _debugLogger.LogTrace($"[Player {playerController.PlayerName}] Menu init completed, opening...");
        
        
        // Open blank menu before actual menu opened
        // Because sometimes screen menu is not appear
        CS2MenuManager.API.Menu.ScreenMenu blankMenu =
            new CS2MenuManager.API.Menu.ScreenMenu(menuTitle.ToString(), _plugin);
        blankMenu.Display(playerController, 0);
        
        Server.NextFrame(() =>
        {;
            menu.Display(playerController, 0);
        });
    }

    public void CloseMenu()
    {
        MenuManager.CloseActiveMenu(playerController);
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