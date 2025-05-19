# Configurating ConVar

ConVar configuration file is located in `game/csgo/cfg/MapChooserSharp/convars.cfg`

## DebugLogger

| ConVar                                | Description                                                                                                                      | Default Value | note |
|---------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|---------------|------|
| mcs_debug_level                       | 0: Nothing, 1: Print info, warn, error message, 2: Print previous one and debug message, 3: Print previous one and trace message | 0             |      |
| mcs_debug_show_console                | Debug message shown in client console?                                                                                           | false         |      |
| mcs_debug_console_print_required_flag | Required flag for print to client console                                                                                        | css/generic   |      |


## Map Cycle Controller

| ConVar                       | Description                                                                         | Default Value | note                   |
|------------------------------|-------------------------------------------------------------------------------------|---------------|------------------------|
| mcs_vote_start_timing_time   | When should vote started if map is based on mp_timelimit or mp_roundtime? (seconds) | 180           | Valid Range is 0 - 600 |
| mcs_vote_start_timing_round  | When should vote started if map is based on mp_maxrounds? (rounds)                  | 2             | Valid Range is 2 - 15  |

## Map Cycle Extend Controller

| ConVar                       | Description                                         | Default Value | note                      |
|------------------------------|-----------------------------------------------------|---------------|---------------------------|
| mcs_ext_user_vote_threshold  | How many percent to require extend a map by users?  | 0.5           | Valid Range is 0.0 - 1.0  |

## Map Cycle Extend Vote Controller

| ConVar                            | Description                                        | Default Value | note                       |
|-----------------------------------|----------------------------------------------------|---------------|----------------------------|
| mcs_vote_extend_success_threshold | How many percent to require extend a map by votes? | 0.5           | Valid Range is 0.0 - 1.0   |
| mcs_vote_extend_vote_time         | How many seconds to wait vote ends                 | 15.0          | Valid Range is 10.0 - 60.0 |

## Vote Controller

| ConVar                               | Description                                                                                                                                  | Default Value | note                               |
|--------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|---------------|------------------------------------|
| mcs_vote_shuffle_menu                | Should vote menu elements is shuffled per player?                                                                                            | false         |                                    |
| mcs_vote_end_time                    | How long to take vote ends in seconds?                                                                                                       | 15.0          | Valid Range is 5.0 - 120.0 seconds |
| mcs_vote_countdown_time              | How long to take vote starts in seconds                                                                                                      | 13            | Valid Range is 0 - 120 seconds     |
| mcs_vote_runoff_map_pickup_threshold | If there is no vote that higher than mcs_vote_winner_pickup_threshold, then it will pick up maps higher than this percentage for runoff vote | 0.3           | Valid Range is 0.0 - 1.0           |
| mcs_vote_winner_pickup_threshold     | If vote is higher than this percent, it will picked up as winner.                                                                            | 0.7           | Valid Range is 0.0 - 1.0           |
| mcs_vote_exclude_spectators          | Should exclude spectators from vote                                                                                                          | false         |                                    |

## Nomination Command

| ConVar                                     | Description                     | Default Value | note                 |
|--------------------------------------------|---------------------------------|---------------|----------------------|
| mcs_nomination_command_cooldown            | Cooldown for nomination command | 10.0          | Specify with seconds |
| mcs_nomination_command_prevent_spectators  | Prevent spectators nomination   | false         |                      |

## Nomination Controller

| ConVar                          | Description                                                                                              | Default Value | note |
|---------------------------------|----------------------------------------------------------------------------------------------------------|---------------|------|
| mcs_nomination_per_group_limit  | Maximum number of maps that can be nominated from the same group. Set to 0 to disable group limitations  | 0             |      |

## RTV Controller

| ConVar                                         | Description                                                                         | Default Value | note                        |
|------------------------------------------------|-------------------------------------------------------------------------------------|---------------|-----------------------------|
| mcs_rtv_command_unlock_time_next_map_confirmed | Seconds to take unlock RTV command after next map confirmed in vote                 | 60.0          | Valid Range is 0.0 - 1200.0 |
| mcs_rtv_command_unlock_time_map_dont_change    | Seconds to take unlock RTV command after map is not changed in rtv vote             | 240.0         | Valid Range is 0.0 - 1200.0 |
| mcs_rtv_command_unlock_time_map_extend         | Seconds to take unlock RTV command after map is extended in vote                    | 120.0         | Valid Range is 0.0 - 1200.0 |
| mcs_rtv_command_unlock_time_map_start          | Seconds to take unlock RTV command after map started                                | 300.0         | Valid Range is 0.0 - 1200.0 |
| mcs_rtv_vote_start_threshold                   | How many percent to require start rtv vote?                                         | 0.5           | Valid Range is 0.0 - 1.0    |
| mcs_rtv_map_change_timing                      | Seconds to change map after RTV is success. Set 0.0 to change immediately           | 3.0           | Valid Range is 0.0 - 60.0   |


## Timeleft Util

| ConVar                      | Description                                                                                            | Default Value | note                                                                                           |
|-----------------------------|--------------------------------------------------------------------------------------------------------|---------------|------------------------------------------------------------------------------------------------|
| mcs_map_time_type_override  | Override map time type. 0 = automatic detection, 1 = mp_timelimit, 2 = mp_maxrounds, 3 = mp_roundtime  | 0             | You should set correspond CVars correctly when using override, otherwise plugin will not work. |
