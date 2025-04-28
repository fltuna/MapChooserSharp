using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.Nomination;

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

        
        
        _mcsApi.EventSystem.RegisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsApi.EventSystem.RegisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
    }

    public override void Unload(bool hotReload)
    {
        _mcsApi.EventSystem.UnregisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsApi.EventSystem.UnregisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
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
}