# Plugin Config Customization

Translation and translation assist by Claude.ai

## General Settings

```toml
#MapChooserSharp Plugin Configuration

[General]
# Should use alias map name if available? (This will take effect to all things that prints a map name)
ShouldUseAliasMapNameIfAvailable = true

# Should print the cooldown? 
# if true, and commands in cooldown, it will show cooldown message with seconds
# if false, and commands in cooldown, it will show only cooldown message
VerboseCooldownPrint = true

# Workshop Collection IDs to automatically fetch maps from
# Example: WorkshopCollectionIds = [ "3070257939", "1234567890" ]
WorkshopCollectionIds = []

# Should automatically fix map name in map settings when map starts?
# This will update the map name in settings to match the actual map name from the server
ShouldAutoFixMapName = true

```

### ShouldUseAliasMapNameIfAvailable

Whether to display the alias name in the Nomination menu and voting menu if AliasMapName exists in the map config.

### VerboseCooldownPrint

Whether to display the number of seconds when RTV commands and other commands are in cooldown.

### WorkshopCollectionIds

An array of collection IDs to automatically fetch maps from Steam Workshop. When the plugin loads, it retrieves map information from the specified collections and automatically creates map settings. Existing map settings (with the same Workshop ID) are skipped.

### ShouldAutoFixMapName

Specifies whether to automatically correct the map name in map settings with the actual map name (Server.MapName) when a map starts. This is useful when the Workshop map title differs from the actual map name.

## SQL Settings

```toml
[General.Sql]
# SQL settings for MapChooserSharp

# What SQL provider should be use?
#
# Currently Supports:
# - Sqlite
# - MySQL
# - PostgreSQL
#
# See GitHub readme for more and updated information.
Type = "sqlite"
DatabaseName = "MapChooserSharp.db"
Address = ""
Port = ""
User = ""
Password = ""

GroupInformationTableName = "McsGroupInformation"
MapInformationTableName = "McsMapInformation"
```

### Type

Specify the database type here.

### DatabaseName

Name of database, if database type is SQLite then this name will be a file name.

### Address, Port, User, Password

These will be necessary for connecting to the database when MySQL and PostgreSQL are supported in the future.

### GroupInformationTableName

You can specify the table name for group information.

### MapInformationTableName

You can specify the table name for map information.

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

You can specify the default maximum number of extensions when playing a map that doesn't exist in the config.

### FallbackMaxExtCommandUses

You can specify the default maximum number of extensions by `!ext` command when playing a map that doesn't exist in the config.

### FallbackExtendTimePerExtends

You can specify the default extension time for `mp_timelimit` or `mp_roundtime` in minutes when playing a map that doesn't exist in the config.

#### FallbackExtendRoundsPerExtends

You can specify the default extension for `mp_maxrounds` in rounds when playing a map that doesn't exist in the config.

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

You can specify the menu type to use for voting.

Only `BuiltInHtml` can be used without additional plugins. By adding the following plugins, you can use other menu types.

Also, if an invalid or incorrect value is entered, it will fall back to `BuiltInHtml`.

- Using Cs2ScreenMenuApi requires [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI)
- Using Cs2MenuManagerScreen requires [CS2MenuManager](https://github.com/schwarper/CS2MenuManager)

### MaxVoteElements

Specifies the number of maps that appear in one vote. In reality, because `Extend` and `Don't change` are included, the number of maps becomes MaxVoteElements - 1.

### ShouldPrintVoteToChat

You can specify whether to display the vote destination in chat when a player votes.

### ShouldPrintVoteRemainingTime

You can specify wheter to display the vote remaining time while voting.

### CountdownUiType

You can specify how to display the countdown before voting begins.

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

Specify the file path to `.vsndevts` for precaching. Example: `soundevents/soundevents_mapchooser.vsndevts`

If the file is already precached by other plugins, etc., you can leave it blank.

### Initial Vote

### InitialVoteCountdownStartSound

You can specify the sound to play when the first vote countdown starts.

### InitialVoteStartSound

You can specify the sound to play when the first vote starts.

### InitialVoteFinishSound

You can specify the sound to play when a vote is passed.

### InitialVoteCountdownSoundXX

You can specify the sound to play at the number of seconds corresponding to the number.

### Runoff Vote

### RunoffVoteCountdownStartSound

You can specify the sound to play when the runoff vote countdown starts.

### RunoffVoteStartSound

You can specify the sound to play when the runoff vote starts.

### RunoffVoteFinishSound

You can specify the sound to play when a vote is passed.

### RunoffVoteCountdownSoundXX

You can specify the sound to play when the runoff vote countdown starts.

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
You can specify the menu type to use for nomination.

Only `BuiltInHtml` can be used without additional plugins. By adding the following plugins, you can use other menu types.

Also, if an invalid or incorrect value is entered, it will fall back to `BuiltInHtml`.

- Using Cs2ScreenMenuApi requires [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI)
- Using Cs2MenuManagerScreen requires [CS2MenuManager](https://github.com/schwarper/CS2MenuManager)

## ConfigInformation

```toml
[ConfigInformation]
Version = "0.0.1"
```

### Version

This is the version of the config. It will warn you in the console when there is a version change, so please do not change it except when updating.