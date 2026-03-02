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

    public async Task<TelegramCodeResult?> GetCodeForBotAsync(long telegramUserId, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/telegram/code-for-bot");
        request.Headers.Add("X-Bot-Secret", _botOptions.SecretKey);
        request.Content = JsonContent.Create(new { TelegramUserId = telegramUserId });
        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        if (!json.TryGetProperty("code", out var codeProp))
            return null;
        var code = codeProp.GetString();
        if (string.IsNullOrEmpty(code))
            return null;
        DateTimeOffset? expiresAtUtc = null;
        if (json.TryGetProperty("expiresAtUtc", out var expProp) && expProp.TryGetDateTimeOffset(out var dt))
            expiresAtUtc = dt;
        var regenerated = json.TryGetProperty("regenerated", out var regProp) && regProp.GetBoolean();
        return new TelegramCodeResult(code, expiresAtUtc, regenerated);
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

    public async Task<bool> SetLanguageAsync(long telegramUserId, string language, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/telegram/set-language");
        request.Headers.Add("X-Bot-Secret", _botOptions.SecretKey);
        request.Content = JsonContent.Create(new { TelegramUserId = telegramUserId, Language = language });
        var response = await _httpClient.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }
}
