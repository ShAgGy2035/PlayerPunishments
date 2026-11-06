# Player Punishments

A [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) plugin that adds server-console/RCON commands for slaying and slapping players. It is designed for [ShAgGy's Ultimate CS2 RCON/Server Tool](https://github.com/ShAgGy2035/Ultimate-RCON-Server-Tool) and does not require a separate admin plugin.

Version: 1.0.1

Author: ShAgGy

Use it only if your existing administration package does not already provide `css_slay` and `css_slap`. Universal is optional: the commands also work directly from the server console or an authorized RCON client.

**New installation?** Follow [Install and Verify](#install). **Upgrading?** Read the [compatibility warning](#upgrade-and-compatibility) first. See the [1.0.1 release notes](https://github.com/ShAgGy2035/PlayerPunishments/releases/tag/v1.0.1) for what changed.

## Contents

- [Requirements](#requirements)
- [Download](#download)
- [Upgrade and Compatibility](#upgrade-and-compatibility)
- [Install and Verify](#install)
- [Commands](#commands)
  - [Choose a Player](#choose-a-player)
  - [Test on a Non-Production Server](#test-on-a-non-production-server)
- [RCON Tool Integration](#rcon-tool-integration)
- [Configuration and Diagnostics](#configuration)
- [Troubleshooting](#troubleshooting)

## Requirements

- Counter-Strike 2 dedicated server
- Metamod:Source
- CounterStrikeSharp with .NET 10 support
- CounterStrikeSharp API `1.0.372`

## Download

Download the latest installation ZIP from [GitHub Releases](https://github.com/ShAgGy2035/PlayerPunishments/releases). Compiled binaries are not distributed from the repository's source file listing.

Download version 1.0.1, not an older build with different command ownership.

## Upgrade and Compatibility

**`css_alive` moves to [RCON Tool Companion 1.0.3](https://github.com/ShAgGy2035/RconCompanionTool).** PlayerPunishments 1.0.1 provides only `css_slay` and `css_slap`. If you use alive/dead and health reporting, upgrade both plugins together. Do not retain an older PlayerPunishments copy alongside the new Companion: both would register `css_alive`.

Back up the current plugin and configuration before upgrading. Keep only the intended PlayerPunishments DLL in its plugin folder and restart after replacing it. The new optional diagnostics do not change command behavior or targeting.

<a id="install"></a>

## Install and Verify

1. Download the matching installation ZIP from GitHub Releases.
2. Extract it into `game/csgo/addons/counterstrikesharp/plugins`. Confirm the final path is:

   ```text
   game/csgo/addons/counterstrikesharp/plugins/PlayerPunishments/PlayerPunishments.dll
   ```

3. Restart the CS2 server or load the plugin through CounterStrikeSharp.
4. From the server console or RCON, verify that it appears in:

   ```text
   css_plugins list
   ```

Listing plugins does not affect players. Once loading is confirmed, use the commands below only on the intended target.

## Commands

Commands are server-console/RCON only. Players cannot execute them from chat or their client console.

| Command | Description |
| --- | --- |
| `css_slay <#userid\|steamid64\|exact name>` | Kills a living player. |
| `css_slap <#userid\|steamid64\|exact name> [damage]` | Damages and launches a living player. Damage defaults to `5` and must be from `0` to `100`. |

### Choose a Player

Run `status` to get the current user ID, then prefer `#userid` for an unambiguous target. User IDs belong to the current connection; check them again after a player reconnects.

Names are matched case-insensitively but must otherwise be exact; quote names containing spaces. SteamID and name lookup target humans, while `#userid` also resolves bots. Duplicate names are rejected rather than choosing an arbitrary player.

### Test on a Non-Production Server

Use a test bot or consenting player. **Slay kills the target; Slap damages and launches them.** A zero-damage slap still launches them. Lethal slap damage slays the player, and dead targets are rejected.

For example, after confirming that `#12` is your intended living test target:

```text
status
css_slap #12 0
css_slap #12
css_slap #12 10
css_slay #12
```

## RCON Tool Integration

Select the player in Universal's player table, then use **Slay** or **Slap**. Universal sends the selected user ID and its configured slap damage:

```text
css_slap #<userid> <damage>
css_slay #<userid>
```

The plugin writes success and failure messages back to the server console, so they are visible in RCON output and logs.

Universal's Slap damage setting is under **Settings > Settings > General** and accepts `0`-`99`. Direct plugin commands accept `0`-`100`. Alive/health columns require Companion's `css_alive`, not this plugin.

<a id="configuration"></a>

## Configuration and Diagnostics

No configuration change is needed for normal Slay/Slap use. CounterStrikeSharp generates:

```text
game/csgo/addons/counterstrikesharp/configs/plugins/PlayerPunishments/PlayerPunishments.json
```

For optional tracking:

1. Set `"DebugLogging": true` in that file; the default is `false`.
2. Keep the generated `ConfigVersion` field, save valid JSON, and reload the plugin.
3. Reproduce the issue and inspect `[PlayerPunishments:debug]` information-level entries.
4. Set `DebugLogging` back to `false` and reload when finished.

Tracking records fixed command names and outcomes: started, succeeded, missing or ambiguous target, dead target, invalid damage, or no players. It omits target names, user IDs, Steam IDs, raw arguments, chat, and credentials. Disabling it suppresses diagnostic entries without changing replies or behavior.

**Review logs before sharing:** ordinary command replies and successful-action logs are unchanged and can contain player names and user IDs. Those are not the secret-safe debug entries.

## Troubleshooting

| Problem | Check |
| --- | --- |
| Commands are unknown | Confirm `css_plugins list` shows PlayerPunishments and that its DLL is in the plugin directory shown above. Check CounterStrikeSharp's load errors and runtime requirements. |
| A player cannot run a command | This is intentional: use the server console or RCON, not player chat/client console. |
| A target is missing or ambiguous | Use the current `#userid` from `status`, or quote an exact name. Use `#userid` for bots. |
| Slap is rejected | Check that the player is alive and damage is between 0 and 100. |
| Alive/health reporting is unavailable | Install Companion 1.0.3; PlayerPunishments 1.0.1 no longer provides `css_alive`. |
| More diagnostics are needed | Enable `DebugLogging` as described under Configuration, reproduce the problem, then disable it. Review ordinary logs for player information before sharing. |
