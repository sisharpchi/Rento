namespace Rento.TelegramBot.Configuration;

/// <summary>
/// API client options. BaseUrl is not used when running under Aspire AppHost:
/// HttpClient is configured with service discovery address "http://apiservice" in Program.cs.
/// </summary>
public class RentoApiOptions
{
    public const string SectionName = "RentoApi";

    /// <summary>Optional override when not using Aspire service discovery (e.g. standalone run).</summary>
    public string BaseUrl { get; set; } = "";
}
