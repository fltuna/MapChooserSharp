using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.Nomination;
using MapChooserSharp.API.Events.RockTheVote;

namespace MapChooserSharp.API.Example;

public class MapChooserSharpApiExample: BasePlugin
{
    public override string ModuleName => "MapChooserSharp API Example";
    public override string ModuleVersion => "0.0.1";

    private IMapChooserSharpApi _mcsApi = null!;
    
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        try
        {
            var api = IMapChooserSharpApi.Capability.Get();

            _mcsApi = api ?? throw new NullReferenceException("IMapChooserSharpApi is null");
        }
        catch (Exception)
        {
            throw new InvalidOperationException("IMapChooserSharpApi is not available");
        }

        
        AddCommand("mcsapi_test_nomination", "Test nomination with API", CommandTestMapNomination);
        
        
        _mcsApi.EventSystem.RegisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsApi.EventSystem.RegisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
        _mcsApi.EventSystem.RegisterEventHandler<McsPlayerRtvCastEvent>(OnPlayerRtv);
        _mcsApi.EventSystem.RegisterEventHandler<McsAdminForceRtvEvent>(OnForceRtv);
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommand("mcsapi_test_nomination", CommandTestMapNomination);
        
        
        _mcsApi.EventSystem.UnregisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsApi.EventSystem.UnregisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
        _mcsApi.EventSystem.UnregisterEventHandler<McsPlayerRtvCastEvent>(OnPlayerRtv);
        _mcsApi.EventSystem.UnregisterEventHandler<McsAdminForceRtvEvent>(OnForceRtv);
    }


    private void CommandTestMapNomination(CCSPlayerController? playerController, CommandInfo info)
    {
        // IMcsNominationApi::NominateMap() API is not allow console player to nominate
        // Use IMcsNominationApi::AdminNominateMap can nominate map from console
        if (playerController == null)
            return;

        if (info.ArgCount < 2)
        {
            playerController.PrintToChat("Usage: mcsapi_test_nomination <MapName>");
            return;
        }
        
        
        // TODO: Expose map config api and implement sample program
        
        // _mcsApi.MapConfig
        
        // _mcsApi.McsNominationApi.NominateMap(playerController, );
    }
    

    private McsEventResultWithCallback OnMapNominationBegin(McsNominationBeginEvent @event)
    {
        if (@event.Player != null && @event.Player.PlayerPawn.Value != null && @event.Player.PlayerPawn.Value.Health < 80)
        {
            return McsEventResultWithCallback.Stop(result =>
            {
                @event.Player!.PrintToChat($"[McsMapNominationBeginEvent Listener] Your health is not enough to nominate!!! Cancelling nomination. Status: {result}");
                @event.Player.PrintToChat($"{@event.ModulePrefix} You can use MCS module's prefix!");
            });
        }

        return McsEventResult.Continue;
    }

    private void OnMapNominated(McsMapNominatedEvent @event)
    {
        var player = @event.Player;
        
        if (player == null)
        {
            Server.PrintToChatAll($"[McsMapNominatedEvent Listener] detected CONSOLE nominated {@event.NominationData.MapConfig.MapName}");
        }
        else
        {
            Server.PrintToChatAll($"[McsMapNominatedEvent Listener] detected {player.PlayerName} nominated {@event.NominationData.MapConfig.MapName}");
        }
    }


    private McsEventResultWithCallback OnPlayerRtv(McsPlayerRtvCastEvent @event)
    {
        Server.PrintToChatAll($"{@event.ModulePrefix} {@event.Player.PlayerName} cast rtv");
        return McsEventResult.Continue;
    }
    
    private McsEventResultWithCallback OnForceRtv(McsAdminForceRtvEvent @event)
    {
        var player = @event.Player;

        if (player == null)
        {
            Server.PrintToChatAll($"{@event.ModulePrefix} {ChatColors.DarkRed}CONSOLE{ChatColors.Default} cast force rtv");
        }
        else
        {
            Server.PrintToChatAll($"{@event.ModulePrefix} {player.PlayerName}cast force rtv");
        }
        
        return McsEventResult.Continue;
    }
}