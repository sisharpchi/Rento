using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rento.TelegramBot.HostedServices;

public sealed class TelegramBotWorker : BackgroundService
{
    private readonly ILogger<TelegramBotWorker> _logger;
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;

    public TelegramBotWorker(
        ILogger<TelegramBotWorker> logger,
        ITelegramBotClient bot,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _bot = bot;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: new Telegram.Bot.Polling.ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
            },
            cancellationToken: stoppingToken);

        _logger.LogInformation("Telegram Bot is listening. Press Ctrl+C to stop.");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        try
        {
            if (update.Message is { } message)
            {
                if (message.Text?.StartsWith("/start", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var startHandler = scope.ServiceProvider.GetRequiredService<Services.StartHandler>();
                    await startHandler.HandleAsync(botClient, update, cancellationToken);
                }
                else if (IsPhoneMessage(message))
                {
                    var startHandler = scope.ServiceProvider.GetRequiredService<Services.StartHandler>();
                    await startHandler.HandlePhoneMessageAsync(botClient, update, cancellationToken);
                }
                return;
            }

            if (update.CallbackQuery is { })
            {
                var callbackHandler = scope.ServiceProvider.GetRequiredService<Services.CallbackQueryHandler>();
                await callbackHandler.HandleAsync(botClient, update, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handle update failed. UpdateId={UpdateId}", update.Id);
            if (update.Message?.Chat.Id is { } chatId)
            {
                try
                {
                    await botClient.SendTextMessageAsync(chatId, Services.BotMessages.ServiceError, cancellationToken: cancellationToken);
                }
                catch { /* best effort */ }
            }
            else if (update.CallbackQuery?.Id is { } callbackId)
            {
                try
                {
                    await botClient.AnswerCallbackQueryAsync(callbackId, Services.BotMessages.ServiceError, cancellationToken: cancellationToken);
                }
                catch { /* best effort */ }
            }
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram polling error");
        return Task.CompletedTask;
    }

    private static bool IsPhoneMessage(Message message)
    {
        if (message.Contact?.PhoneNumber != null) return true;
        if (string.IsNullOrWhiteSpace(message.Text)) return false;
        var digits = message.Text.Count(c => c == '+' || char.IsDigit(c));
        return digits >= 9;
    }
}
