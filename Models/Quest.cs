using System.Text.Json.Serialization;

namespace FragHubQuests.Models;

public class Quest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("expReward")]
    public int ExpReward { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // e.g., "KILLS", "HEADSHOTS", "DAMAGE"

    [JsonPropertyName("targetAmount")]
    public int TargetAmount { get; set; }

    [JsonPropertyName("currentProgress")]
    public int CurrentProgress { get; set; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }
}
