# Mapコンフィグのカスタマイズ

## Mapコンフィグの配置方法

このプラグインでは以下の2つの方法をサポートしています。

### 1. maps.tomlがある場合

`config/maps.toml`がある場合は、`maps.toml`から設定をロードします。

```
.
└── MapChooserSharp/
    ├── config/
    │   └── maps.toml
    └── MapChooserSharp.dll
```

### 2. xxx.tomlがある場合

一つのファイルに集約させるのではなく、以下のような形でコンフィグを分けることができます。
この方法を取る場合は、`maps.toml`はファイル名に使用しないてください。

また、最低限一つのtomlファイルは`config/`フォルダ直下に配置してください。

```
.
└── MapChooserSharp/
    ├── config/
    │   ├── ze_maps/
    │   │   └── ze_xxxxx_v1.toml
    │   ├── de_maps/
    │   │   ├── de_dust2.toml
    │   │   └── de_mirage.toml
    │   ├── surf_maps/
    │   │   ├── surf_xxxxx.toml
    │   │   └── surf_yyyyy.toml
    │   └── default.toml
    └── MapChooserSharp.dll
```

## 設定の適用順とグループについて

### マップの優先度

マップの設定優先度は `マップ > グループ > デフォルト` のようにマップの設定が一番優先度が高いです。

もし、`MinPlayers = 0`をデフォルトで設定し、グループでは`MinPlayers = 16`とし、マップの設定ではこのグループを継承し`MinPlayers = 32`と設定した場合、最終的な`MinPlayers`の値は`32`になります。

### マップの設定適用順

ここでは`ze_example_xyz`をマップコンフィグ例として解説します。

### 1. デフォルト値

まず最初に、マップコンフィグは、`[MapChooserSharpSettings.Default]`内の設定値を取得します。

この時点でのコンフィグをファイルに起こすと...

---

デフォルトファイルが次のような場合

```toml
[MapChooserSharpSettings.Default]
MinPlayers = 0
MaxPlayers = 0
OnlyNomination = false
```

以下のようになります。

```toml
[ze_example_xyz]
MinPlayers = 0
MaxPlayers = 0
OnlyNomination = false
```

### 2. グループ設定

次にMapChooserSharpではマップにグループ設定を適用することができます。

グループは`MapChooserSharpSettings.Groups.<GroupName>`で定義できます。
```toml
[MapChooserSharpSettings.Groups.Group1]
MinPlayers = 16
OnlyNomination = true
```

そして、`MapChooserSharpSettings.Groups`の後ろの`Group1`のところがグループ名となります。

```toml
[ze_example_xyz]
GroupSettings = ["Group1"]
```

### グループの設定方法

マップのグループは複数持つことができますが、一番最初のグループの設定の優先度が高くなります。


グループの使用は次のように行えます。

```toml
[ze_example_789]
GroupSettings = ["Group1", "Group2", "Group3", ......]
```

この際、Group設定の中ではGroup1が一番優先度が高いため、Group2やGroup3で適用された値を上書きします。

GroupはDefault設定より優先度が高いため、`MinPlayers`がデフォルトの`0`から`16`に変わります。

また、`OnlyNomination`も`false`から`true`になります。

ひとまず現時点で、プラグインにロードされたデータをコンフィグで再現すると次のようになります

```toml
[ze_example_xyz]
MinPlayers = 16
MaxPlayers = 0
OnlyNomination = true
```

### 3. マップ設定

次にプラグインはマップのコンフィグデータを読み取ります。

以下のような定義があったとします。

```toml
[ze_example_xyz]
MinPlayers = 32
```

そうなると、前述の通り優先度はマップが一番高いため、最終的にプラグインにロードされたデータをコンフィグで再現すると次のようになります。

```toml
[ze_example_xyz]
MinPlayers = 32
MaxPlayers = 0
OnlyNomination = true
```

### 4. 一部の例外

以下に例示する値は上書きではなく統合されます。
- AllowedSteamIds
- DisallowedSteamIds
- Extra設定

---

例えば、以下のようなコンフィグがあったとします。

グループ設定1

```toml
[MapChooserSharpSettings.Groups.Group1]
MinPlayers = 0
AllowedSteamIds = [123012301230]

[MapChooserSharpSettings.Groups.Group1.extra.shop]
cost = 100
```
グループ設定2

```toml
[MapChooserSharpSettings.Groups.Group2]
MinPlayers = 32
AllowedSteamIds = [123456789]
DisallowedSteamIds = [987654321]

[MapChooserSharpSettings.Groups.Group2.extra.AnotherShop]
cost = 999
```

マップ設定

