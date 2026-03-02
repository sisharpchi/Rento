using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rento.TelegramBot.Services;

/// <summary>
/// Handles inline button callbacks: NewCode (Yangi kod olish), ProfileClose (X).
/// </summary>
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
        var messageId = update.CallbackQuery.Message?.MessageId;
        if (chatId is null || messageId is null)
            return;

        try
        {
            if (data == CallbackData.NewCode)
                await HandleNewCodeAsync(bot, chatId.Value, messageId.Value, telegramUserId, update.CallbackQuery.Id, ct);
            else if (data == CallbackData.ProfileClose)
                await HandleProfileCloseAsync(bot, chatId.Value, messageId.Value, update.CallbackQuery.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Callback handling failed. Data={Data}", data);
            await bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, BotMessages.Get("ServiceError", null), showAlert: true, cancellationToken: ct);
        }
    }

    private async Task HandleNewCodeAsync(ITelegramBotClient bot, long chatId, int messageId, long telegramUserId, string callbackQueryId, CancellationToken ct)
    {
        var profile = await _apiClient.GetProfileAsync(telegramUserId, ct);
        var lang = profile?.Language;

        var result = await _apiClient.GetCodeForBotAsync(telegramUserId, ct);

        if (result == null)
        {
            await bot.AnswerCallbackQueryAsync(callbackQueryId, cancellationToken: ct);
            await bot.SendTextMessageAsync(chatId, BotMessages.Get("ServiceError", lang), cancellationToken: ct);
            return;
        }

        if (!result.Regenerated)
        {
            await bot.AnswerCallbackQueryAsync(callbackQueryId, BotMessages.Get("OldCodeStillValid", lang), showAlert: true, cancellationToken: ct);
            return;
        }

        await bot.AnswerCallbackQueryAsync(callbackQueryId, cancellationToken: ct);
        var codeText = string.Format(BotMessages.Get("CodeSentFormat", lang), result.Code);
        var inline = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(BotMessages.Get(BotMessages.KeyNewCodeButton, lang), CallbackData.NewCode));
        await bot.EditMessageTextAsync(
            chatId,
            messageId,
            codeText,
            replyMarkup: inline,
            cancellationToken: ct);
    }

    private async Task HandleProfileCloseAsync(ITelegramBotClient bot, long chatId, int messageId, string callbackQueryId, CancellationToken ct)
    {
        await bot.AnswerCallbackQueryAsync(callbackQueryId, cancellationToken: ct);
        await bot.DeleteMessageAsync(chatId, messageId, ct);
    }
}
