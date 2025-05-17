# MCS API Documentation

Translation and translation assist by Claude.ai

MapChooserSharp API (hereinafter referred to as MCS API) provides a flexible API that allows various operations from external plugins.

This document explains how to use the MCS API.

## Obtaining the API

The API can be obtained through the `IMapChooserSharpApi.Capability.Get()` method.

```csharp
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
}
```

## Utilizing the Event System

The MCS API provides an Event system that allows you to listen to various events such as when a map vote starts, when a player nominates, when they RTV, and more... You can freely implement functionality by listening to these events.

Here, we'll explain an example and how to find events.

### Listening to Events

You can register an event listener by calling `_IMapChooserSharpApi.EventSystem.RegisterEventHandler()`.

```csharp
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
}
```

### When Listening...

Although it's optional, to prevent unexpected behavior, it's recommended to call `IMapChooserSharpApi.EventSystem.UnregisterEventHandler()` to remove the listener when unloading.

```csharp
public override void Unload(bool hotReload)
{
    _mcsApi.EventSystem.UnregisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
}
```

### Listener Sample (Cancellable Type)

Here, we'll try listening to the OnMapNominationBegin event.

Cancellable type events can be cancelled using McsEventResultWithCallback. You can also receive values from arguments as needed.

```csharp
private McsEventResultWithCallback OnMapNominationBegin(McsNominationBeginEvent @event)
{
    // Mapped with end of extra config setting name
    // [MapChooserSharp.ze_example_map.Extra.Shop]
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
```

### Listener Sample (Non-Cancellable Type)

Here, we'll try listening to the OnMapNominated event.

Non-cancellable type events are defined as void and receive values from arguments as needed.

```csharp
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
```

### MapConfig Related Information

Additional information can be defined for groups and maps, which can be retrieved and utilized from the arguments of nomination events.

For example, if you have a config like this:

```toml
[ze_example_abc.extra.shop]
cost = 100
```

You can retrieve it from the listener as follows:

The following code gets a Dictionary from the shop key if the map name matches the intended name, and then gets the cost information from the cost key of that Dictionary.

Then, it checks if the player can nominate by checking the shop credits the player has (hardcoded in the sample code) with `PlayerShopBalance`.

```csharp
private McsEventResultWithCallback OnMapNominationBegin(McsNominationBeginEvent @event)
{
    if (!@event.NominationData.MapConfig.MapName.Equals("ze_example_abc", StringComparison.OrdinalIgnoreCase))
        return McsEventResult.Continue;
    
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
}
```

### Listener Responsibilities

For cancellable type events, if cancelled, all subsequent tasks are cancelled, so it's the API's responsibility to report the status, such as informing the player of failure.

To avoid confusing players, always make sure to notify the player when cancelling.


### How to Find Events

Events are stored in the API's [Events package](../../../MapChooserSharp.API/Events), so please look there.


## Utilizing the MapCycle Controller API

With the Map Cycle Controller, you can change the next map, move to the next map, get the remaining number of extensions, etc.

For details, please check [IMcsMapCycleControllerApi.cs](../../../MapChooserSharp.API/MapCycleController/IMcsMapCycleControllerApi.cs).


## Utilizing the MapCycle Extend Controller API

With the MapCycle Extend Controller, you can Extend a current map, and change and get `!ext` command remaining counts. Also, can toggle `!ext` command usage,

For details, please check [IMcsMapCycleExtendControllerApi.cs](../../../MapChooserSharp.API/MapCycleController/IMcsMapCycleExtendControllerApi.cs).


## Utilizing the MapCycle Extend Vote Controller API

With the MapCycle Extend Vote Controller, you can start extend vote with using CS2's Native Vote UI.

For details, please check [IMcsMapCycleExtendVoteControllerApi.cs](../../../MapChooserSharp.API/MapCycleController/IMcsMapCycleExtendVoteControllerApi.cs).


## Utilizing the MapVote Controller API

With the MapVote Controller API, you can start, cancel, etc. votes.

For details, please check [IMcsMapVoteControllerApi.cs](../../../MapChooserSharp.API/MapVoteController/IMcsMapVoteControllerApi.cs).


## Utilizing the Nomination API

With the Nomination API, you can check nominated maps, nominate maps, delete nominations, etc.

For details, please check [IMcsNominationApi.cs](../../../MapChooserSharp.API/Nomination/IMcsNominationApi.cs).


## Utilizing the RTV Controller API

With the RTV Controller API, you can make players participate in RTV, force RTV, etc.

For details, please check [IMcsRtvControllerApi.cs](../../../MapChooserSharp.API/RtvController/IMcsRtvControllerApi.cs).