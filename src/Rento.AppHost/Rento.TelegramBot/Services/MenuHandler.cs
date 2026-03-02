using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Rento.TelegramBot.Services;

/// <summary>
/// Handles main menu and lang menu text messages: Kodni ko'rish, Profil, Til, Orqaga, O'zbekcha, Русский, English.
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
        var profile = await _apiClient.GetProfileAsync(telegramUserId, ct);
        var lang = profile?.Language;

        if (BotMessages.MatchesButton(BotMessages.KeyButtonViewCode, text))
        {
            await HandleViewCodeAsync(bot, chatId, telegramUserId, lang, ct);
            return;
        }

        if (BotMessages.MatchesButton(BotMessages.KeyButtonProfile, text))
        {
            await HandleProfileAsync(bot, chatId, telegramUserId, lang, ct);
            return;
        }

        if (BotMessages.MatchesButton(BotMessages.KeyButtonLang, text))
        {
            await SendLangMenuAsync(bot, chatId, telegramUserId, ct);
            return;
        }

        if (BotMessages.MatchesButton(BotMessages.KeyButtonBack, text))
        {
            await bot.SendTextMessageAsync(
                chatId,
                BotMessages.Get("ChooseMenuHint", lang),
                replyMarkup: Keyboards.GetMainMenu(lang),
                cancellationToken: ct);
            return;
        }

        if (BotMessages.MatchesButton(BotMessages.KeyLangLabelUz, text))
        {
            await SetLangAndRespondAsync(bot, chatId, telegramUserId, BotMessages.LangUz, ct);
            return;
        }
        if (BotMessages.MatchesButton(BotMessages.KeyLangLabelRu, text))
        {
            await SetLangAndRespondAsync(bot, chatId, telegramUserId, BotMessages.LangRu, ct);
            return;
        }
        if (BotMessages.MatchesButton(BotMessages.KeyLangLabelEn, text))
        {
            await SetLangAndRespondAsync(bot, chatId, telegramUserId, BotMessages.LangEn, ct);
            return;
        }
    }

    private async Task SetLangAndRespondAsync(ITelegramBotClient bot, long chatId, long telegramUserId, string lang, CancellationToken ct)
    {
        var ok = await _apiClient.SetLanguageAsync(telegramUserId, lang, ct);
        if (!ok)
            _logger.LogWarning("SetLanguage failed for TelegramUserId={TelegramUserId}", telegramUserId);
        await bot.SendTextMessageAsync(
            chatId,
            BotMessages.Get("LanguageSet", lang),
            replyMarkup: Keyboards.GetMainMenu(lang),
            cancellationToken: ct);
    }

    /// <summary>
    /// Kodni ko'rish: show current code if any (no generation); else GetCodeFromMiniApp message. No inline "Yangi kod olish".
    /// </summary>
    public async Task HandleViewCodeAsync(ITelegramBotClient bot, long chatId, long telegramUserId, string? lang, CancellationToken ct)
    {
        var result = await _apiClient.GetCodeForBotAsync(telegramUserId, ct);
        if (result != null)
        {
            var codeText = string.Format(BotMessages.Get("CodeSentFormat", lang), result.Code);
            await bot.SendTextMessageAsync(chatId, codeText, cancellationToken: ct);
            return;
        }
        await bot.SendTextMessageAsync(chatId, BotMessages.Get("GetCodeFromMiniApp", lang), cancellationToken: ct);
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
        var closeButton = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(BotMessages.Get(BotMessages.KeyProfileCloseButton, lang), CallbackData.ProfileClose));
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
            replyMarkup: Keyboards.GetLangMenu(lang),
            cancellationToken: ct);
    }
}
