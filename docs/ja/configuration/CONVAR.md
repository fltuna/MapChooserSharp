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

## Vote Controller

| ConVar                               | Description                                                              | Default Value | note                                |
|--------------------------------------|--------------------------------------------------------------------------|---------------|-------------------------------------|
| mcs_vote_shuffle_menu                | メニューの内容をプレイヤーごとにシャッフルするか否か                                               | false         |                                     |
| mcs_vote_end_time                    | 投票終了まで何秒待つか                                                              | 15.0          | 有効な範囲: 5.0 - 120.0 seconds          |
| mcs_vote_countdown_time              | 投票開始まで何秒カウントダウンするか                                                       | 13            | 有効な範囲: 0 - 120 seconds              |
| mcs_vote_runoff_map_pickup_threshold | もし、mcs_vote_winner_pickup_thresholdを超えるマップがなかった場合、最終投票を開始する際にピック対象になる投票率 | 0.3           | 有効な範囲: 0.0 - 1.0                    |
| mcs_vote_winner_pickup_threshold     | もし、このパーセントより高い投票を得たマップがある場合そのまま投票を終了します                                  | 0.7           | 有効な範囲: 0.0 - 1.0                    |

## Nomination Command

| ConVar                           | Description      | Default Value  | note                 |
|----------------------------------|------------------|----------------|----------------------|
| mcs_nomination_command_cooldown  | ノミネートコマンドのクールダウン | 10.0           | Specify with seconds |

## RTV Controller

| ConVar                                         | Description                                                                | Default Value | note                         |
|------------------------------------------------|----------------------------------------------------------------------------|---------------|------------------------------|
| mcs_rtv_command_unlock_time_next_map_confirmed | 次のマップが決まってからRTV出来るようになるまでの秒数                                               | 60.0          | 有効な範囲: 0.0 - 1200.0          |
| mcs_rtv_command_unlock_time_map_dont_change    | RTVでマップを変更しないと決まってからRTV出来るようになるまでの秒数                                       | 240.0         | 有効な範囲: 0.0 - 1200.0          |
| mcs_rtv_command_unlock_time_map_extend         | マップの延長が決まってからRTV出来るようになるまでの秒数                                              | 120.0         | 有効な範囲: 0.0 - 1200.0          |
| mcs_rtv_command_unlock_time_map_start          | マップがスタートしてからRTV出来るようになるまでの秒数                                               | 300.0         | 有効な範囲: 0.0 - 1200.0          |
| mcs_rtv_vote_start_threshold                   | RTV投票を始めるために必要な投票率                                                         | 0.5           | 有効な範囲: 0.0 - 1.0             |
| mcs_rtv_map_change_timing                      | RTV完了後にマップを変更するまでの時間 (病で指定)                                                | 3.0           | 有効な範囲: 0.0 - 60.0            |
| mcs_rtv_map_change_timing_should_round_end     | RTV完了後のマップ変更をラウンド終了時にするか否か、 もしTrueの場合はmcs_rtv_map_change_timingの内容が無視されます。 | true          |                              |