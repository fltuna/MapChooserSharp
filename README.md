# [MapChooserSharp](https://github.com/fltuna/MapChooserSharp)

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/fltuna/MapChooserSharp) ![NuGet Version](https://img.shields.io/nuget/v/MapChooserSharp.API)


CounterStrikeSharp implmentation of map chooser plugin with powerful API and configuration.

## Translated readme is available

[日本語](README_JA.md)

## NOTE

This plugin is mainly intended for `mp_timelimit` based gamemode, such as ZombieEscape, Minigame.

This plugin is supported `mp_maxrounds` and `mp_roundtime` based gamemode, such as surf, TTT, etc... But there is no gurantee to work properly.


## Special Thanks

- [Lupercalia Server](https://steamcommunity.com/groups/lupercalia) (ZombieEscape/Minigame) - for a lot of live testing.

- [Spitice](https://github.com/spitice) - for vote controller code contribution
- [Uru](https://github.com/2vg) - for hunting bugs

## Features

### General

- Full translation support (Except Native Vote UI)

### Map Vote

- Basic map voting
- !revote support
- Admin command to cancel vote `!cancelvote`
- Multiple UI support
   - Currently supports: BuiltInHtml, CS2ScreenMenuAPI, CS2MenuManager(Screen)
- Play sounds when countdown start, countdown, vote start, vote finish

### Nomination

- Map nomination with partial name match
- Much much nomination restriction settings with map config
- Very clear nomination list with `!nomlist`
- Admin map nomination and removal with `!nominate_addmap <MapName>`, `!nominate_removemap <MapName>`
- Multiple UI support
   - Currently supports: BuiltInHtml, CS2ScreenMenuAPI, CS2MenuManager(Screen)

### RTV

- !rtv and rtv chat trigger
- Force activate RTV with command `!forcertv`
- Toggle RTV by `!enablertv` and `!disablertv`

### Map Cycle

- `!nextmap` and `nextmap` chat trigger
- `!currentmap` and `currentmap` chat trigger
- `!timeleft` and `timeleft` chat trigger
- `!mapinfo` command to show map's information
- `!extends` command to check remaining extends
- User extend vote command with `!ext`
    - Admin command to manipulate `!ext` counts with `!setext <count>`
    - Admin command to toggle `!ext` command with `!enableext` and `!disableext`
- Admin extend command with `!extend` and `!voteextend`, `!ve` (with native vote UI)
- Admin map cycle manipulation with `!setnextmap <MapName>` and `!removenextmap`
- Automatic Detection of Map Time Type (See below for details)
- Database cooldown management for Map and Group
  - Databases currently supports: SQLite, MySQL, PostgreSQL
- Admin cooldown manipulation command `!setmapcooldown`, `!setgroupcooldown`

### Abundant Customization Options

See [ConVar Document](docs/en/configuration/CONVAR.md) for ConVar configuration

See [Map Config Document](docs/en/configuration/MAP_CONFIG.md) for Map configuration

See [Plugin Config Document](docs/en/configuration/PLUGIN_CONFIG.md) for Plugin configuration

### Automatic Detection of Map Time Type

It checks three CVars: `mp_timelimit`, `mp_maxrounds`, and `mp_roundtime`, and automatically selects the most appropriate one.

The check order is `mp_maxrounds` -> `mp_timelimit` -> `mp_roundtime`. If any of these CVars is non-zero, it will use that CVar as the basis for detecting the map time type.

Also, for `mp_roundtime`-based servers like surf servers, the round time is directly extended when an map extend occurs.

### Powerful API

See [MCS API document](docs/en/development/USING_MCS_API.md)

### A handy commands

See [Commands Document](docs/en/COMMANDS.md)

## Known Issues

- UI doesn't display when using CenterHTML for Countdown HUD

## Installation

### Dependency

These dependencies are already packed in to `MapChooserSharp-osname-with-dependencies.zip`, so you don't need to download them manually.

- [TNCSSPluginFoundation](https://github.com/fltuna/TNCSSPluginFoundation/releases/latest)
- [NativeVoteAPI](https://github.com/fltuna/NativeVoteAPI-CS2/releases/latest)

### Optional Dependency

These optinal dependency is not required, but you need to install them if you want to use the screen menu in UI.

Our plugin currently supports 2 plugins for screen menu.

You don't need to install both of them, but you need to install at least one of them for using screen menu.

- [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI) - To use screen menu (Version 3.0.4 Required)
- [CS2MenuManager](https://github.com/schwarper/CS2MenuManager) - To use screen menu (Version 1.0.32 Required)

### Installation

1. Go to [Latest Release Page](https://github.com/fltuna/MapChooserSharp/releases/latest) | Also, Latest development version is provided in [GitHub Actions Artifacts](https://github.com/fltuna/MapChooserSharp/actions) (Requires GitHub Account)
2. Download zip files depends on your situation
   1. If you are first time, or release page says redownload, then donwload `MapChooserSharp-osname-with-dependencies.zip`
   2. Just updating, then download `MapChooserSharp-osname.zip`
3. Extract ZIP
4. Put folders into `game/csgo/addons/counterstrikesharp/`
5. Run the server
6. It's done

## Reosuces

- [CounterStrikeSharp.API](https://github.com/roflmuffin/CounterStrikeSharp) - Foundation of plugin development environment for CS2.
- [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI) - Screen menu implementation
- [CS2MenuManager](https://github.com/schwarper/CS2MenuManager) - Screen menu implementation
- [TNCSSPluginFoundation](https://github.com/fltuna/TNCSSPluginFoundation) - Powerful plugin development base.
- [Dapper](https://github.com/DapperLib/Dapper) - Supporting general database operation
- [System.Data.SQLite.Core](https://www.nuget.org/packages/system.data.sqlite.core/) - Supporting Sqlite database
- [Npgsql](https://github.com/npgsql/npgsql) - Supporting PostgreSql database
- [MySqlConnector](https://github.com/mysql-net/MySqlConnector) - Supporting MySql database

### Developed with JetBrains Rider

<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider_icon.png" width="64" alt="JetBrains Rider IDE"/>

Copyright © 2025 JetBrains s.r.o. [Rider] and the [Rider] logo are trademarks of JetBrains s.r.o.