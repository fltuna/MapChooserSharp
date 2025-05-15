# プラグインコンフィグのカスタマイズ



## 一般設定

```toml
#MapChooserSharp Plugin Configuration

[General]
# Should use alias map name if available? (This will take effect to all things that prints a map name)
ShouldUseAliasMapNameIfAvailable = true

# Should print the cooldown? 
# if true, and commands in cooldown, it will show cooldown message with seconds
# if false, and commands in cooldown, it will show only cooldown message
VerboseCooldownPrint = true

```

### ShouldUseAliasMapNameIfAvailable

マップコンフィグにAliasMapNameが存在する場合、Nominationメニューや投票メニューにそのエイリアス名を表示するかどうか

### VerboseCooldownPrint

RTVコマンドなどがクールダウンのときに秒数を表示するかどうか


## SQL設定

```toml
[General.Sql]
# SQL settings for MapChooserSharp

# What SQL provider should be use?
#
# Currently Supports:
# - Sqlite
#
# See GitHub readme for more and updated information.
Type = "sqlite"
Address = ""
User = ""
Password = ""

GroupInformationTableName = "McsGroupInformation"
MapInformationTableName = "McsMapInformation"
```

### Type

データベースタイプをここで指定します。 現在はSqliteのみサポートしています。

### Address, User, Password

これは将来的にMySQLやPostgreSQLをサポートした際に、データベースに接続するために必要になります。

### GroupInformationTableName

グループ情報のテーブル名を指定できます。

### MapInformationTableName

マップ情報のテーブル名を指定できます。

## MapCycle

```toml
[MapCycle]
# Fallback settings for maps with no config
# These settings are ignored when map has a config.

# How many extends allowed if map is not in map config.
FallbackMaxExtends = 3

# How many times allowed to extend a map using !ext command
FallbackMaxExtCommandUses = 1

# How long to extend when map is extended in time left/ round time based game?
FallbackExtendTimePerExtends = 15

# How long to extend when map is extended in round based game?
FallbackExtendRoundsPerExtends = 5
```

### FallbackMaxExtends

コンフィグに存在しないマップを遊んでいる際のデフォルトの最大延長回数を指定できます。

### FallbackMaxExtCommandUses

コンフィグに存在しないマップを遊んでいる際の`!ext`コマンドによる延長のデフォルトの最大延長回数を指定できます。

### FallbackExtendTimePerExtends

コンフィグに存在しないマップを遊んでいる際のデフォルトの`mp_timelimit`もしくは`mp_roundtime`の延長時間を分単位で指定できます。

#### FallbackExtendRoundsPerExtends

コンフィグに存在しないマップを遊んでいる際のデフォルトの`mp_maxrounds`をラウンド単位で指定できます。

## MapVote

```toml
[MapVote]
# What menu type should be use?
#
# Currently supports:
# - BuiltInHtml
# - Cs2ScreenMenuApi
# - Cs2MenuManagerScreen
#
# See GitHub readme for more information.
MenuType = "BuiltInHtml"

# How many maps should be appeared in map vote?
# I would recommend to set 5 when you using BuiltInHtml menu
MaxVoteElements = 5

# Should print vote text to everyone?
ShouldPrintVoteToChat = true

# Should print the vote remaining time?
ShouldPrintVoteRemainingTime = true


# What countdown ui type should be use?
#
# Currently supports:
# - None
# - CenterHud
# - CenterAlert
# - CenterHtml
# - Chat
#
# See GitHub readme for more information.
CountdownUiType = "CenterHtml"
```

### MenuType

投票に使用するメニュータイプを指定できます。

追加のプラグイン無しで使用できるのは `BuiltInHtml` のみで、追加で以下のプラグインを追加することによって別のメニュータイプを使用することが出来るようになります。

また、無効もしくは不正な値が入力された場合は `BuiltInHtml` にすべてフォールバックされます。

