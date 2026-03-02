using Telegram.Bot.Types.ReplyMarkups;

namespace Rento.TelegramBot.Services;

/// <summary>
/// Shared reply keyboards for the bot.
/// </summary>
public static class Keyboards
{
    /// <summary>
    /// Main menu: SMS kod olish, Profil, Til (one row).
    /// </summary>
    public static ReplyKeyboardMarkup GetMainMenu()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { BotMessages.ButtonSmsCode, BotMessages.ButtonProfile, BotMessages.ButtonLang }
        })
        {
            ResizeKeyboard = true
        };
    }

    /// <summary>
    /// Lang submenu: O'zbekcha, Русский, English; then Orqaga.
    /// </summary>
    public static ReplyKeyboardMarkup GetLangMenu()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { BotMessages.LangLabelUz, BotMessages.LangLabelRu, BotMessages.LangLabelEn },
            new KeyboardButton[] { BotMessages.ButtonBack }
        })
        {
            ResizeKeyboard = true
        };
    }

    /// <summary>
    /// Request contact for phone (e.g. when no phone and user pressed SMS kod).
    /// </summary>
    public static ReplyKeyboardMarkup GetRequestPhone()
    {
        return new ReplyKeyboardMarkup(KeyboardButton.WithRequestContact(BotMessages.SendPhoneButton))
        {
            OneTimeKeyboard = true,
            ResizeKeyboard = true
        };
    }
}
