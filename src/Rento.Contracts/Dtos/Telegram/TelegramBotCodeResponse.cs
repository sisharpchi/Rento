namespace Rento.Contracts.Dtos.Telegram;

/// <summary>
/// Bot: code returned to the bot.
/// </summary>
/// <param name="Code">4-digit code.</param>
/// <param name="ExpiresAtUtc">When the code expires (UTC).</param>
/// <param name="Regenerated">True if a new code was generated; false if the previous code is still valid and returned as-is.</param>
public record TelegramBotCodeResponse(string Code, DateTimeOffset? ExpiresAtUtc, bool Regenerated);
