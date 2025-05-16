# Command List


## For Users


| Command                | Description                                 | note                                                                                                                           |
|------------------------|---------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------|
| css_timeleft           | Show timeleft                               | You can get same result for typing `timeleft` in chat                                                                          |
| css_nextmap            | Show next map                               | You can get same result for typing `nextmap` in chat                                                                           |
| css_currentmap         | Show current map                            | You can get same result for typing `currentmap` in chat                                                                        |
| css_mapinfo            | Show current map's information if available |                                                                                                                                |
| css_extends            | Shows remaining extends                     |                                                                                                                                |
| css_revote             | Revote command                              |                                                                                                                                |
| css_nominate [MapName] | Nominate a map                              | You can get same result for typing `nominate` in chat. Also, if you don't specify the map name, then nomination menu will open |
| css_nomlist            | Shows nomination list                       | Admins can see who nominated a map using `full` argument e.g. `!nomlist full`                                                  |
| css_rtv                | Rock The Vote                               | You can get same result for typing `rtv` in chat                                                                               |
| css_ext                | Extend a current map by vote                |                                                                                                                                |

## For Admins


| Command                                                | Description                       | note                                                                                        |
|--------------------------------------------------------|-----------------------------------|---------------------------------------------------------------------------------------------|
| css_setnextmap <MapName>                               | Set next map                      |                                                                                             |
| css_removenextmap                                      | Remove next map                   |                                                                                             |
| css_setmapcooldown <MapName> <Cooldown>                | Set specified map's cooldown      |                                                                                             |
| css_setgroupcooldown <GroupName> <Cooldown>            | Set specified group's cooldown    |                                                                                             |
| css_cancelvote                                         | Cancel the current vote           |                                                                                             |
| css_nominate_addmap [MapName]                          | Insert a map to nomination        | If you don't specify the map name, then nomination menu will open.                          |
| css_nominate_removemap [MapName]                       | Remove a map from nomination      | If you don't specify the map name, and nominated map exists, then nomination menu will open |
| css_enablertv                                          | Enable RTV                        |                                                                                             |
| css_disablertv                                         | Disable RTV                       |                                                                                             |
| css_forcertv                                           | Force RTV                         |                                                                                             |
| css_extend <time>                                      | Extends a current map             | This extend command follows map time type.                                                  |
| css_ve <minutes/round>/ css_voteextend <minutes/round> | Starts a extend vote              | This extend command follows map time type.                                                  |
| css_setext <count>                                     | Set remaining `!ext` command uses |                                                                                             |
| css_enableext                                          | Enable `!ext` command use         |                                                                                             |
| css_disableext                                         | Disable `!ext` command use        |                                                                                             |