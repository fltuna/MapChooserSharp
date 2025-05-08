# [MapChooserSharp](https://github.com/fltuna/MapChooserSharp)

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/fltuna/MapChooserSharp) ![NuGet Version](https://img.shields.io/nuget/v/MapChooserSharp.API)

CounterStrikeSharpで実装された強力なAPIとカスタマイズ性を備えたMapChooserプラグインです。 

## Translated readme is available

[English](README.md)


## 機能

### 豊富なカスタマイズ項目

ConVarに関しては [ConVarドキュメント](docs/ja/configuration/CONVAR.md) を参照してください。

Map Configについては [Configドキュメント](docs/ja/configuration/MAP_CONFIG.md) を参照してください。

プラグインの設定については [プラグイン設定ドキュメント](docs/ja/configuration/PLUGIN_CONFIG.md) を参照してください。

### 使いやすいコマンド

詳細は [コマンドドキュメント](docs/ja/COMMANDS.md) を参照してください。

### マップの時間タイプの自動検出

`mp_timelimit`, `mp_maxrounds`, `mp_roundtime`の3個のCVarを確認して自動的に最適なものを選択します。

確認順は `mp_maxrounds` -> `mp_timelimit` -> `mp_roundtime` でどれかのCVarが0でなかった場合にそのCVarを元としてマップの時間タイプを検出します。

また、surfサーバーのような`mp_roundtime`ベースの場合は、延長された場合にラウンドの時間がそのまま延長されます。

### 強力なAPI

詳細は [MCS API ドキュメント](docs/ja/development/USING_MCS_API.md) を参照してください。

## 既知の問題

- Countdown HUDでCenterHTMLを使用するとUIが表示されない

## インストール

### 必要な依存関係

- [TNCSSPluginFoundation](https://github.com/fltuna/TNCSSPluginFoundation/releases/latest)

### 依存関係 (オプション)

これらの依存関係は必須ではありませんが、Screen Menuをサポートするにはどちらか、もしくは両方をインストールする必要があります。

このプラグインは現在2個のScreen Menu APIをサポートしています。

両方をインストールする必要はありませんが、Screen Menuを使用するにはどちらか一つをインストールする必要があります。

- [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI)
- [CS2MenuManager](https://github.com/schwarper/CS2MenuManager)

### インストール

1. [リリースページ](https://github.com/fltuna/MapChooserSharp/releases/latest) にアクセスします
2. 状況に合わせて最適なZIPファイルをダウンロードします
    1. 最初のインストールか、リリースページで再ダウンロードを促されている場合は `MapChooserSharp-osname-with-dependencies.zip` をダウンロードしてください
    2. アップデートだけであれば `MapChooserSharp-osname.zip` をダウンロードしてください
3. ZIPファイルを解凍
4. でてきたフォルダを `game/csgo/addons/counterstrikesharp/` に入れる
5. サーバーを起動
6. 完了!

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