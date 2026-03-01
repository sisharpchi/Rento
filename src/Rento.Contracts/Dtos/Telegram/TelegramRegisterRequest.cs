namespace Rento.Contracts.Dtos.Telegram;

/// <summary>
/// Mini App start: register or link user by phone and Telegram user id.
/// </summary>
public record TelegramRegisterRequest(string PhoneNumber, long TelegramUserId);
