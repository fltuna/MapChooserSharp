using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Menu;
using MapChooserSharp.API.MapVoteController;
using MapChooserSharp.Modules.MapVote;
using MapChooserSharp.Modules.McsMenu.Interfaces;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;
using MapChooserSharp.Modules.Nomination;
using MapChooserSharp.Modules.PluginConfig.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;
using TNCSSPluginFoundation.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.BuiltInHtml;

public class McsBuiltInHtmlNominationUi(CCSPlayerController playerController, IServiceProvider provider): IMcsNominationUserInterface
{
    private IMcsGeneralMenuOption? _generalMenuOption;
    
    private List<IMcsNominationMenuOption> _nominationMenuOptions = new();

    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    private readonly IMcsPluginConfigProvider _mcsPluginConfigProvider = provider.GetRequiredService<IMcsPluginConfigProvider>();
    
    private readonly IDebugLogger _debugLogger = provider.GetRequiredService<IDebugLogger>();
    
    private readonly McsMapVoteController _voteController = provider.GetRequiredService<McsMapVoteController>();
    
    public int NominationOptionCount => _nominationMenuOptions.Count;
    
    public void OpenMenu()
    {
        if (_voteController.CurrentVoteState != McsMapVoteState.NoActiveVote)
            return;
        
        List<ChatMenuOption> chatMenuOptions = new();
        
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
        
        CenterHtmlMenu menu = new(menuTitle.ToString(), _plugin);
        
        foreach (var (option, index) in _nominationMenuOptions.Select((value, i) => (value, i)))
        {
            StringBuilder builder = new();
            
            // TODO() Use Alias name if enabled and available
            // TODO() Truncate MapName if too long
            builder.Append(option.NominationOption.MapConfig.MapName);
            
            chatMenuOptions.Add(new ChatMenuOption(builder.ToString(), option.MenuDisabled, (_, _) =>
            {
                _nominationMenuOptions[index].SelectionCallback.Invoke(playerController, option.NominationOption);
            }));
        }
        
        menu.MenuOptions.Clear();
        menu.MenuOptions.AddRange(chatMenuOptions);
        
        MenuManager.OpenCenterHtmlMenu(_plugin, playerController, menu);
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