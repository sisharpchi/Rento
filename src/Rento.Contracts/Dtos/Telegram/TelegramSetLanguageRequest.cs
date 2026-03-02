namespace Rento.Contracts.Dtos.Telegram;

/// <summary>
/// Bot: set user language (uz, ru, en).
/// </summary>
public record TelegramSetLanguageRequest(long TelegramUserId, string Language);
