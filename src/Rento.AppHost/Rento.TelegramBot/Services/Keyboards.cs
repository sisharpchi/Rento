using Telegram.Bot.Types.ReplyMarkups;

namespace Rento.TelegramBot.Services;

/// <summary>
/// Shared reply keyboards for the bot. Pass lang so labels match user language.
/// </summary>
public static class Keyboards
{
    /// <summary>
    /// Main menu: OTP kod / Profil / Til (one row). lang = user Language (uz/ru/en).
    /// </summary>
    public static ReplyKeyboardMarkup GetMainMenu(string? lang = null)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { BotMessages.Get(BotMessages.KeyButtonSmsCode, lang), BotMessages.Get(BotMessages.KeyButtonProfile, lang), BotMessages.Get(BotMessages.KeyButtonLang, lang) }
        })
        {
            ResizeKeyboard = true
        };
    }

    /// <summary>
    /// Lang submenu: O'zbekcha, Русский, English; then Orqaga. lang = user Language.
    /// </summary>
    public static ReplyKeyboardMarkup GetLangMenu(string? lang = null)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { BotMessages.Get(BotMessages.KeyLangLabelUz, lang), BotMessages.Get(BotMessages.KeyLangLabelRu, lang), BotMessages.Get(BotMessages.KeyLangLabelEn, lang) },
            new KeyboardButton[] { BotMessages.Get(BotMessages.KeyButtonBack, lang) }
        })
        {
            ResizeKeyboard = true
        };
    }

    /// <summary>
    /// Request contact for phone. lang = user Language.
    /// </summary>
    public static ReplyKeyboardMarkup GetRequestPhone(string? lang = null)
    {
        return new ReplyKeyboardMarkup(KeyboardButton.WithRequestContact(BotMessages.Get(BotMessages.KeySendPhoneButton, lang)))
        {
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        };
    }
}
