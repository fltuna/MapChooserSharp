# ConVarのカスタマイズ

ConVarのコンフィグファイルは `game/csgo/cfg/MapChooserSharp/convars.cfg` にあります。

## DebugLogger

| ConVar                                | Description                                                                             | Default Value | note |
|---------------------------------------|-----------------------------------------------------------------------------------------|---------------|------|
| mcs_debug_level                       | 0: 無し, 1: info, warn, error メッセージを表示, 2: 前のやつと debug メッセージを表示, 3: 前のやつと trace メッセージを表示  | 0             |      |
| mcs_debug_show_console                | デバッグメッセージをクライアントコンソールにも表示するか否か                                                          | false         |      |
| mcs_debug_console_print_required_flag | クライアントコンソールに表示するために必要な権限                                                                | css/generic   |      |


## Map Cycle Controller

| ConVar                       | Description                                            | Default Value | note                    |
|------------------------------|--------------------------------------------------------|---------------|-------------------------|
| mcs_vote_start_timing_time   | mp_timelimit か mp_roundtime ベースのマップでいつごろ投票を始めるか (秒で指定) | 180           | 有効な範囲: 0 - 600          |
| mcs_vote_start_timing_round  | mp_maxrounds ベースのマップでいつごろ投票を始めるか (ラウンド数で指定)            | 2             | 有効な範囲: 2 - 15           |

## Map Cycle Extend Controller

| ConVar                       | Description           | Default Value | note             |
|------------------------------|-----------------------|---------------|------------------|
| mcs_ext_user_vote_threshold  | ユーザーによる延長を行うために必要な投票率 | 0.5           | 有効な範囲: 0.0 - 1.0 |

## Map Cycle Extend Vote Controller

| ConVar                            | Description         | Default Value | note               |
|-----------------------------------|---------------------|---------------|--------------------|
| mcs_vote_extend_success_threshold | 投票による延長を行うために必要な投票率 | 0.5           | 有効な範囲: 0.0 - 1.0   |
| mcs_vote_extend_vote_time         | 投票終了まで何秒待つか         | 15.0          | 有効な範囲: 10.0 - 60.0 |

## Vote Controller

| ConVar                                           | Description                                                              | Default Value | note                       |
|--------------------------------------------------|--------------------------------------------------------------------------|---------------|----------------------------|
| mcs_vote_shuffle_menu                            | メニューの内容をプレイヤーごとにシャッフルするか否か                                               | false         |                            |
| mcs_vote_end_time                                | 投票終了まで何秒待つか                                                              | 15.0          | 有効な範囲: 5.0 - 120.0 seconds |
| mcs_vote_countdown_time                          | 投票開始まで何秒カウントダウンするか                                                       | 13            | 有効な範囲: 0 - 120 seconds     |
| mcs_vote_runoff_map_pickup_threshold             | もし、mcs_vote_winner_pickup_thresholdを超えるマップがなかった場合、最終投票を開始する際にピック対象になる投票率 | 0.3           | 有効な範囲: 0.0 - 1.0           |
| mcs_vote_winner_pickup_threshold                 | もし、このパーセントより高い投票を得たマップがある場合そのまま投票を終了します                                  | 0.7           | 有効な範囲: 0.0 - 1.0           |
| mcs_vote_exclude_spectators                      | 観戦者を投票参加者から除外するか                                                         | false         |                            |
| mcs_vote_change_map_immediately_rtv_vote_success | 有効の場合、RTVで次のマップが決まった際即座にマップを変更します                                        | false         |                            |

## Nomination Command

| ConVar                                     | Description      | Default Value | note                 |
|--------------------------------------------|------------------|---------------|----------------------|
| mcs_nomination_command_cooldown            | ノミネートコマンドのクールダウン | 10.0          | Specify with seconds |
| mcs_nomination_command_prevent_spectators  | 観戦者のノミネートを禁止するか  | false         |                      |

## Nomination Controller

| ConVar                          | Description                                | Default Value | note |
|---------------------------------|--------------------------------------------|---------------|------|
| mcs_nomination_per_group_limit  | 同一グループに所属しているマップの同時zノミネート可能な数を制限します。 0で無効化 | 0             |      |

## RTV Controller

| ConVar                                         | Description                                    | Default Value | note                |
|------------------------------------------------|------------------------------------------------|---------------|---------------------|
| mcs_rtv_command_unlock_time_next_map_confirmed | 次のマップが決まってからRTV出来るようになるまでの秒数                   | 60.0          | 有効な範囲: 0.0 - 1200.0 |
| mcs_rtv_command_unlock_time_map_dont_change    | RTVでマップを変更しないと決まってからRTV出来るようになるまでの秒数           | 240.0         | 有効な範囲: 0.0 - 1200.0 |
| mcs_rtv_command_unlock_time_map_extend         | マップの延長が決まってからRTV出来るようになるまでの秒数                  | 120.0         | 有効な範囲: 0.0 - 1200.0 |
| mcs_rtv_command_unlock_time_map_start          | マップがスタートしてからRTV出来るようになるまでの秒数                   | 300.0         | 有効な範囲: 0.0 - 1200.0 |
| mcs_rtv_vote_start_threshold                   | RTV投票を始めるために必要な投票率                             | 0.5           | 有効な範囲: 0.0 - 1.0    |
| mcs_rtv_map_change_timing                      | 次のマップが確定している際にRTVが可決された場合にマップを変更するまでの時間 (秒で指定) | 3.0           | 有効な範囲: 0.0 - 60.0   |
| mcs_rtv_minimum_requirements                   | RTVを行うために最低限必要な人数を指定します。 0を指定すると無効化出来ます。       | 0             | 有効な範囲: 0 - 64       |


## Timeleft Util

| ConVar                      | Description                                                                   | Default Value | note                                                              |
|-----------------------------|-------------------------------------------------------------------------------|---------------|-------------------------------------------------------------------|
| mcs_map_time_type_override  | マップタイプを上書きします. 0 = 自動検出, 1 = mp_timelimit, 2 = mp_maxrounds, 3 = mp_roundtime | 0             | オーバーライドを使用する際は対応するCVarを正しく設定する必要があります。正しく設定されていない場合、プラグインは動作しません。 |
