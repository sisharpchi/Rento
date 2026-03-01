namespace Rento.Contracts.Dtos.Telegram;

/// <summary>
/// Bot /start: save or update user from Telegram (PhoneNumber optional, can be set later).
/// </summary>
public record TelegramUserInfoRequest(
    long TelegramUserId,
    string? FirstName,
    string? LastName,
    string? UserName,
    string? PhoneNumber);
