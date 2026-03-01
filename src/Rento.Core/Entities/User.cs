using Microsoft.AspNetCore.Identity;
using Rento.Core.Entities.Common;

namespace Rento.Core.Entities;

public class User : IdentityUser, IEntity<string>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Code { get; set; }
    /// <summary>
    /// When the current <see cref="Code"/> expires (UTC). Code is valid for 2 minutes.
    /// </summary>
    public DateTimeOffset? CodeExpiresAtUtc { get; set; }
    /// <summary>
    /// Telegram user id — Mini App dan keladi, Bot /start da shu orqali kod yuboradi.
    /// </summary>
    public long? TelegramId { get; set; }
    public string FullName => LastName + " " + FirstName;
}
