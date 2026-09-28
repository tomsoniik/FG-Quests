using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FragHubQuests.Models;
using FragHubQuests.Config;

namespace FragHubQuests.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly PluginConfig _config;

    public ApiService(PluginConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ServerToken);
    }

    public async Task<List<Quest>?> GetPlayerQuestsAsync(string steamId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_config.ApiUrl}?action=server_quests_get&steamId={steamId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Quest>>(content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FragHubQuests] Error fetching quests for {steamId}: {ex.Message}");
        }
        return null;
    }

    public async Task UpdateQuestProgressAsync(string steamId, string questId, int progress, bool isCompleted)
    {
        try
        {
            var payload = new
            {
                steamId = steamId,
                questId = questId,
                progress = progress,
                completed = isCompleted
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_config.ApiUrl}?action=server_quests_progress", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[FragHubQuests] Failed to update progress for {steamId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FragHubQuests] Error updating progress for {steamId}: {ex.Message}");
        }
    }

    public async Task<bool> LinkAccountAsync(string steamId, string token)
    {
        try
        {
            var payload = new
            {
                steamId = steamId,
                token = token
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_config.ApiUrl}?action=server_link", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FragHubQuests] Error linking account for {steamId}: {ex.Message}");
            return false;
        }
    }
}
