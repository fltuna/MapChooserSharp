using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.McsMenu.NominationMenu.Interfaces;

namespace MapChooserSharp.Modules.McsMenu.NominationMenu.BuiltInHtml;

public class McsBuiltInHtmlNominationUiFactory(IServiceProvider provider): IMcsNominationUiFactory
{
    public IMcsNominationUserInterface Create(CCSPlayerController player)
    {
        return new McsBuiltInHtmlNominationUi(player, provider);
    }
}