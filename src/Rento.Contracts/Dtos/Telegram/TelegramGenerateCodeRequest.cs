namespace Rento.Contracts.Dtos.Telegram;

/// <summary>
/// Mini App: generate code for login — only phone number. User must already be linked (bot /start or register) so API finds user by phone and generates code.
/// </summary>
public record TelegramGenerateCodeRequest(string PhoneNumber);
