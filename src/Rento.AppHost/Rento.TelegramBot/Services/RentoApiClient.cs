using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Rento.TelegramBot.Configuration;

namespace Rento.TelegramBot.Services;

public class RentoApiClient : IRentoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TelegramBotOptions _botOptions;

    public RentoApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<TelegramBotOptions> botOptions)
    {
        _httpClient = httpClientFactory.CreateClient("RentoApi");
        _botOptions = botOptions.Value;
    }

    public async Task<bool> EnsureUserAsync(long telegramUserId, string? firstName, string? lastName, string? userName, string? phoneNumber, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/telegram/ensure-user");
        request.Headers.Add("X-Bot-Secret", _botOptions.SecretKey);
        request.Content = JsonContent.Create(new
        {
            TelegramUserId = telegramUserId,
            FirstName = firstName,
            LastName = lastName,
            UserName = userName,
            PhoneNumber = phoneNumber
        });
        var response = await _httpClient.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> GetCodeForBotAsync(long telegramUserId, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/telegram/code-for-bot");
        request.Headers.Add("X-Bot-Secret", _botOptions.SecretKey);
        request.Content = JsonContent.Create(new { TelegramUserId = telegramUserId });
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : null;
    }

    public async Task<TelegramProfileDto?> GetProfileAsync(long telegramUserId, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/auth/telegram/profile?telegramUserId={telegramUserId}");
        request.Headers.Add("X-Bot-Secret", _botOptions.SecretKey);
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;
        var dto = await response.Content.ReadFromJsonAsync<TelegramProfileDto>(ct);
        return dto;
    }
}
