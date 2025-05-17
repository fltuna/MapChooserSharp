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

## About Settings Application Order and Groups

### Map Priority

The priority of map settings is `Map > Group > Default`, meaning map settings have the highest priority.

If, for example, you set `MinPlayers = 0` as the default, set `MinPlayers = 16` in a group, and then set `MinPlayers = 32` in the map settings while inheriting from this group, the final value of `MinPlayers` will be `32`.

### Map Settings Application Order

Here we'll explain using `ze_example_xyz` as an example map config.

### 1. Default Values

First, the map config gets the setting values from `[MapChooserSharpSettings.Default]`.

If we were to write the config at this point into a file...

---

If the default file is like this:

```toml
[MapChooserSharpSettings.Default]
MinPlayers = 0
MaxPlayers = 0
OnlyNomination = false
```

It would be as follows:

```toml
[ze_example_xyz]
MinPlayers = 0
MaxPlayers = 0
OnlyNomination = false
```

### 2. Group Settings

Next, MapChooserSharp can apply group settings to maps.

Groups can be defined with `MapChooserSharpSettings.Groups.<GroupName>`.
```toml
[MapChooserSharpSettings.Groups.Group1]
MinPlayers = 16
OnlyNomination = true
```

And the `Group1` part after `MapChooserSharpSettings.Groups` becomes the group name.

```toml
[ze_example_xyz]
GroupSettings = ["Group1"]
```

### How to Set Groups

Maps can have multiple groups, but the settings of the first group have the highest priority.

Groups can be used as follows:

```toml
[ze_example_789]
GroupSettings = ["Group1", "Group2", "Group3"]
```

In this case, Group1 has the highest priority among Group settings, so it overwrites values applied by Group2 or Group3.

Group settings have higher priority than Default settings, so `MinPlayers` changes from the default `0` to `16`.

Also, `OnlyNomination` changes from `false` to `true`.

At this point, if we recreate the data loaded into the plugin as a config, it would be as follows:

```toml
[ze_example_xyz]
MinPlayers = 16
MaxPlayers = 0
OnlyNomination = true
```

### 3. Map Settings

Next, the plugin reads the map's config data.

Let's say there's a definition like this:

```toml
[ze_example_xyz]
MinPlayers = 32
```

Then, as mentioned earlier, map settings have the highest priority, so if we recreate the data finally loaded into the plugin as a config, it would be as follows:

```toml
[ze_example_xyz]
MinPlayers = 32
MaxPlayers = 0
OnlyNomination = true
```

### 4. Some Exceptions

The following values are integrated rather than overwritten:
- AllowedSteamIds
- DisallowedSteamIds
- Extra settings

### 4-2. About Cooldown

Cooldown settings are handled differently for maps and groups, and are applied separately.

For example, let's say that `Group1` has a cooldown of `15`, and `ze_example_xyz` has a cooldown of `20`.

And let's assume that `ze_example_xyz` and `ze_example_abc` belong to the same group.

```toml
[ze_example_xyz]
Cooldown = 20
GroupSettings = ["Group1"]

[ze_example_abc]
Cooldown = 0
GroupSettings = ["Group1"]

[MapChooserSharpSettings.Groups.Group1]
Cooldown = 15
```

In this case, when `ze_example_xyz` is played, a cooldown of `15` is applied to the group, and a cooldown of `20` is applied to the map separately.

And this group cooldown is also applied to other maps belonging to the same group. In this case, `ze_example_abc` is also affected by the group cooldown, which effectively gives it a cooldown of `15`.

---

For example, let's say there are configs like these:

Group Setting 1

```toml
[MapChooserSharpSettings.Groups.Group1]
MinPlayers = 0
AllowedSteamIds = [123012301230]

[MapChooserSharpSettings.Groups.Group1.extra.shop]
cost = 100
```
Group Setting 2

```toml
[MapChooserSharpSettings.Groups.Group2]
MinPlayers = 32
AllowedSteamIds = [123456789]
DisallowedSteamIds = [987654321]

[MapChooserSharpSettings.Groups.Group2.extra.AnotherShop]
cost = 999
```

Map Setting

```toml
[ze_example_xyz]
MinPlayers = 40
AllowedSteamIds = [0]
DisallowedSteamIds = [0]
GroupSettings = ["Group1", "Group2"]

[ze_example_xyz.extra.ExternalShop]
cost = 10000
```

These will ultimately look like this:

```toml
[ze_example_xyz]
MinPlayers = 40
AllowedSteamIds = [0, 123012301230, 123456789]
DisallowedSteamIds = [0, 987654321]

[ze_example_xyz.extra.ExternalShop]
cost = 10000

[ze_example_xyz.extra.AnotherShop]
cost = 999

[ze_example_xyz.extra.extra.shop]
cost = 100
```

This is how MapChooserSharp achieves config flexibility.

---

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
MaxExtCommandUses = 1
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

If this key is blank, It will use `ds_workshop_changelevel <keyName>` for workshop map or `changelevel <keyName>` for official map.

We will use top key name for map name. e.g. when key is `[ze_example_abc]` then `ds_workshop_changelevel ze_example_abc`.

### OnlyNomination

Specifies whether the map is nomination-only.
If enabled here, the map will not be selected in random map voting.

### Cooldown

Specifies the cooldown applied after a map is played.
Please note this is separate from group cooldowns.

### MaxExtends

Specifies the maximum number of times a map can be extended.

### MaxExtCommandUses

Specifies the maximum number of times a map can be extended by `!ext` command.

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

Server Console can bypasses this limit.

### DaysAllowed

Allows nomination only on the days listed here. Also, please be careful as this applies simultaneously with AllowedTimeRanges.

`DaysAllowed = ["wednesday", "monday"]` allows nomination only on Wednesdays and Mondays.

### AllowedTimeRanges

Allows nomination only during the time ranges listed here. Also, please be careful as this applies simultaneously with DaysAllowed.

`AllowedTimeRanges = ["10:00-12:00", "22:00-03:00"]` allows nomination only between 10:00-12:00 or 22:00-03:00.


### Extra configuration

See [MCS API Document](../development/USING_MCS_API.md)