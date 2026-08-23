using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace PlayerPunishments;

public sealed class PlayerPunishments : BasePlugin
{
    private const int DefaultSlapDamage = 5;

    public override string ModuleName => "Player Punishments";
    public override string ModuleDescription => "Adds RCON-only player state, slay, and slap commands";
    public override string ModuleAuthor => "ShAgGy";
    public override string ModuleVersion => "1.0.0";

    [ConsoleCommand("css_alive", "Show whether one or all players are alive or dead")]
    [CommandHelper(minArgs: 0, usage: "[#userid|steamid64|name]", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnAliveCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (command.ArgCount >= 2)
        {
            var target = ResolveTarget(command.GetArg(1), command);
            if (target is not null) ReplyWithPlayerState(target, command);
            return;
        }

        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid)
            .OrderBy(player => player.UserId)
            .ToList();
        if (players.Count == 0)
        {
            command.ReplyToCommand("[PlayerPunishments] No players are connected.");
            return;
        }

        command.ReplyToCommand($"[PlayerPunishments] Player states ({players.Count}):");
        foreach (var player in players) ReplyWithPlayerState(player, command);
    }

    [ConsoleCommand("css_slay", "Slay a player by #userid, SteamID64, or exact name")]
    [CommandHelper(minArgs: 1, usage: "<#userid|steamid64|name>", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnSlayCommand(CCSPlayerController? caller, CommandInfo command)
    {
        var target = ResolveTarget(command.GetArg(1), command);
        if (target is null || !RequireAlivePawn(target, command)) return;

        target.CommitSuicide(explode: false, force: true);
        command.ReplyToCommand($"[PlayerPunishments] Slayed {target.PlayerName} (#{target.UserId}).");
        Logger.LogInformation("Slayed {PlayerName} (#{UserId}) from server console/RCON.", target.PlayerName, target.UserId);
    }

    [ConsoleCommand("css_slap", "Slap a player by #userid, SteamID64, or exact name")]
    [CommandHelper(minArgs: 1, usage: "<#userid|steamid64|name> [damage]", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnSlapCommand(CCSPlayerController? caller, CommandInfo command)
    {
        var target = ResolveTarget(command.GetArg(1), command);
        if (target is null || !RequireAlivePawn(target, command)) return;

        var damage = DefaultSlapDamage;
        if (command.ArgCount >= 3 && (!int.TryParse(command.GetArg(2), out damage) || damage is < 0 or > 100))
        {
            command.ReplyToCommand("[PlayerPunishments] Damage must be a whole number from 0 to 100.");
            return;
        }

        var pawn = target.PlayerPawn.Value!;
        if (damage >= pawn.Health)
        {
            target.CommitSuicide(explode: false, force: true);
        }
        else if (damage > 0)
        {
            pawn.Health -= damage;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        var horizontalX = Random.Shared.NextSingle() * 500f - 250f;
        var horizontalY = Random.Shared.NextSingle() * 500f - 250f;
        pawn.Teleport(null, null, new Vector(horizontalX, horizontalY, 350f));

        command.ReplyToCommand($"[PlayerPunishments] Slapped {target.PlayerName} (#{target.UserId}) for {damage} damage.");
        Logger.LogInformation(
            "Slapped {PlayerName} (#{UserId}) for {Damage} damage from server console/RCON.",
            target.PlayerName,
            target.UserId,
            damage);
    }

    private static CCSPlayerController? ResolveTarget(string input, CommandInfo command)
    {
        var targetText = input.Trim();
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid && !player.IsBot)
            .ToList();

        if (targetText.StartsWith('#') && int.TryParse(targetText[1..], out var userId))
        {
            var byUserId = Utilities.GetPlayerFromUserid(userId);
            if (byUserId is not null && byUserId.IsValid) return byUserId;
        }

        if (ulong.TryParse(targetText.TrimStart('#'), out var steamId))
        {
            var bySteamId = players.FirstOrDefault(player => player.SteamID == steamId);
            if (bySteamId is not null) return bySteamId;
        }

        var nameMatches = players
            .Where(player => string.Equals(player.PlayerName, targetText, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (nameMatches.Count == 1) return nameMatches[0];

        command.ReplyToCommand(nameMatches.Count > 1
            ? $"[PlayerPunishments] More than one player is named '{targetText}'. Use #userid."
            : $"[PlayerPunishments] Player '{targetText}' was not found. Use #userid from status.");
        return null;
    }

    private static bool RequireAlivePawn(CCSPlayerController target, CommandInfo command)
    {
        if (IsAlive(target)) return true;

        command.ReplyToCommand($"[PlayerPunishments] {target.PlayerName} is not alive.");
        return false;
    }

    private static bool IsAlive(CCSPlayerController target)
    {
        var pawn = target.PlayerPawn.Value;
        return pawn is not null && pawn.IsValid && pawn.LifeState == 0 && pawn.Health > 0;
    }

    private static void ReplyWithPlayerState(CCSPlayerController target, CommandInfo command)
    {
        var pawn = target.PlayerPawn.Value;
        var isAlive = IsAlive(target);
        var state = isAlive ? "alive" : "dead";
        var health = isAlive ? pawn!.Health : 0;
        var bot = target.IsBot ? ", bot" : string.Empty;
        command.ReplyToCommand(
            $"[PlayerPunishments] #{target.UserId} \"{target.PlayerName}\": {state}, health={health}{bot}.");
    }
}