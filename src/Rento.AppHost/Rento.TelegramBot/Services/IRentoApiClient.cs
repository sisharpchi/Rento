namespace Rento.TelegramBot.Services;

public interface IRentoApiClient
{
    Task<bool> EnsureUserAsync(long telegramUserId, string? firstName, string? lastName, string? userName, string? phoneNumber, CancellationToken ct = default);
    Task<string?> GetCodeForBotAsync(long telegramUserId, CancellationToken ct = default);
    Task<TelegramProfileDto?> GetProfileAsync(long telegramUserId, CancellationToken ct = default);
}

/// <summary>
/// Profile data returned by API (matches Rento.Contracts.Dtos.Telegram.TelegramProfileResponse).
/// </summary>
public record TelegramProfileDto(string? FirstName, string? LastName, string? PhoneNumber, long TelegramId);
