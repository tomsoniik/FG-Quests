using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using FragHubQuests.Config;
using FragHubQuests.Managers;
using FragHubQuests.Services;

namespace FragHubQuests;

[MinimumApiVersion(130)]
public class FragHubQuestsPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "FragHub Quests";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Antigravity";
    public override string ModuleDescription => "Quests integration with FragHub";

    public PluginConfig Config { get; set; } = new();

    private ApiService _apiService = null!;
    private QuestManager _questManager = null!;

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        _apiService = new ApiService(Config);
        _questManager = new QuestManager(_apiService);

        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        // Add more events like round_mvp, bomb_defused, etc. based on quest types
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null && player.IsValid && !player.IsBot)
        {
            Task.Run(() => _questManager.LoadPlayerQuestsAsync(player));
        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null)
        {
            _questManager.RemovePlayer(player);
        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var victim = @event.Userid;

        if (attacker != null && attacker.IsValid && !attacker.IsBot && attacker != victim)
        {
            Task.Run(async () => 
            {
                await _questManager.HandleProgressAsync(attacker, "KILLS", 1);
                
                if (@event.Headshot)
                {
                    await _questManager.HandleProgressAsync(attacker, "HEADSHOTS", 1);
                }
            });
        }
        return HookResult.Continue;
    }

    [ConsoleCommand("css_quests", "Pokazuje liste zadan")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnQuestsCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;

        var steamId = player.AuthorizedSteamID?.SteamId64.ToString();
        if (steamId == null) return;

        var questPlayer = _questManager.GetPlayer(steamId);
        if (questPlayer == null || questPlayer.Quests.Count == 0)
        {
            player.PrintToChat($"{Config.ChatPrefix} Nie masz aktywnych zadan.");
            return;
        }

        player.PrintToChat($"{Config.ChatPrefix} Twoje aktywne zadania:");
        foreach (var quest in questPlayer.Quests)
        {
            string status = quest.IsCompleted ? "[Ukończone]" : $"[{quest.CurrentProgress}/{quest.TargetAmount}]";
            player.PrintToChat($" \x04- {quest.Title} {status} (+{quest.ExpReward} EXP)");
        }
    }

    [ConsoleCommand("css_link", "Laczy konto z FragHub")]
    [CommandHelper(minArgs: 1, usage: "<token>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnLinkCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null) return;

        var token = command.ArgByIndex(1);
        var steamId = player.AuthorizedSteamID?.SteamId64.ToString();
        
        if (steamId == null) return;

        Task.Run(async () =>
        {
            var success = await _apiService.LinkAccountAsync(steamId, token);
            if (success)
            {
                Server.NextFrame(() => player.PrintToChat($"{Config.ChatPrefix} Konto zostalo pomyslnie polaczone!"));
                await _questManager.LoadPlayerQuestsAsync(player);
            }
            else
            {
                Server.NextFrame(() => player.PrintToChat($"{Config.ChatPrefix} Nie udalo sie polaczyc konta. Sprawdz token."));
            }
        });
    }
}
