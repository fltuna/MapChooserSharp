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

ワークショップの場合はこの値を指定することで、マップ名が変わってもコンフィグを編集するがなくなります。

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