- Cs2ScreenMenuApiの使用には [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI) が必要です
- Cs2MenuManagerScreenの使用には [CS2MenuManager](https://github.com/schwarper/CS2MenuManager) が必要です

### MaxVoteElements

1回の投票で出てくるマップ数を指定します。 実際のところは`Extend`と`Don't change`が入るためMaxVoteElements - 1の数がマップ数となります。

### ShouldPrintVoteToChat

プレイヤーの投票時に、チャットに投票先を表示するかを指定できます。

### ShouldPrintVoteRemainingTime

投票中に残り時間を表示するかを指定できます。

### CountdownUiType

投票開始前のカウントダウンをどのように表示するかを指定できます。


### MapVote Sound

```toml
[MapVote.Sound]
# Sound setting of map vote
# If you leave value as blank, then no sound will played.


# Path to .vsndevts. file extension should be end with `.vsndevts`
# If you already precached a .vsndevts file in another plugin, then you can leave as blank.
SoundFile = ""


# Initial vote sounds

# This sound will be played when starting initial vote countdown
InitialVoteCountdownStartSound = ""

# This sound will be played when starting initial vote
InitialVoteStartSound = ""

# This sound will be played when finishing initial vote (This sound will not be played when runoff vote starts)
InitialVoteFinishSound = ""

# Vote countdown sound mapped to its seconds
InitialVoteCountdownSound1 = ""
InitialVoteCountdownSound2 = ""
InitialVoteCountdownSound3 = ""
InitialVoteCountdownSound4 = ""
InitialVoteCountdownSound5 = ""
InitialVoteCountdownSound6 = ""
InitialVoteCountdownSound7 = ""
InitialVoteCountdownSound8 = ""
InitialVoteCountdownSound9 = ""
InitialVoteCountdownSound10 = ""


# Runoff vote sounds

# This sound will be played when starting runoff vote countdown
RunoffVoteCountdownStartSound = ""

# This sound will be played when starting runoff vote
RunoffVoteStartSound = ""

# This sound will be played when finishing runoff vote
RunoffVoteFinishSound = ""


# Runoff vote countdown sound mapped to its seconds
RunoffVoteCountdownSound1 = ""
RunoffVoteCountdownSound2 = ""
RunoffVoteCountdownSound3 = ""
RunoffVoteCountdownSound4 = ""
RunoffVoteCountdownSound5 = ""
RunoffVoteCountdownSound6 = ""
RunoffVoteCountdownSound7 = ""
RunoffVoteCountdownSound8 = ""
RunoffVoteCountdownSound9 = ""
RunoffVoteCountdownSound10 = ""
```

### SoundFile

Precacheのために `.vsndevts`までのファイルパスを指定します。 例: `soundevents/soundevents_mapchooser.vsndevts`

もし他のプラグインなどで既に当該のファイルをprecacheしている場合は空欄で大丈夫です。

### Initial Vote

### InitialVoteCountdownStartSound

最初の投票のカウントダウン開始時に再生される音声を指定できます。

### InitialVoteStartSound

最初の投票の開始時に再生される音声を指定できます。

### InitialVoteFinishSound

投票が可決された際に再生される音声を指定できます。

### InitialVoteCountdownSoundXX

数字に対応した秒数に再生される音声を指定できます。

### Runoff Vote

### RunoffVoteCountdownStartSound

Runoff投票のカウントダウン開始時に再生される音声を指定できます。

### RunoffVoteStartSound

Runoff投票の開始時に再生される音声を指定できます。

### RunoffVoteFinishSound

投票が可決された際に再生される音声を指定できます。

### RunoffVoteCountdownSoundXX

Runoff投票のカウントダウン開始時に再生される音声を指定できます。


## Nomination

```toml
[Nomination]
# What menu type should be use?
#
# Currently supports:
# - BuiltInHtml
# - Cs2ScreenMenuApi
# - Cs2MenuManagerScreen
#
# See GitHub readme for more information.
MenuType = "BuiltInHtml"
```

### MenuType
ノミネートに使用するメニュータイプを指定できます。

追加のプラグイン無しで使用できるのは `BuiltInHtml` のみで、追加で以下のプラグインを追加することによって別のメニュータイプを使用することが出来るようになります。

また、無効もしくは不正な値が入力された場合は `BuiltInHtml` にすべてフォールバックされます。

- Cs2ScreenMenuApiの使用には [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI) が必要です
- Cs2MenuManagerScreenの使用には [CS2MenuManager](https://github.com/schwarper/CS2MenuManager) が必要です

## ConfigInformation

```toml
[ConfigInformation]
Version = "0.0.1"
```


### Version

コンフィグのバージョンです。 バージョン変更があった際にコンソールに警告をしてくれるので、更新時以外は変更しないでください。