using Rento.Contracts.Dtos.Telegram;
using Rento.Shared;

namespace Rento.Contracts.Services;

/// <summary>
/// Telegram Mini App and Bot auth: register/link, request code (2 min), get code for bot.
/// </summary>
public interface ITelegramAuthService
{
    /// <summary>
    /// Mini App start: create user or link TelegramId to existing user by phone.
    /// </summary>
    Task<ResponseResult> RegisterOrLinkAsync(TelegramRegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// Mini App login: find user by phone, generate 2-minute code and save.
    /// </summary>
    Task<ResponseResult<TelegramRequestCodeResponse>> RequestCodeAsync(TelegramRequestCodeRequest request, CancellationToken ct = default);

    /// <summary>
    /// Bot: get code by Telegram user id; create and save new code if missing or expired.
    /// </summary>
    Task<ResponseResult<string>> GetCodeForBotAsync(long telegramUserId, CancellationToken ct = default);

    /// <summary>
    /// Bot /start: ensure user exists and update FirstName, LastName, UserName from Telegram.
    /// </summary>
    Task<ResponseResult> EnsureTelegramUserAsync(TelegramUserInfoRequest request, CancellationToken ct = default);

    /// <summary>
    /// Bot: get profile by Telegram user id (for profile button).
    /// </summary>
    Task<ResponseResult<TelegramProfileResponse>> GetProfileByTelegramIdAsync(long telegramUserId, CancellationToken ct = default);
}
