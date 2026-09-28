using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using FragHubQuests.Models;
using FragHubQuests.Services;
using System.Collections.Concurrent;

namespace FragHubQuests.Managers;

public class QuestManager
{
    private readonly ApiService _apiService;
    private readonly ConcurrentDictionary<string, QuestPlayer> _players = new();

    public QuestManager(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task LoadPlayerQuestsAsync(CCSPlayerController player)
    {
        var steamId = player.AuthorizedSteamID?.SteamId64.ToString();
        if (string.IsNullOrEmpty(steamId)) return;

        var quests = await _apiService.GetPlayerQuestsAsync(steamId);
        if (quests != null)
        {
            var questPlayer = new QuestPlayer(player, steamId)
            {
                Quests = quests
            };
            _players[steamId] = questPlayer;
        }
    }

    public void RemovePlayer(CCSPlayerController player)
    {
        var steamId = player.AuthorizedSteamID?.SteamId64.ToString();
        if (!string.IsNullOrEmpty(steamId))
        {
            _players.TryRemove(steamId, out _);
        }
    }

    public QuestPlayer? GetPlayer(string steamId)
    {
        _players.TryGetValue(steamId, out var player);
        return player;
    }

    public async Task HandleProgressAsync(CCSPlayerController player, string eventType, int amount = 1)
    {
        var steamId = player.AuthorizedSteamID?.SteamId64.ToString();
        if (string.IsNullOrEmpty(steamId)) return;

        if (_players.TryGetValue(steamId, out var questPlayer))
        {
            foreach (var quest in questPlayer.Quests.Where(q => !q.IsCompleted && q.Type == eventType))
            {
                quest.CurrentProgress += amount;
                if (quest.CurrentProgress >= quest.TargetAmount)
                {
                    quest.CurrentProgress = quest.TargetAmount;
                    quest.IsCompleted = true;
                    Server.NextFrame(() => player.PrintToChat($" \x04[FragHub] Gratulacje! Ukończyłeś zadanie: {quest.Title} (+{quest.ExpReward} EXP)"));
                    await _apiService.UpdateQuestProgressAsync(steamId, quest.Id, quest.CurrentProgress, true);
                }
                else
                {
                    // Update progress periodically or immediately
                    await _apiService.UpdateQuestProgressAsync(steamId, quest.Id, quest.CurrentProgress, false);
                }
            }
        }
    }
}
