using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace PlayerPunishments;

public sealed class PluginConfig : BasePluginConfig
{
    public bool DebugLogging { get; set; }
}

public sealed class PlayerPunishments : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "Player Punishments";
    public override string ModuleDescription => "Adds RCON-only slay and slap commands";
    public override string ModuleAuthor => "ShAgGy";
    public override string ModuleVersion => "1.0.2";

    public PluginConfig Config { get; set; } = new();
    public void OnConfigParsed(PluginConfig config) => Config = config;

    [ConsoleCommand("css_slay", "Slay a player by #userid, SteamID64, or exact name")]
    [CommandHelper(minArgs: 1, usage: "<#userid|steamid64|name>", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnSlayCommand(CCSPlayerController? caller, CommandInfo command) =>
        CreateCommands(command).Slay(command.GetArg(1));

    [ConsoleCommand("css_slap", "Slap a player by #userid, SteamID64, or exact name")]
    [CommandHelper(minArgs: 1, usage: "<#userid|steamid64|name> [damage]", whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnSlapCommand(CCSPlayerController? caller, CommandInfo command) =>
        CreateCommands(command).Slap(command.GetArg(1), command.ArgCount >= 3 ? command.GetArg(2) : null);

    private PunishmentCommands CreateCommands(CommandInfo command) => new(
        () => Utilities.GetPlayers().Select(player => (IPlayer)new NativePlayer(player)).ToList(),
        userId => Utilities.GetPlayerFromUserid(userId) is { } player ? new NativePlayer(player) : null,
        command.ReplyToCommand, Logger, Config.DebugLogging);

    private sealed class NativePlayer(CCSPlayerController player) : IPlayer
    {
        private CCSPlayerPawn? _pawn;
        public bool IsValid => player.IsValid;
        public bool IsBot => player.IsBot;
        public int? UserId => player.UserId;
        public ulong SteamId => player.SteamID;
        public string Name => player.PlayerName;
        public bool IsAlive => (_pawn = player.PlayerPawn.Value) is { IsValid: true, LifeState: 0, Health: > 0 };
        public int Health => (_pawn ?? player.PlayerPawn.Value)?.Health ?? 0;
        public void Slay() => player.CommitSuicide(explode: false, force: true);

        public void SetHealth(int health)
        {
            var pawn = _pawn!;
            pawn.Health = health;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        public void Launch()
        {
            var horizontalX = Random.Shared.NextSingle() * 500f - 250f;
            var horizontalY = Random.Shared.NextSingle() * 500f - 250f;
            _pawn!.Teleport(null, null, new Vector(horizontalX, horizontalY, 350f));
        }
    }
}
