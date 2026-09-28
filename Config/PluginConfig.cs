using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace FragHubQuests.Config;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("ApiUrl")]
    public string ApiUrl { get; set; } = "https://twoja-domena.pl/fg_addons/api/api.php";

    [JsonPropertyName("ServerToken")]
    public string ServerToken { get; set; } = "YOUR_SERVER_TOKEN_HERE";

    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = "[{Blue}FragHub{Default}]";
}
