namespace Rento.TelegramBot.Services;

/// <summary>
/// User-facing message strings (DRY).
/// </summary>
public static class BotMessages
{
    public const string Welcome = "Assalomu alaykum! Quyidagi tugmalardan birini tanlang.";
    public const string AskPhone = "Telefon raqamingizni yuboring (kontakt yoki +998901234567 formatida):";
    public const string SendPhoneButton = "Raqamni yuborish";
    public const string PhoneSaved = "Rahmat, raqam saqlandi.";
    public const string TelegramUserIdNotFound = "Telegram user id topilmadi.";
    public const string NoCodeYet = "Siz uchun hozircha kod yo'q. Avval Mini App orqali telefon raqamingizni kiriting va kod so'rang.";
    public const string CodeRequestError = "Kod olishda xato. Keyinroq urinib ko'ring.";
    public const string ServiceError = "Xizmat vaqtincha ishlamayapti. Keyinroq urinib ko'ring.";
    public const string CodeSentFormat = "Sizning parolingiz: {0}\n\nBu parolni Mini App'da kirish uchun ishlating. Hech kimga bermang.";
    public const string ProfileFormat = "Profil ma'lumotlari:\nIsm: {0}\nFamiliya: {1}\nTelegram ID: {2}\nTelefon: {3}";
    public const string ProfileMiniAppHint = "Telefon raqamini Mini App orqali to'ldiring.";
    public const string LanguageChoose = "Tilni tanlang / Выберите язык / Choose language:";
    public const string LanguageSet = "Til o'zgartirildi.";
    public const string ButtonCode = "SMS kod olish";
    public const string ButtonProfile = "Profil ma'lumotlari";
    public const string ButtonLanguage = "Til tanlash";
}
