namespace Rento.TelegramBot.Services;

/// <summary>
/// User-facing message strings with i18n (uz, ru, en).
/// Use Get(key, lang) for locale-aware text; lang defaults to "uz" if null/empty.
/// </summary>
public static class BotMessages
{
    public const string LangUz = "uz";
    public const string LangRu = "ru";
    public const string LangEn = "en";

    /// <summary>Keys for menu/reply buttons (use Get(key, lang) for label).</summary>
    public const string KeyButtonViewCode = "ButtonViewCode";
    public const string KeyButtonProfile = "ButtonProfile";
    public const string KeyButtonLang = "ButtonLang";
    public const string KeyButtonBack = "ButtonBack";
    public const string KeyLangLabelUz = "LangLabelUz";
    public const string KeyLangLabelRu = "LangLabelRu";
    public const string KeyLangLabelEn = "LangLabelEn";
    public const string KeyNewCodeButton = "NewCodeButton";
    public const string KeySendPhoneButton = "SendPhoneButton";
    public const string KeyProfileCloseButton = "ProfileCloseButton";

    private static readonly string[] MenuButtonKeys = [KeyButtonViewCode, KeyButtonProfile, KeyButtonLang, KeyButtonBack, KeyLangLabelUz, KeyLangLabelRu, KeyLangLabelEn];

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        ["Welcome"] = new() { [LangUz] = "Xush kelibsiz!", [LangRu] = "Добро пожаловать!", [LangEn] = "Welcome!" },
        ["AskPhone"] = new() { [LangUz] = "Telefon raqamingizni yuboring (kontakt yoki +998901234567 formatida):", [LangRu] = "Отправьте номер телефона (контакт или +998901234567):", [LangEn] = "Send your phone number (contact or +998901234567):" },
        [KeySendPhoneButton] = new() { [LangUz] = "Raqamni yuborish", [LangRu] = "Отправить номер", [LangEn] = "Send number" },
        ["PhoneSaved"] = new() { [LangUz] = "Rahmat, raqam saqlandi.", [LangRu] = "Спасибо, номер сохранён.", [LangEn] = "Thank you, number saved." },
        ["TelegramUserIdNotFound"] = new() { [LangUz] = "Telegram user id topilmadi.", [LangRu] = "ID пользователя Telegram не найден.", [LangEn] = "Telegram user id not found." },
        ["NoCodeYet"] = new() { [LangUz] = "Siz uchun hozircha kod yo'q. Avval telefon raqamingizni kiriting (Profil yoki /start).", [LangRu] = "Код пока не создан. Сначала укажите номер телефона.", [LangEn] = "No code yet. Please enter your phone number first (Profile or /start)." },
        ["ServiceError"] = new() { [LangUz] = "Xizmat vaqtincha ishlamayapti. Keyinroq urinib ko'ring.", [LangRu] = "Сервис временно недоступен. Попробуйте позже.", [LangEn] = "Service temporarily unavailable. Please try again later." },
        ["CodeSentFormat"] = new() { [LangUz] = "Sizning parolingiz: {0}\n\nBu parolni Mini App'da kirish uchun ishlating. Hech kimga bermang.", [LangRu] = "Ваш пароль: {0}\n\nИспользуйте его для входа в Mini App. Никому не передавайте.", [LangEn] = "Your code: {0}\n\nUse it to sign in to the Mini App. Do not share with anyone." },
        ["ProfileFormat"] = new() { [LangUz] = "Profil:\nIsm: {0}\nFamiliya: {1}\nTelegram ID: {2}\nTelefon: {3}", [LangRu] = "Профиль:\nИмя: {0}\nФамилия: {1}\nTelegram ID: {2}\nТелефон: {3}", [LangEn] = "Profile:\nFirst name: {0}\nLast name: {1}\nTelegram ID: {2}\nPhone: {3}" },
        ["ProfileMiniAppHint"] = new() { [LangUz] = "Telefon raqamini to'ldiring (Mini Appda kod olish yoki /start).", [LangRu] = "Укажите номер телефона (получить код в Mini App или /start).", [LangEn] = "Add phone number (get code in Mini App or /start)." },
        ["LanguageChoose"] = new() { [LangUz] = "Tilni tanlang:", [LangRu] = "Выберите язык:", [LangEn] = "Choose language:" },
        ["LanguageSet"] = new() { [LangUz] = "Til o'zgartirildi.", [LangRu] = "Язык изменён.", [LangEn] = "Language changed." },
        ["OldCodeStillValid"] = new() { [LangUz] = "Eski kod hali amalda. 2 daqiqadan keyin yangi kod olish mumkin.", [LangRu] = "Старый код ещё действителен. Новый код можно получить через 2 минуты.", [LangEn] = "Current code is still valid. You can get a new code in 2 minutes." },
        ["ChooseMenuHint"] = new() { [LangUz] = "Quyidagi tugmalardan birini tanlang.", [LangRu] = "Выберите одну из кнопок ниже.", [LangEn] = "Choose one of the buttons below." },
        ["GetCodeFromMiniApp"] = new() { [LangUz] = "Kod yo'q. Avval Mini Appda telefon raqamingizni kiriting va \"Kod olish\" tugmasini bosing.", [LangRu] = "Кода нет. Сначала введите номер в Mini App и нажмите «Получить код».", [LangEn] = "No code. Enter your phone number in the Mini App and tap \"Get code\" first." },
        [KeyButtonViewCode] = new() { [LangUz] = "Kodni ko'rish", [LangRu] = "Посмотреть код", [LangEn] = "View code" },
        [KeyButtonProfile] = new() { [LangUz] = "Profil", [LangRu] = "Профиль", [LangEn] = "Profile" },
        [KeyButtonLang] = new() { [LangUz] = "Til", [LangRu] = "Язык", [LangEn] = "Language" },
        [KeyButtonBack] = new() { [LangUz] = "Orqaga", [LangRu] = "Назад", [LangEn] = "Back" },
        [KeyLangLabelUz] = new() { [LangUz] = "O'zbekcha", [LangRu] = "O'zbekcha", [LangEn] = "O'zbekcha" },
        [KeyLangLabelRu] = new() { [LangUz] = "Русский", [LangRu] = "Русский", [LangEn] = "Русский" },
        [KeyLangLabelEn] = new() { [LangUz] = "English", [LangRu] = "English", [LangEn] = "English" },
        [KeyNewCodeButton] = new() { [LangUz] = "Yangi kod olish", [LangRu] = "Получить новый код", [LangEn] = "Get new code" },
        [KeyProfileCloseButton] = new() { [LangUz] = "✕", [LangRu] = "✕", [LangEn] = "✕" },
    };

    /// <summary>
    /// Get localized message. lang null/empty defaults to uz.
    /// </summary>
    public static string Get(string key, string? lang)
    {
        var l = string.IsNullOrWhiteSpace(lang) ? LangUz : lang.Trim().ToLowerInvariant();
        if (Messages.TryGetValue(key, out var dict) && dict.TryGetValue(l, out var text))
            return text;
        if (dict != null && dict.TryGetValue(LangUz, out var uzText))
            return uzText;
        return key;
    }

    /// <summary>
    /// Returns true if text equals the button label in any language (for routing).
    /// </summary>
    public static bool MatchesButton(string buttonKey, string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return Get(buttonKey, LangUz) == text || Get(buttonKey, LangRu) == text || Get(buttonKey, LangEn) == text;
    }

    /// <summary>
    /// All possible menu button texts in any language (for IsMenuMessage).
    /// </summary>
    public static HashSet<string> GetAllMenuButtonTexts()
    {
        var set = new HashSet<string>();
        foreach (var key in MenuButtonKeys)
        {
            set.Add(Get(key, LangUz));
            set.Add(Get(key, LangRu));
            set.Add(Get(key, LangEn));
        }
        return set;
    }

    // Convenience: default uz
    public static string Welcome => Get("Welcome", LangUz);
    public static string AskPhone => Get("AskPhone", LangUz);
    public static string SendPhoneButton => Get(KeySendPhoneButton, LangUz);
    public static string PhoneSaved => Get("PhoneSaved", LangUz);
    public static string TelegramUserIdNotFound => Get("TelegramUserIdNotFound", LangUz);
    public static string NoCodeYet => Get("NoCodeYet", LangUz);
    public static string ServiceError => Get("ServiceError", LangUz);
    public static string CodeSentFormat => Get("CodeSentFormat", LangUz);
    public static string ProfileFormat => Get("ProfileFormat", LangUz);
    public static string ProfileMiniAppHint => Get("ProfileMiniAppHint", LangUz);
    public static string LanguageChoose => Get("LanguageChoose", LangUz);
    public static string LanguageSet => Get("LanguageSet", LangUz);
    public static string OldCodeStillValid => Get("OldCodeStillValid", LangUz);
    public static string ChooseMenuHint => Get("ChooseMenuHint", LangUz);
    public static string GetCodeFromMiniApp => Get("GetCodeFromMiniApp", LangUz);
}
