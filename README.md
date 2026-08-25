# Player Punishments

A [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) plugin that adds server-console/RCON commands for checking player state, slaying, and slapping players. It is designed for [ShAgGy's Ultimate CS2 RCON/Server Tool](https://github.com/ShAgGy2035/Ultimate-RCON-Server-Tool) and does not require a separate admin plugin.

Version: 1.0.0  
Author: ShAgGy

## Download

Download the latest installation ZIP from [GitHub Releases](https://github.com/ShAgGy2035/PlayerPunishments/releases). Compiled binaries are not distributed from the repository's source file listing.

## Requirements

- Counter-Strike 2 dedicated server
- Metamod:Source
- CounterStrikeSharp with .NET 10 support
- CounterStrikeSharp API `1.0.372`

## Commands

Commands are server-console/RCON only. Players cannot execute them from chat or their client console.

| Command | Description |
| --- | --- |
| `css_alive [#userid\|steamid64\|exact name]` | Reports the state and health of one player, or all connected players when no target is supplied. |
| `css_slay <#userid\|steamid64\|exact name>` | Kills a living player. |
| `css_slap <#userid\|steamid64\|exact name> [damage]` | Damages and launches a living player. Damage defaults to `5` and must be from `0` to `100`. |

Target names are matched case-insensitively but must otherwise be exact; wrap names containing spaces in quotes. SteamID and name lookup target human players, while `#userid` can also resolve a bot. Prefer `#userid` from the server's `status` output because names may be ambiguous.

Examples:

```text
css_alive
css_alive #12
css_alive "Player Name"
css_slay #12
css_slap #12
css_slap #12 10
```

`css_alive` includes bots when listing all connected players. When a target is supplied, it reports only the selected player.

Every slap applies randomized horizontal and upward velocity, including a `0`-damage slap. Lethal slap damage slays the player.

## Install

Download the installation ZIP from [GitHub Releases](https://github.com/ShAgGy2035/PlayerPunishments/releases), then extract it into `game/csgo/addons/counterstrikesharp/plugins`. The final DLL path should be:

```text
game/csgo/addons/counterstrikesharp/plugins/PlayerPunishments/PlayerPunishments.dll
```

Restart the CS2 server or load the plugin through CounterStrikeSharp. Verify it with:

```text
css_plugins list
```

Test from the server console or RCON:

```text
status
css_alive
css_alive #<userid>
css_slap #<userid> 5
css_slay #<userid>
```

## RCON Tool Integration

The matching RCON tool buttons send:

```text
css_slap #<userid>
css_slay #<userid>
```

The plugin writes success and failure messages back to the server console, so they are visible in RCON output and logs.