using Microsoft.AspNetCore.Mvc;
using Rento.Contracts.Dtos.Telegram;
using Rento.Contracts.Services;
using Rento.Shared;

namespace Rento.AppHost.ApiService.Controllers;

[ApiController]
[Route("api/auth/telegram")]
public class TelegramAuthController : ControllerBase
{
    private readonly ITelegramAuthService _telegramAuthService;
    private readonly IConfiguration _configuration;

    public TelegramAuthController(ITelegramAuthService telegramAuthService, IConfiguration configuration)
    {
        _telegramAuthService = telegramAuthService;
        _configuration = configuration;
    }

    /// <summary>
    /// Mini App start: register or link user by phone and Telegram user id.
    /// </summary>
    [HttpPost("register")]
    [Produces("application/json")]
    public async Task<IActionResult> Register([FromBody] TelegramRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _telegramAuthService.RegisterOrLinkAsync(request, cancellationToken);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    /// <summary>
    /// Mini App: request code for login (phone + telegram user id). Code is valid 2 minutes.
    /// </summary>
    [HttpPost("request-code")]
    [Produces("application/json")]
    public async Task<IActionResult> RequestCode([FromBody] TelegramRequestCodeRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _telegramAuthService.RequestCodeAsync(request, cancellationToken);
        if (result.Success)
            return Ok(result);
        if (result.ErrorCode == 40401) // UserNotFound
            return NotFound(result);
        return BadRequest(result);
    }

    /// <summary>
    /// Bot: get code by Telegram user id. Requires X-Bot-Secret header.
    /// </summary>
    [HttpPost("code-for-bot")]
    [Produces("application/json")]
    public async Task<IActionResult> GetCodeForBot([FromBody] TelegramBotCodeRequest body, [FromHeader(Name = "X-Bot-Secret")] string? botSecret, CancellationToken cancellationToken = default)
    {
        var expectedSecret = _configuration["TelegramBot:SecretKey"];
        if (string.IsNullOrEmpty(expectedSecret) || botSecret != expectedSecret)
            return Unauthorized();

        var result = await _telegramAuthService.GetCodeForBotAsync(body.TelegramUserId, cancellationToken);
        if (result.Success)
            return Ok(new TelegramBotCodeResponse(result.Value!));
        if (result.ErrorCode == 40402) // NoCodeForTelegramUser
            return NotFound(result);
        return BadRequest(result);
    }

    /// <summary>
    /// Bot /start: ensure user exists and update from Telegram (TelegramUserId, FirstName, LastName, UserName). Requires X-Bot-Secret header.
    /// </summary>
    [HttpPost("ensure-user")]
    [Produces("application/json")]
    public async Task<IActionResult> EnsureUser([FromBody] TelegramUserInfoRequest request, [FromHeader(Name = "X-Bot-Secret")] string? botSecret, CancellationToken cancellationToken = default)
    {
        var expectedSecret = _configuration["TelegramBot:SecretKey"];
        if (string.IsNullOrEmpty(expectedSecret) || botSecret != expectedSecret)
            return Unauthorized();

        var result = await _telegramAuthService.EnsureTelegramUserAsync(request, cancellationToken);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    /// <summary>
    /// Bot: get profile by Telegram user id. Requires X-Bot-Secret header.
    /// </summary>
    [HttpGet("profile")]
    [Produces("application/json")]
    public async Task<IActionResult> GetProfile([FromQuery] long telegramUserId, [FromHeader(Name = "X-Bot-Secret")] string? botSecret, CancellationToken cancellationToken = default)
    {
        var expectedSecret = _configuration["TelegramBot:SecretKey"];
        if (string.IsNullOrEmpty(expectedSecret) || botSecret != expectedSecret)
            return Unauthorized();

        var result = await _telegramAuthService.GetProfileByTelegramIdAsync(telegramUserId, cancellationToken);
        if (result.Success)
            return Ok(result.Value);
        if (result.ErrorCode == 40401)
            return NotFound(result);
        return BadRequest(result);
    }
}
