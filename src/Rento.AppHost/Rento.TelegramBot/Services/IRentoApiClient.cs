namespace Rento.TelegramBot.Services;

public interface IRentoApiClient
{
    Task<bool> EnsureUserAsync(long telegramUserId, string? firstName, string? lastName, string? userName, string? phoneNumber, CancellationToken ct = default);
    Task<TelegramCodeResult?> GetCodeForBotAsync(long telegramUserId, CancellationToken ct = default);
    Task<TelegramProfileDto?> GetProfileAsync(long telegramUserId, CancellationToken ct = default);
    Task<bool> SetLanguageAsync(long telegramUserId, string language, CancellationToken ct = default);
}

/// <summary>
/// Profile data returned by API (matches Rento.Contracts.Dtos.Telegram.TelegramProfileResponse).
/// </summary>
public record TelegramProfileDto(string? FirstName, string? LastName, string? PhoneNumber, long TelegramId, string? Language);

/// <summary>
/// Code response from API (matches TelegramBotCodeResponse).
/// </summary>
public record TelegramCodeResult(string Code, DateTimeOffset? ExpiresAtUtc);