```toml
[ze_example_xyz]
MinPlayers = 40
AllowedSteamIds = [000000]
DisallowedSteamIds = [000000]
GroupSettings = ["Group1", "Group2"]

[ze_example_xyz.extra.ExternalShop]
cost = 10000
```

これらは最終的に以下のような形になります。

```toml
[ze_example_xyz]
MinPlayers = 40
AllowedSteamIds = [000000, 123012301230, 123456789]
DisallowedSteamIds = [000000, 987654321]

[ze_example_xyz.extra.ExternalShop]
cost = 10000

[ze_example_xyz.extra.AnotherShop]
cost = 999

[ze_example_xyz.extra.extra.shop]
cost = 100
```

このような仕組みでMapChooserSharpはコンフィグの柔軟性を実現しています。

---

## 設定値の詳細

```
[ze_example_abc]
MapNameAlias = "ze example a b c"
MapDescription = "This map contains a jump scare"
IsDisabled = false
WorkshopId = 1234567891234
OnlyNomination = false
Cooldown = 60
MaxExtends = 3
ExtendTimePerExtends = 15
MapTime = 20
ExtendRoundsPerExtends = 5
MapRounds = 10
RequiredPermissions = ["css/generic"]
RestrictToAllowedUsersOnly = false
AllowedSteamIds = [76561198815323852]
DisallowedSteamIds = [76561198815323852]
MaxPlayers = 64
MinPlayers = 10
ProhibitAdminNomination = false
DaysAllowed = ["wednesday", "monday"]
AllowedTimeRanges = ["10:00-12:00", "20:00-22:00", "22:00-03:00"]

[ze_example_abc.extra.shop]
cost = 100
```

## マップ全般

### MapNameAlias

投票画面等で表示される名前を変更できます。

### MapDescription

`!mapinfo`コマンドで表示される内容をここで指定できます。

### IsDisabled

マップが有効か否かを指定します。
ここで無効化されている場合は、マップはノミネーションできず、投票でのランダムマップ選択にも選ばれません。

### WorkshopId

ワークショップの場合はこの値を指定することで、マップ名が変わってもコンフィグを編集する必要がなくなります。

このキーを設定しなかった場合、ワークショップマップは`ds_workshop_changelevel <keyName>` 公式マップは `changelevel <keyName>` でマップ変更がなされます。

また、マップ名としてトップのキーの名前を使用します。 例: `[ze_example_abc]` の場合は `ds_workshop_changelevel ze_example_abc` になります。

### OnlyNomination

ノミネート限定にするか否かを指定します。
ここで有効化された場合、投票でのランダムマップ選択にも選ばれません。

### Cooldown

マップがプレイされた後に適用されるクールダウンを指定します。
グループクールダウンとはまた別のため注意してください。

### MaxExtends

マップの最大延長回数を指定します。

### ExtendTimePerExtends

マップが時間ベースの場合に、延長される度に何分延長するかを指定します。

### MapTime

現在は使用していません。

マップが時間ベースの場合に、マップの時間を指定します。

### ExtendRoundsPerExtends

マップがラウンドベースの場合に、延長される度に何ラウンド延長するかを指定します。

### MapRounds

現在は使用していません。

マップがラウンドベースの場合に、マップのラウンド数を指定します。

## ノミネート関連

### RequiredPermissions

ノミネートに必要な権限を指定します。
複数指定すると、指定された権限のどれかを持っていればノミネート可能になります。

### RestrictToAllowedUsersOnly

有効化された場合AllowedSteamIdsに含まれているユーザーのみがノミネート出来るように制限します。

### AllowedSteamIds

このリストに含まれているユーザーは、すべての権限設定を回避してノミネートが可能になります。
追加するにはSteamID 64を指定します。

### DisallowedSteamIds

このリストに含まれているユーザーは、どのような形であってもノミネートが不可能になります。

### MaxPlayers

サーバー内の人数がこの数値より大きい場合ノミネートができなくなります。

### MinPlayers

サーバー内の人数がこの数値より小さい場合ノミネートができなくなります。

### ProhibitAdminNomination

`!nominate_addmap` でのノミネートを禁止します。

### DaysAllowed

ここに記載された日付のみノミネートを可能にします。 また、AllowedTimeRangesと同時に適用されるため日付の指定にはご注意ください。

`DaysAllowed = ["wednesday", "monday"]` は、水曜日と月曜日にのみノミネートが可能になります。

### AllowedTimeRanges

ここに記載された時間帯のみノミネートを可能にします。 また、DaysAllowedと同時に適用されるため時間帯の指定にはご注意ください。

`AllowedTimeRanges = ["10:00-12:00", "22:00-03:00"]` は 10:00 - 12:00 か 22:00 - 03:00 の間にのみノミネートが可能になります。

### Extra 設定

[MCS API ドキュメント](../development/USING_MCS_API.md) を確認してください。