# Player Punishments

CounterStrikeSharp plugin that adds server-console/RCON commands for checking player state, slaying, and slapping players. It is designed for ShAgGy's Ultimate CS2 RCON/Server Tool (https://github.com/ShAgGy2035/Ultimate-RCON-Server-Tool) and does not require a separate admin plugin.

Version: 1.0.0  
Author: ShAgGy

## Requirements

- Counter-Strike 2 dedicated server
- Metamod:Source
- CounterStrikeSharp with .NET 10 support
- CounterStrikeSharp API `1.0.372` or a compatible newer release

## Commands

Commands are server-console/RCON only. Players cannot execute them from chat or their client console.

```text
css_alive [#userid|steamid64|exact name]
css_slay <#userid|steamid64|exact name>
css_slap <#userid|steamid64|exact name> [damage]
```

Examples:

```text
css_alive
css_alive #12
css_slay #12
css_slap #12
css_slap #12 10
```

`css_alive` lists every connected player and reports `alive` or `dead` with current health. When a target is supplied, it reports only that player.

`css_slap` defaults to 5 damage when damage is omitted. Damage can be from 0 to 100. A slap also applies randomized horizontal and upward velocity. Lethal slap damage slays the player.

The CS2 RCON tool uses `#userid`, which is taken from the server's `status` output and avoids ambiguous player names.

## Build

```powershell
dotnet build PlayerPunishments.csproj -c Release
```

The plugin DLL is created at:

```text
bin/Release/net10.0/PlayerPunishments.dll
```

To build and create an installation ZIP:

```powershell
.\build-release.ps1
```

## Install

Extract or copy the plugin to:

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