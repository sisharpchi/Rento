using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rento.TelegramBot.Services;

/// <summary>
/// Handles main menu and lang menu text messages: SMS kod olish, Profil, Til, Orqaga, O'zbekcha, Русский, English.
/// </summary>
public class MenuHandler
{
    private readonly IRentoApiClient _apiClient;
    private readonly ILogger<MenuHandler> _logger;

    public MenuHandler(IRentoApiClient apiClient, ILogger<MenuHandler> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task HandleAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message?.From is not { } from || update.Message.Text is not { } text)
            return;

        var chatId = update.Message.Chat.Id;
        var telegramUserId = from.Id;

        if (text == BotMessages.ButtonSmsCode)
        {
            await HandleSmsCodeAsync(bot, chatId, telegramUserId, null, ct);
            return;
        }

        if (text == BotMessages.ButtonProfile)
        {
            await HandleProfileAsync(bot, chatId, telegramUserId, null, ct);
            return;
        }

        if (text == BotMessages.ButtonLang)
        {
            await SendLangMenuAsync(bot, chatId, telegramUserId, ct);
            return;
        }

        if (text == BotMessages.ButtonBack)
        {
            await bot.SendTextMessageAsync(
                chatId,
                BotMessages.Get("ChooseMenuHint", null),
                replyMarkup: Keyboards.GetMainMenu(),
                cancellationToken: ct);
            return;
        }

        if (text == BotMessages.LangLabelUz || text == BotMessages.LangLabelRu || text == BotMessages.LangLabelEn)
        {
            var lang = text == BotMessages.LangLabelUz ? BotMessages.LangUz
                : text == BotMessages.LangLabelRu ? BotMessages.LangRu
                : BotMessages.LangEn;
            var ok = await _apiClient.SetLanguageAsync(telegramUserId, lang, ct);
            if (!ok)
                _logger.LogWarning("SetLanguage failed for TelegramUserId={TelegramUserId}", telegramUserId);
            await bot.SendTextMessageAsync(
                chatId,
                BotMessages.Get("LanguageSet", lang),
                replyMarkup: Keyboards.GetMainMenu(),
                cancellationToken: ct);
        }
    }

    /// <summary>
    /// SMS kod olish: if no phone ask for phone; else send code message with "Yangi kod olish" inline. lang for i18n.
    /// </summary>
    public async Task HandleSmsCodeAsync(ITelegramBotClient bot, long chatId, long telegramUserId, string? lang, CancellationToken ct)
    {
        var profile = await _apiClient.GetProfileAsync(telegramUserId, ct);
        if (profile == null || string.IsNullOrWhiteSpace(profile.PhoneNumber))
        {
            await bot.SendTextMessageAsync(
                chatId,
                BotMessages.Get("NoCodeYet", lang),
                replyMarkup: Keyboards.GetRequestPhone(),
                cancellationToken: ct);
            return;
        }

        var result = await _apiClient.GetCodeForBotAsync(telegramUserId, ct);
        if (result == null)
        {
            await bot.SendTextMessageAsync(chatId, BotMessages.Get("ServiceError", lang), cancellationToken: ct);
            return;
        }

        var codeText = string.Format(BotMessages.Get("CodeSentFormat", lang), result.Code);
        var inline = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(BotMessages.NewCodeButton, CallbackData.NewCode));
        await bot.SendTextMessageAsync(chatId, codeText, replyMarkup: inline, cancellationToken: ct);
    }

    /// <summary>
    /// Profil: send profile text and inline X button to close. lang for i18n.
    /// </summary>
    public async Task HandleProfileAsync(ITelegramBotClient bot, long chatId, long telegramUserId, string? lang, CancellationToken ct)
    {
        var profile = await _apiClient.GetProfileAsync(telegramUserId, ct);
        if (profile == null)
        {
            await bot.SendTextMessageAsync(chatId, BotMessages.Get("ServiceError", lang), cancellationToken: ct);
            return;
        }

        var firstName = profile.FirstName ?? "—";
        var lastName = profile.LastName ?? "—";
        var phone = string.IsNullOrWhiteSpace(profile.PhoneNumber) ? BotMessages.Get("ProfileMiniAppHint", lang) : profile.PhoneNumber;
        var profileText = string.Format(BotMessages.Get("ProfileFormat", lang ?? profile.Language), firstName, lastName, profile.TelegramId, phone);
        var closeButton = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(BotMessages.ProfileCloseButton, CallbackData.ProfileClose));
        await bot.SendTextMessageAsync(chatId, profileText, replyMarkup: closeButton, cancellationToken: ct);
    }

    /// <summary>
    /// Send lang submenu (O'zbekcha, Русский, English, Orqaga).
    /// </summary>
    public async Task SendLangMenuAsync(ITelegramBotClient bot, long chatId, long telegramUserId, CancellationToken ct)
    {
        var profile = await _apiClient.GetProfileAsync(telegramUserId, ct);
        var lang = profile?.Language;
        await bot.SendTextMessageAsync(
            chatId,
            BotMessages.Get("LanguageChoose", lang),
            replyMarkup: Keyboards.GetLangMenu(),
            cancellationToken: ct);
    }
}
