# [MapChooserSharp](https://github.com/fltuna/MapChooserSharp)

CounterStrikeSharp implmentation of map chooser plugin with powerful API and configuration.

## Translated readme is available

[日本語](README_JA.md)

## Installation

### Dependency

- [TNCSSPluginFoundation](https://github.com/fltuna/TNCSSPluginFoundation/releases/latest)

### Optional Dependency

These optinal dependency is not required, but you need to install them if you want to use the screen menu in UI.

Our plugin currently supports 2 plugins for screen menu.

You don't need to install both of them, but you need to install at least one of them for using screen menu.

- [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI) - To use screen menu
- [CS2MenuManager](https://github.com/schwarper/CS2MenuManager) - To use screen menu

### Installation

1. Go to [Latest Release Page](https://github.com/fltuna/MapChooserSharp/releases/latest)
2. Download zip files depends on your situation
   1. If you are first time, or release page says redownload, then donwload `MapChooserSharp-osname-with-dependencies.zip`
   2. Just updating, then download `MapChooserSharp-osname.zip`
3. Extract ZIP
4. Put folders into `game/csgo/addons/counterstrikesharp/`
5. Run the server
6. It's done

## Configuraiton

See [ConVar Document](docs/en/configuration/CONVAR.md) for ConVar configuration

See [Map Config Document](docs/en/configuration/MAP_CONFIG.md) for Map configuration

See [Plugin Config Document](docs/en/configuration/PLUGIN_CONFIG.md) for Plugin configuration

## Commands list

See [Commands Document](docs/en/COMMANDS.md)

## Using MCS API

See [MCS API document](docs/en/development/USING_MCS_API.md)

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