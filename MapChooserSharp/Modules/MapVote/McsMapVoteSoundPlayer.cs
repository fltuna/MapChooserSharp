using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.Modules.PluginConfig.Interfaces;

namespace MapChooserSharp.Modules.MapVote;

internal class McsMapVoteSoundPlayer(IMcsVoteSoundConfig config)
{
    public void PlayVoteCountdownStartSoundToAll(bool isRunoffVote)
    {
        string sound = isRunoffVote
            ? config.RunoffVoteSounds.VoteCountdownStartSound
            : config.InitialVoteSounds.VoteCountdownStartSound;
        
        if (string.IsNullOrEmpty(sound))
            return;
        
        
        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }))
        {
            PlaySoundToPlayer(player, sound);
        }
    }
    
    public void PlayVoteCountdownSoundToAll(int seconds, bool isRunoffVote)
    {
        if (seconds >= 11 || seconds < 1)
            return;

        string sound = isRunoffVote
            ? config.RunoffVoteSounds.VoteCountdownSounds[seconds - 1]
            : config.InitialVoteSounds.VoteCountdownSounds[seconds - 1];
        
        if (string.IsNullOrEmpty(sound))
            return;
        
        
        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }))
        {
            PlaySoundToPlayer(player, sound);
        }
    }
    
    public void PlayVoteStartSoundToAll(bool isRunoffVote)
    {
        string sound = isRunoffVote
            ? config.RunoffVoteSounds.VoteStartSound
            : config.InitialVoteSounds.VoteStartSound;
        
        if (string.IsNullOrEmpty(sound))
            return;
        
        
        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }))
        {
            PlaySoundToPlayer(player, sound);
        }
    }

    public void PlayVoteFinishedSoundToAll(bool isRunoffVote)
    {
        string sound = isRunoffVote
            ? config.RunoffVoteSounds.VoteFinishSound
            : config.InitialVoteSounds.VoteFinishSound;
        
        if (string.IsNullOrEmpty(sound))
            return;
        
        
        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => p is { IsBot: false, IsHLTV: false }))
        {
            PlaySoundToPlayer(player, sound);
        }
    }

    private void PlaySoundToPlayer(CCSPlayerController player, string soundName, float volume = 1.0F)
    {
        // TODO() Sound volume per player
        var playerPawn = player.PlayerPawn.Value;
        
        if (playerPawn == null)
            return;

        playerPawn.EmitSound(soundName, new RecipientFilter(player), volume);
    }
}