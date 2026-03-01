namespace Rento.TelegramBot.Configuration;

public class TelegramBotOptions
{
    public const string SectionName = "TelegramBot";

    public string BotToken { get; set; } = "";
    public string SecretKey { get; set; } = "";
}
