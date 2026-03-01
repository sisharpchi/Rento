namespace Rento.Contracts.Dtos.Telegram;

public record TelegramProfileResponse(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    long TelegramId);
