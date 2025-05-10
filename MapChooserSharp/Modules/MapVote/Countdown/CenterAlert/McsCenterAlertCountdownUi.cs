using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MapChooserSharp.Modules.MapVote.Countdown.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TNCSSPluginFoundation;

namespace MapChooserSharp.Modules.MapVote.Countdown.CenterAlert;

public class McsCenterAlertCountdownUi(IServiceProvider provider): IMcsCountdownUi
{
    private readonly TncssPluginBase _plugin = provider.GetRequiredService<TncssPluginBase>();
    
    public void ShowCountdownToPlayer(CCSPlayerController player, int secondsLeft, McsCountdownType countdownType)
    {
        switch (countdownType)
        {
            case McsCountdownType.VoteStart:
                player.PrintToCenterAlert(_plugin.LocalizeStringForPlayer(player, "MapVote.Broadcast.Countdown", secondsLeft));
                break;
                
            case McsCountdownType.Voting:
                player.PrintToCenterAlert(_plugin.LocalizeStringForPlayer(player, "MapVote.Broadcast.Voting.VoteEndCountdown", secondsLeft));
                break;
        }
        
    }

    public void Close(CCSPlayerController player)
    {
        player.PrintToCenterAlert(" ");
    }
}