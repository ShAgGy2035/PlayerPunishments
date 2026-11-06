using Microsoft.Extensions.Logging;

namespace PlayerPunishments;

internal interface IPlayer
{
    bool IsValid { get; }
    bool IsBot { get; }
    int? UserId { get; }
    ulong SteamId { get; }
    string Name { get; }
    bool IsAlive { get; }
    int Health { get; }
    void Slay();
    void SetHealth(int health);
    void Launch();
}

internal sealed class PunishmentCommands(
    Func<IReadOnlyList<IPlayer>> getPlayers,
    Func<int, IPlayer?> getByUserId,
    Action<string> reply,
    ILogger logger,
    bool debugLogging)
{
    internal void Slay(string input)
    {
        Track("slay", "started");
        var target = ResolveTarget(input, "slay");
        if (target is null || !RequireAlive(target, "slay")) return;
        target.Slay();
        reply($"[PlayerPunishments] Slayed {target.Name} (#{target.UserId}).");
        logger.LogInformation("Slayed {PlayerName} (#{UserId}) from server console/RCON.", target.Name, target.UserId);
        Track("slay", "succeeded");
    }

    internal void Slap(string input, string? damageText)
    {
        Track("slap", "started");
        var target = ResolveTarget(input, "slap");
        if (target is null || !RequireAlive(target, "slap")) return;

        var damage = 5;
        if (damageText is not null && (!int.TryParse(damageText, out damage) || damage is < 0 or > 100))
        {
            reply("[PlayerPunishments] Damage must be a whole number from 0 to 100.");
            Track("slap", "invalid_damage");
            return;
        }

        if (damage >= target.Health) target.Slay();
        else if (damage > 0) target.SetHealth(target.Health - damage);
        target.Launch();
        reply($"[PlayerPunishments] Slapped {target.Name} (#{target.UserId}) for {damage} damage.");
        logger.LogInformation(
            "Slapped {PlayerName} (#{UserId}) for {Damage} damage from server console/RCON.",
            target.Name, target.UserId, damage);
        Track("slap", "succeeded");
    }

    private IPlayer? ResolveTarget(string input, string command)
    {
        var targetText = input.Trim();
        var players = getPlayers().Where(player => player.IsValid && !player.IsBot).ToList();
        if (targetText.StartsWith('#') && int.TryParse(targetText[1..], out var userId))
        {
            var byUserId = getByUserId(userId);
            if (byUserId is { IsValid: true }) return byUserId;
        }

        if (ulong.TryParse(targetText.TrimStart('#'), out var steamId))
        {
            var bySteamId = players.FirstOrDefault(player => player.SteamId == steamId);
            if (bySteamId is not null) return bySteamId;
        }

        var matches = players.Where(player => string.Equals(player.Name, targetText, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count == 1) return matches[0];
        reply(matches.Count > 1
            ? $"[PlayerPunishments] More than one player is named '{targetText}'. Use #userid."
            : $"[PlayerPunishments] Player '{targetText}' was not found. Use #userid from status.");
        Track(command, matches.Count > 1 ? "ambiguous_target" : "missing_target");
        return null;
    }

    private bool RequireAlive(IPlayer target, string command)
    {
        if (target.IsAlive) return true;
        reply($"[PlayerPunishments] {target.Name} is not alive.");
        Track(command, "dead_target");
        return false;
    }

    private void Track(string command, string outcome)
    {
        if (debugLogging)
            logger.LogInformation("[PlayerPunishments:debug] Command={Command} Outcome={Outcome}", command, outcome);
    }
}
