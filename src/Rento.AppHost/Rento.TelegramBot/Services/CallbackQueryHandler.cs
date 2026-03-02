using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rento.TelegramBot.Services;

/// <summary>
/// Handles inline button callbacks: ProfileClose (X). NewCode removed — get new code only from Mini App.
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
            if (data == CallbackData.ProfileClose)
                await HandleProfileCloseAsync(bot, chatId.Value, messageId.Value, update.CallbackQuery.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Callback handling failed. Data={Data}", data);
            await bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, BotMessages.Get("ServiceError", null), showAlert: true, cancellationToken: ct);
        }
    }

    private async Task HandleProfileCloseAsync(ITelegramBotClient bot, long chatId, int messageId, string callbackQueryId, CancellationToken ct)
    {
        await bot.AnswerCallbackQueryAsync(callbackQueryId, cancellationToken: ct);
        await bot.DeleteMessageAsync(chatId, messageId, ct);
    }
}
