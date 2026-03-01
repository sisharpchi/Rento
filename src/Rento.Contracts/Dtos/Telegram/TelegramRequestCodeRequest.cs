namespace Rento.Contracts.Dtos.Telegram;

/// <summary>
/// Mini App: request code for login — phone + telegram user id.
/// </summary>
public record TelegramRequestCodeRequest(string PhoneNumber, long TelegramUserId);
