using Microsoft.EntityFrameworkCore;
using Rento.Contracts.Dtos.Telegram;
using Rento.Contracts.Services;
using Rento.Core.Entities;
using Rento.Core.Persistence;
using Rento.Shared;

namespace Rento.Application.Services;

public class TelegramAuthService : ITelegramAuthService
{
    private const int CodeValidMinutes = 2;
    private readonly IMainRepository _mainRepository;

    public TelegramAuthService(IMainRepository mainRepository)
    {
        _mainRepository = mainRepository;
    }

    public async Task<ResponseResult> RegisterOrLinkAsync(TelegramRegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return ResponseResult.CreateError("Phone number is required.", ErrorCodes.InvalidPhone);

        var user = await _mainRepository.Set<User>()
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, ct);

        if (user is null)
        {
            user = new User
            {
                UserName = request.PhoneNumber,
                PhoneNumber = request.PhoneNumber,
                TelegramId = request.TelegramUserId
            };
            _mainRepository.Add(user);
        }
        else
        {
            user.TelegramId = request.TelegramUserId;
        }

        await _mainRepository.UnitOfWork.CommitAsync(ct);
        return ResponseResult.CreateSuccess();
    }

    public async Task<ResponseResult<TelegramRequestCodeResponse>> RequestCodeAsync(TelegramRequestCodeRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return ResponseResult<TelegramRequestCodeResponse>.CreateError("Phone number is required.", ErrorCodes.InvalidPhone);

        var user = await _mainRepository.Set<User>()
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, ct);

        if (user is null)
            return ResponseResult<TelegramRequestCodeResponse>.CreateError("User with this phone number not found.", ErrorCodes.UserNotFound);

        var code = Generate4DigitCode();
        user.Code = code;
        user.CodeExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(CodeValidMinutes);
        user.TelegramId = request.TelegramUserId;
        await _mainRepository.UnitOfWork.CommitAsync(ct);

        return ResponseResult<TelegramRequestCodeResponse>.CreateSuccess(
            new TelegramRequestCodeResponse(
                true,
                "Code generated. Open the Telegram bot and press /start to receive your code."));
    }

    public async Task<ResponseResult<string>> GetCodeForBotAsync(long telegramUserId, CancellationToken ct = default)
    {
        var user = await _mainRepository.Set<User>()
            .FirstOrDefaultAsync(u => u.TelegramId == telegramUserId, ct);

        if (user is null)
            return ResponseResult<string>.CreateError(
                "No user linked to this Telegram account. Ask the user to register from the Mini App first.",
                ErrorCodes.NoCodeForTelegramUser);

        var now = DateTimeOffset.UtcNow;
        var codeExpired = user.CodeExpiresAtUtc is null || user.CodeExpiresAtUtc < now;

        if (codeExpired || string.IsNullOrEmpty(user.Code))
        {
            var code = Generate4DigitCode();
            user.Code = code;
            user.CodeExpiresAtUtc = now.AddMinutes(CodeValidMinutes);
            await _mainRepository.UnitOfWork.CommitAsync(ct);
            return ResponseResult<string>.CreateSuccess(code);
        }

        return ResponseResult<string>.CreateSuccess(user.Code);
    }

    public async Task<ResponseResult> EnsureTelegramUserAsync(TelegramUserInfoRequest request, CancellationToken ct = default)
    {
        var user = await _mainRepository.Set<User>()
            .FirstOrDefaultAsync(u => u.TelegramId == request.TelegramUserId, ct);

        if (user is null)
        {
            user = new User
            {
                UserName = request.UserName ?? request.TelegramUserId.ToString(),
                TelegramId = request.TelegramUserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            };
            _mainRepository.Add(user);
        }
        else
        {
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            if (!string.IsNullOrWhiteSpace(request.UserName))
                user.UserName = request.UserName;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;
        }

        await _mainRepository.UnitOfWork.CommitAsync(ct);
        return ResponseResult.CreateSuccess();
    }

    public async Task<ResponseResult<TelegramProfileResponse>> GetProfileByTelegramIdAsync(long telegramUserId, CancellationToken ct = default)
    {
        var user = await _mainRepository.Set<User>()
            .FirstOrDefaultAsync(u => u.TelegramId == telegramUserId, ct);

        if (user is null)
            return ResponseResult<TelegramProfileResponse>.CreateError("User not found.", ErrorCodes.UserNotFound);

        return ResponseResult<TelegramProfileResponse>.CreateSuccess(
            new TelegramProfileResponse(user.FirstName, user.LastName, user.PhoneNumber, user.TelegramId ?? 0));
    }

    private static string Generate4DigitCode() =>
        new string(Enumerable.Range(0, 4).Select(_ => Random.Shared.Next(0, 10).ToString()[0]).ToArray());
}
