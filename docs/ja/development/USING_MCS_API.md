# MCS API ドキュメント

MapChooserSharp API (以下 MCS API) では柔軟なAPIを提供し、外部のプラグインから様々な操作を行えるようになっています。

このドキュメントではそのMCS APIの使用方法などについて解説していきます。

## APIの取得

APIは `IMapChooserSharpApi.Capability.Get()` メソッドを通じて取得できます。

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

## Eventシステムを活用する

MCS APIではEventシステムを提供しており、マップの投票開始時やプレイヤーがノミネートする時、RTVする時などなど... 様々なイベントをListenし自由に機能を実装可能です。

ここでは一例と、イベントの探し方について解説していきます。

### EventをListenする

EventをListenするときは`_IMapChooserSharpApi.EventSystem.RegisterEventHandler()` を呼び出してイベントリスナーを登録することができます。

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

### Listenするときは...

一応任意ではありますが、予期しない動作を防ぐために、アンロード時に `IMapChooserSharpApi.EventSystem.UnregisterEventHandler()` を呼び出してリスナーを削除するのをおすすめします。

```csharp
public override void Unload(bool hotReload)
{
    _mcsApi.EventSystem.UnregisterEventHandler<McsNominationBeginEvent>(OnMapNominationBegin);
}
```

### Listenerのサンプル (キャンセルできるタイプ)

ここでは、OnMapNominationBeginイベントをListenしてみます。 

キャンセルできるタイプのイベントはMcsEventResultWithCallbackを使用してイベントをキャンセルできます。また、必要に応じて引数から値を受け取ります。

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

### Listenerのサンプル (キャンセルできないタイプ)

ここでは、OnMapNominatedイベントをListenしてみます。

キャンセルできないタイプのイベントはvoidで定義し、必要に応じて引数から値を受け取ります。

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

### MapConfig関連の情報

グループや、マップには追加の情報を定義することが可能で、これはnominationイベントの引数から情報を取得し活用できます。

例えば以下のようなコンフィグがあった場合は

```toml
[ze_example_abc.extra.shop]
cost = 100
```

次のようにリスナーから取得できます。

以下のコードでは、マップ名が意図した名前と一致していた場合に、shopキーからDictionaryを取得し、そのDictionaryのcostキーからコスト情報を取得しています。

そして、`PlayerShopBalance`でプレイヤーの持っているショップクレジットを確認し(サンプルコードではハードコードされているが) ノミネート可能かを確認しています。

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


### Listenerの責任

キャンセルできるタイプのイベントは、キャンセルされた場合その後の後続のタスクをすべてキャンセルするため、プレイヤーに失敗した事などを伝えるなど、状態の報告はAPI側にあります。

プレイヤーを混乱させないためにも、キャンセルする際は必ずプレイヤーに対して通知を出すようにしましょう。


### Eventの探し方

EventはAPIの[Eventsパッケージ](../../../MapChooserSharp.API/Events) に格納されているのでそこから探してください。


## MapCycle Controller APIを活用する

Map Cycle Controllerでは、次のマップの変更や、次のマップへの移動、残りの延長回数等を取得することができます。

詳細は [IMcsMapCycleControllerApi.cs](../../../MapChooserSharp.API/MapCycleController/IMcsMapCycleControllerApi.cs) を確認してください。


## MapCycle Extend Controller APIを活用する

MapCycle Extend Controllerでは、現在のマップの延長、`!ext`コマンドの使用可能回数の入手, 変更、 そして`!ext`コマンドの使用制限をかけることが可能です。

詳細は [IMcsMapCycleExtendControllerApi.cs](../../../MapChooserSharp.API/MapCycleController/IMcsMapCycleExtendControllerApi.cs) を確認してください。


## MapCycle Extend Vote Controller APIを活用する

MapCycle Extend Vote Controllerでは、 CS2の投票UIを使用して延長投票を開始することができます。

詳細は [IMcsMapCycleExtendVoteControllerApi.cs](../../../MapChooserSharp.API/MapCycleController/IMcsMapCycleExtendVoteControllerApi.cs) を確認してください。


## MapVote Controller APIを活用する

MapVote Controller APIでは、投票の開始、キャンセル等を行うことができます。

詳細は [IMcsMapVoteControllerApi.cs](../../../MapChooserSharp.API/MapVoteController/IMcsMapVoteControllerApi.cs) を確認してください。


## Nomination APIを活用する

Nomination APIでは、ノミネートされたマップの確認、マップのノミネート、ノミネートの削除等が行えます。

詳細は [IMcsNominationApi.cs](../../../MapChooserSharp.API/Nomination/IMcsNominationApi.cs) を確認してください。


## RTV Controller API を活用する

RTV Controller APIでは、プレイヤーをRTVに参加させたり、FoceRTV等を行うことができます。

詳細は [IMcsRtvControllerApi.cs](../../../MapChooserSharp.API/RtvController/IMcsRtvControllerApi.cs) を確認してください。

