using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rento.TelegramBot.Services;

public class StartHandler
{
    private readonly IRentoApiClient _apiClient;
    private readonly ILogger<StartHandler> _logger;

    public StartHandler(IRentoApiClient apiClient, ILogger<StartHandler> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// /start: if user with TelegramUserId already exists in DB (with phone), show menu; else ask for phone.
    /// </summary>
    public async Task HandleAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message?.From is not { } from)
        {
            await bot.SendTextMessageAsync(update.Message!.Chat.Id, BotMessages.TelegramUserIdNotFound, cancellationToken: ct);
            return;
        }

        var profile = await _apiClient.GetProfileAsync(from.Id, ct);
        var hasPhone = !string.IsNullOrWhiteSpace(profile?.PhoneNumber);

        if (hasPhone)
        {
            await _apiClient.EnsureUserAsync(from.Id, from.FirstName, from.LastName, from.Username, phoneNumber: null, ct);
            var lang = profile?.Language;
            await bot.SendTextMessageAsync(
                update.Message.Chat.Id,
                BotMessages.Get("Welcome", lang),
                replyMarkup: Keyboards.GetMainMenu(lang),
                cancellationToken: ct);
            return;
        }

        // New or no phone — ensure user record, then ask for phone
        var ok = await _apiClient.EnsureUserAsync(from.Id, from.FirstName, from.LastName, from.Username, phoneNumber: null, ct);
        if (!ok)
            _logger.LogWarning("EnsureUser failed for TelegramUserId={TelegramUserId}", from.Id);

        var requestPhoneKeyboard = Keyboards.GetRequestPhone(null);
        await bot.SendTextMessageAsync(
            update.Message.Chat.Id,
            BotMessages.Get("AskPhone", null),
            replyMarkup: requestPhoneKeyboard,
            cancellationToken: ct);
    }

    /// <summary>
    /// User sent contact or text as phone: save phone, then "Xush kelibsiz" + main menu (Reply keyboard).
    /// </summary>
    public async Task HandlePhoneMessageAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message?.From is not { } from)
            return;

        string? phone = null;
        if (update.Message.Contact?.PhoneNumber is { } contactPhone)
            phone = NormalizePhone(contactPhone);
        else if (!string.IsNullOrWhiteSpace(update.Message.Text))
            phone = NormalizePhone(update.Message.Text);

        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 9)
        {
            await bot.SendTextMessageAsync(update.Message.Chat.Id, BotMessages.Get("AskPhone", null), cancellationToken: ct);
            return;
        }

        var ok = await _apiClient.EnsureUserAsync(from.Id, from.FirstName, from.LastName, from.Username, phone, ct);
        if (!ok)
            _logger.LogWarning("EnsureUser(phone) failed for TelegramUserId={TelegramUserId}", from.Id);

        var profileAfter = await _apiClient.GetProfileAsync(from.Id, ct);
        var lang = profileAfter?.Language;

        await bot.SendTextMessageAsync(
            update.Message.Chat.Id,
            BotMessages.Get("PhoneSaved", lang),
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);

        await bot.SendTextMessageAsync(
            update.Message.Chat.Id,
            BotMessages.Get("Welcome", lang),
            replyMarkup: Keyboards.GetMainMenu(lang),
            cancellationToken: ct);
    }

    private static string? NormalizePhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Where(c => c == '+' || char.IsDigit(c)).ToArray());
        return digits.Length >= 9 ? digits : null;
    }
}
