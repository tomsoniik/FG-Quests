using CounterStrikeSharp.API.Core;

namespace FragHubQuests.Models;

public class QuestPlayer
{
    public CCSPlayerController Player { get; set; }
    public string SteamId { get; set; }
    public List<Quest> Quests { get; set; } = new List<Quest>();

    public QuestPlayer(CCSPlayerController player, string steamId)
    {
        Player = player;
        SteamId = steamId;
    }
}
