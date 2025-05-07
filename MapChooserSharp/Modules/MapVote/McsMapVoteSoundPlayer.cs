using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
            PlayVoteCountdownStartSoundToPlayer(player, sound);
        }
    }

    private void PlayVoteCountdownStartSoundToPlayer(CCSPlayerController player, string soundName)
    {
        // TODO() Sound volume per player
        player.EmitSound(soundName);
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
            PlayVoteCountdownSoundToPlayer(player, sound);
        }
    }
    
    private void PlayVoteCountdownSoundToPlayer(CCSPlayerController player, string soundName)
    {
        // TODO() Sound volume per player
        player.EmitSound(soundName);
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
            PlayVoteStartSoundToPlayer(player, sound);
        }
    }
    
    private void PlayVoteStartSoundToPlayer(CCSPlayerController player, string soundName)
    {
        // TODO() Sound volume per player
        player.EmitSound(soundName);
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
            PlayVoteFinishedSoundToPlayer(player, sound);
        }
    }

    private void PlayVoteFinishedSoundToPlayer(CCSPlayerController player, string soundName)
    {
        // TODO() Sound volume per player
        player.EmitSound(soundName);
    }
}