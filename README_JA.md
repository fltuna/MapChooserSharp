# [MapChooserSharp](https://github.com/fltuna/MapChooserSharp)

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/fltuna/MapChooserSharp) ![NuGet Version](https://img.shields.io/nuget/v/MapChooserSharp.API)

CounterStrikeSharpで実装された強力なAPIとカスタマイズ性を備えたMapChooserプラグインです。 

## Translated readme is available

[English](README.md)

## 留意事項

このプラグインはZombieEscapeやMinigame等の`mp_timelimit`ベースのゲームモードを主軸として開発しています。

このプラグインは、SurfやTTTのような`mp_maxrounds` と `mp_roundtime`のゲームモードもサポートしていますが、動作の保証はありません。

## Special Thanks

- [Lupercalia Server](https://steamcommunity.com/groups/lupercalia) (ZombieEscape/Minigame) - 64人等、実際のサーバー環境でのテスト

- [Spitice](https://github.com/spitice) - 投票コントローラーの一部コード提供
- [Uru](https://github.com/2vg) - バグの発見

## 機能

### 全般

- 完全な翻訳のサポート (CS2の投票UIを除く)
- ワークショップコレクションからマップを自動的に取得し、マップ設定を作成する機能
- マップスタート時にワークショップマップの名前を自動的に修正する機能

### マップ投票

- 基本的な投票機能
- !revote での再投票
- 管理者コマンド `!cancelvote` での投票のキャンセル
- 複数のUIサポート
   - 現在は次をサポートしてます: BuiltInHtml
- カウントダウン開始、カウントダウン、投票開始、投票完了での音声の再生

### ノミネート

- 名前の部分マッチ機能を含んだマップのノミネート
- コンフィグによるたくさんのノミネーションの制御設定
- 見やすい `!nomlist` を使用したノミネーションリスト
- 管理者コマンド `!nominate_addmap <MapName>`, `!nominate_removemap <MapName>` での、ノミネートへのマップの追加、削除のサポート
- 複数のUIサポート
   - 現在は次をサポートしてます: BuiltInHtml

### RTV

- `!rtv` コマンドと `rtv` チャットトリガー
- `!forcertv` を使用した強制的なRTVの開始機能
- `!enablertv` と `!disablertv` でのRTV有効無効の切り替え

### マップサイクル

- `!nextmap` コマンドと `nextmap` チャットトリガー
- `!currentmap` コマンドと `currentmap` チャットトリガー
- `!timeleft` コマンドと `timeleft` チャットトリガー
- `!mapinfo` コマンドでのマップ情報の表示
- `!extends` コマンドでの残り延長回数の表示
- ユーザーによる `!ext` コマンドを使用した延長投票
   - `!ext` を管理者コマンド `!setext <count>` を使用して変更できる機能
   - `!ext` を管理者コマンド `!enableext` と `!disableext` を使用して有効無効の切り替えが行える機能
- 管理者コマンド `!extend` と `!voteextend`, `!ve`(CS2の投票UI) を使用したマップの延長
- 管理者コマンド `!setnextmap <MapName>` と `!removenextmap` によるマップサイクルの管理
- マップの時間タイプの自動検出 (下にあるのでその情報を確認してください)
- データベースによるグループとマップのクールダウンの管理
   - 次のDBを現在サポートしてます: SQLite, MySQL, PostgreSQL
- 管理コマンド `!setmapcooldown` と `!setgroupcooldown` を使用したクールダウンの管理

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

これらの依存関係は`MapChooserSharp-osname-with-dependencies.zip`に含まれているため手動でDLする必要はありません。

- [TNCSSPluginFoundation](https://github.com/fltuna/TNCSSPluginFoundation/releases/latest)
- [NativeVoteAPI](https://github.com/fltuna/NativeVoteAPI-CS2/releases/latest)

### 依存関係 (オプション)

現在は特にありません。

### インストール

1. [リリースページ](https://github.com/fltuna/MapChooserSharp/releases/latest) にアクセスします。 また、最新の開発バージョンは [GitHub Actionsのアーティファクト](https://github.com/fltuna/MapChooserSharp/actions) からダウンロードできます。(GitHubアカウントが必要です)
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