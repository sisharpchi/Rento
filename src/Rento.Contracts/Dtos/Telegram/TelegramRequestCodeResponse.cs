namespace Rento.Contracts.Dtos.Telegram;

/// <summary>
/// Response after requesting code (Mini App).
/// </summary>
public record TelegramRequestCodeResponse(bool Success, string Message);
