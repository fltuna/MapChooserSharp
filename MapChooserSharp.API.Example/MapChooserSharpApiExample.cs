using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using MapChooserSharp.API.Events;
using MapChooserSharp.API.Events.Commands;
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

        // Nomination API example
        AddCommand("mcsapi_test_nomination", "Test nomination with API", CommandTestMapNomination);
        
        // Event API example
        _mcsApi.EventSystem.RegisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsApi.EventSystem.RegisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
        _mcsApi.EventSystem.RegisterEventHandler<McsPlayerRtvCastEvent>(OnPlayerRtv);
        _mcsApi.EventSystem.RegisterEventHandler<McsAdminForceRtvEvent>(OnForceRtv);
        
        _mcsApi.EventSystem.RegisterEventHandler<McsMapInfoCommandExecutedEvent>(OnMapInfoCommandExecuted);
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommand("mcsapi_test_nomination", CommandTestMapNomination);
        
        
        _mcsApi.EventSystem.UnregisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
        _mcsApi.EventSystem.UnregisterEventHandler<McsMapNominatedEvent>(OnMapNominated);
        _mcsApi.EventSystem.UnregisterEventHandler<McsPlayerRtvCastEvent>(OnPlayerRtv);
        _mcsApi.EventSystem.UnregisterEventHandler<McsAdminForceRtvEvent>(OnForceRtv);
        
        _mcsApi.EventSystem.RegisterEventHandler<McsMapInfoCommandExecutedEvent>(OnMapInfoCommandExecuted);
    }


    private void CommandTestMapNomination(CCSPlayerController? playerController, CommandInfo info)
    {
        // IMcsNominationApi::NominateMap() API is not allow nomination from console
        // Use IMcsNominationApi::AdminNominateMap instead to nominate map from console
        if (playerController == null)
            return;

        if (info.ArgCount < 2)
        {
            playerController.PrintToChat("Usage: mcsapi_test_nomination <MapName>");
            return;
        }
        
        string mapName = info.ArgByIndex(1);
        
        var mapConfigs = _mcsApi.McsMapConfigProviderApi.GetMapConfigs();

        var matchedMap = mapConfigs.Where(kv => kv.Key.Contains(mapName)).ToDictionary();

        if (!matchedMap.Any())
        {
            playerController.PrintToChat($"{mapName} is not found in map config list");
            return;
        }
        
        if (matchedMap.Count > 1)
        {
            playerController.PrintToChat($"Multiple map found with {mapName}");
            return;
        }
        
        // You don't have to check permission, check number of users, etc... Because it will check by MCS side.
        _mcsApi.McsNominationApi.NominateMap(playerController, matchedMap.First().Value);
    }

    // This is just an example value.
    private const int PlayerShopBalance = 100;
    
    private McsEventResultWithCallback OnMapNominationBegin(McsNominationBeginEvent @event)
    {
        // Mapped with end of extra config setting name
        // [ze_example_abc.extra.shop]
        if (@event.NominationData.MapConfig.ExtraConfiguration.TryGetValue("shop", out var shopSettings))
        {
            if (shopSettings.TryGetValue("cost", out var cost))
            {
                int costValue = int.Parse(cost);

                if (PlayerShopBalance >= costValue)
                    return McsEventResult.Continue;
                
                // If player doesn't have enough money, you can stop nomination
                return McsEventResultWithCallback.Stop(result =>
                {
                    @event.Player.PrintToChat($"{@event.ModulePrefix} You don't have enough money to nomiante this map!");
                });
            }
        }
        
        // Just a working example
        if (@event.Player.PlayerPawn.Value != null && @event.Player.PlayerPawn.Value.Health < 80)
        {
            // When you stopping the event, the original MCS methods are completely cancelled and no messages will print.
            // This is a API responcibility, You should'nt forget to notify the player.
            return McsEventResultWithCallback.Stop(result =>
            {
                @event.Player.PrintToChat($"[McsMapNominationBeginEvent Listener] Your health is not enough to nominate!!! Cancelling nomination. Status: {result}");
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
            Server.PrintToChatAll($"{@event.ModulePrefix} {player.PlayerName} cast force rtv");
        }
        
        return McsEventResult.Continue;
    }


    private void OnMapInfoCommandExecuted(McsMapInfoCommandExecutedEvent @event)
    {
        var player = @event.Player;
        
        player.PrintToChat($"{@event.ModulePrefix} Additional map info: TEST");
    }
}