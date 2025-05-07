# Map Config Customization

Translation and translation assist by Claude.ai

## How to Place Map Configs

This plugin supports the following two methods:

### 1. When maps.toml exists

If `config/maps.toml` exists, settings will be loaded from `maps.toml`.

```
.
└── MapChooserSharp/
    ├── config/
    │   └── maps.toml
    └── MapChooserSharp.dll
```

### 2. When xxx.toml exists

Instead of consolidating into a single file, you can separate configs as follows.
If you choose this method, please do not use `maps.toml` as a filename.

Also, One of file must be placed on root of `config/` folder.

```
.
└── MapChooserSharp/
    ├── config/
    │   ├── ze_maps/
    │   │   └── ze_xxxxx_v1.toml
    │   ├── de_maps/
    │   │   ├── de_dust2.toml
    │   │   └── de_mirage.toml
    │   ├── surf_maps/
    │   │   ├── surf_xxxxx.toml
    │   │   └── surf_yyyyy.toml
    │   └── default.toml
    └── MapChooserSharp.dll
```

## Configuration Details

```
[ze_example_abc]
MapNameAlias = "ze example a b c"
MapDescription = "This map contains a jump scare"
IsDisabled = false
WorkshopId = 1234567891234
OnlyNomination = false
Cooldown = 60
MaxExtends = 3
ExtendTimePerExtends = 15
MapTime = 20
ExtendRoundsPerExtends = 5
MapRounds = 10
RequiredPermissions = ["css/generic"]
RestrictToAllowedUsersOnly = false
AllowedSteamIds = [76561198815323852]
DisallowedSteamIds = [76561198815323852]
MaxPlayers = 64
MinPlayers = 10
ProhibitAdminNomination = false
DaysAllowed = ["wednesday", "monday"]
AllowedTimeRanges = ["10:00-12:00", "20:00-22:00", "22:00-03:00"]

[ze_example_abc.extra.shop]
cost = 100
```

## General Map Settings

### MapNameAlias

You can change the name displayed on the voting screen, etc.

### MapDescription

You can specify the content displayed with the `!mapinfo` command here.

### IsDisabled

Specifies whether the map is enabled or not.
If disabled here, the map cannot be nominated and will not be selected in random map voting.

### WorkshopId

For workshop maps, specifying this value eliminates the need to edit the config when the map name changes.

### OnlyNomination

Specifies whether the map is nomination-only.
If enabled here, the map will not be selected in random map voting.

### Cooldown

Specifies the cooldown applied after a map is played.
Please note this is separate from group cooldowns.

### MaxExtends

Specifies the maximum number of times a map can be extended.

### ExtendTimePerExtends

For time-based maps, specifies how many minutes to extend each time an extension is granted.

### MapTime

Currently not in use.

For time-based maps, specifies the duration of the map.

### ExtendRoundsPerExtends

For round-based maps, specifies how many rounds to extend each time an extension is granted.

### MapRounds

Currently not in use.

For round-based maps, specifies the number of rounds for the map.

## Nomination Settings

### RequiredPermissions

Specifies the permissions required to nominate.
If multiple permissions are specified, having any one of them will allow nomination.

### RestrictToAllowedUsersOnly

When enabled, restricts nomination to only users included in AllowedSteamIds.

### AllowedSteamIds

Users in this list can nominate, bypassing all permission settings.
Add Steam ID 64 to include users.

### DisallowedSteamIds

Users in this list cannot nominate under any circumstances.

### MaxPlayers

Nomination becomes impossible if the number of players on the server exceeds this value.

### MinPlayers

Nomination becomes impossible if the number of players on the server is less than this value.

### ProhibitAdminNomination

Prohibits nomination via `!nominate_addmap`.

### DaysAllowed

Allows nomination only on the days listed here. Also, please be careful as this applies simultaneously with AllowedTimeRanges.

`DaysAllowed = ["wednesday", "monday"]` allows nomination only on Wednesdays and Mondays.

### AllowedTimeRanges

Allows nomination only during the time ranges listed here. Also, please be careful as this applies simultaneously with DaysAllowed.

`AllowedTimeRanges = ["10:00-12:00", "22:00-03:00"]` allows nomination only between 10:00-12:00 or 22:00-03:00.


### Extra configuration

See [MCS API Document](../development/USING_MCS_API.md)