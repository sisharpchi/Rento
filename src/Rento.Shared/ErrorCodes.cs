namespace Rento.Shared;

/// <summary>
/// Error codes for API responses (e.g. ResponseResult.CreateError(message, code)).
/// </summary>
public static class ErrorCodes
{
    public const int UserNotFound = 40401;
    public const int InvalidPhone = 40001;
    public const int CodeExpired = 40002;
    public const int NoCodeForTelegramUser = 40402;
}
