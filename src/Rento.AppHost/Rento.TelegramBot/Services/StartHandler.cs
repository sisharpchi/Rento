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
    /// /start: ensure user (without phone), then ask for phone.
    /// </summary>
    public async Task HandleAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message?.From is not { } from)
        {
            await bot.SendTextMessageAsync(update.Message!.Chat.Id, BotMessages.TelegramUserIdNotFound, cancellationToken: ct);
            return;
        }

        var ok = await _apiClient.EnsureUserAsync(from.Id, from.FirstName, from.LastName, from.Username, phoneNumber: null, ct);
        if (!ok)
            _logger.LogWarning("EnsureUser failed for TelegramUserId={TelegramUserId}", from.Id);

        var requestPhoneKeyboard = new ReplyKeyboardMarkup(KeyboardButton.WithRequestContact(BotMessages.SendPhoneButton))
        {
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        };

        await bot.SendTextMessageAsync(
            update.Message.Chat.Id,
            BotMessages.AskPhone,
            replyMarkup: requestPhoneKeyboard,
            cancellationToken: ct);
    }

    /// <summary>
    /// User sent contact or text as phone: save phone and show main menu.
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
            await bot.SendTextMessageAsync(update.Message.Chat.Id, BotMessages.AskPhone, cancellationToken: ct);
            return;
        }

        var ok = await _apiClient.EnsureUserAsync(from.Id, from.FirstName, from.LastName, from.Username, phone, ct);
        if (!ok)
            _logger.LogWarning("EnsureUser(phone) failed for TelegramUserId={TelegramUserId}", from.Id);

        // Remove reply keyboard
        await bot.SendTextMessageAsync(
            update.Message.Chat.Id,
            BotMessages.PhoneSaved,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData(BotMessages.ButtonCode, CallbackData.Code),
            InlineKeyboardButton.WithCallbackData(BotMessages.ButtonProfile, CallbackData.Profile),
            InlineKeyboardButton.WithCallbackData(BotMessages.ButtonLanguage, CallbackData.Lang)
        });

        await bot.SendTextMessageAsync(
            update.Message.Chat.Id,
            BotMessages.Welcome,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private static string? NormalizePhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Where(c => c == '+' || char.IsDigit(c)).ToArray());
        return digits.Length >= 9 ? digits : null;
    }
}
