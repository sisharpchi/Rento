namespace Rento.Contracts.Dtos.Telegram;

/// <summary>
/// Mini App: request code for login — only phone number. User must already be linked (bot /start or register) so API finds user by phone and generates code.
/// </summary>
public record TelegramRequestCodeRequest(string PhoneNumber);
