using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rento.TelegramBot.Services;

public class CallbackQueryHandler
{
    private readonly IRentoApiClient _apiClient;
    private readonly ILogger<CallbackQueryHandler> _logger;

    public CallbackQueryHandler(IRentoApiClient apiClient, ILogger<CallbackQueryHandler> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task HandleAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.CallbackQuery?.Data is not { } data || update.CallbackQuery.From?.Id is not { } telegramUserId)
            return;

        var chatId = update.CallbackQuery.Message?.Chat.Id;
        if (chatId is null)
            return;

        try
        {
            if (data == CallbackData.Code)
                await HandleCodeAsync(bot, chatId.Value, telegramUserId, update.CallbackQuery.Id, ct);
            else if (data == CallbackData.Profile)
                await HandleProfileAsync(bot, chatId.Value, telegramUserId, update.CallbackQuery.Id, ct);
            else if (data.StartsWith("lang"))
                await HandleLanguageAsync(bot, chatId.Value, data, update.CallbackQuery.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Callback handling failed. Data={Data}", data);
            await bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, BotMessages.ServiceError, cancellationToken: ct);
        }
    }

    private async Task HandleCodeAsync(ITelegramBotClient bot, long chatId, long telegramUserId, string callbackQueryId, CancellationToken ct)
    {
        var code = await _apiClient.GetCodeForBotAsync(telegramUserId, ct);
        await bot.AnswerCallbackQueryAsync(callbackQueryId, cancellationToken: ct);

        if (string.IsNullOrEmpty(code))
        {
            await bot.SendTextMessageAsync(chatId, BotMessages.NoCodeYet, cancellationToken: ct);
            return;
        }

        await bot.SendTextMessageAsync(chatId, string.Format(BotMessages.CodeSentFormat, code), cancellationToken: ct);
    }

    private async Task HandleProfileAsync(ITelegramBotClient bot, long chatId, long telegramUserId, string callbackQueryId, CancellationToken ct)
    {
        await bot.AnswerCallbackQueryAsync(callbackQueryId, cancellationToken: ct);
        var profile = await _apiClient.GetProfileAsync(telegramUserId, ct);
        var firstName = profile?.FirstName ?? "—";
        var lastName = profile?.LastName ?? "—";
        var phone = string.IsNullOrEmpty(profile?.PhoneNumber) ? BotMessages.ProfileMiniAppHint : profile.PhoneNumber;
        var telegramIdStr = profile?.TelegramId.ToString() ?? "—";
        var profileText = string.Format(BotMessages.ProfileFormat, firstName, lastName, telegramIdStr, phone);
        await bot.SendTextMessageAsync(chatId, profileText, cancellationToken: ct);
    }

    private async Task HandleLanguageAsync(ITelegramBotClient bot, long chatId, string data, string callbackQueryId, CancellationToken ct)
    {
        await bot.AnswerCallbackQueryAsync(callbackQueryId, cancellationToken: ct);

        if (data == CallbackData.Lang)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("O'zbekcha", CallbackData.LangUz),
                InlineKeyboardButton.WithCallbackData("Русский", CallbackData.LangRu),
                InlineKeyboardButton.WithCallbackData("English", CallbackData.LangEn)
            });
            await bot.SendTextMessageAsync(chatId, BotMessages.LanguageChoose, replyMarkup: keyboard, cancellationToken: ct);
            return;
        }

        await bot.SendTextMessageAsync(chatId, BotMessages.LanguageSet, cancellationToken: ct);
    }
}
