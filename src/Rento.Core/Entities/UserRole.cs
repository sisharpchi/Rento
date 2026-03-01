using Microsoft.AspNetCore.Identity;
using Rento.Core.Entities.Common;

namespace Rento.Core.Entities;

public class UserRole : IdentityRole, IEntity<string>
{
    public UserRole(string name) : base(name)
    {
        if (!name.StartsWith("app."))
            base.Name = $"app.{name}";

        DisplayName = base.Name;
    }
    public override string Id { get; set; } = Guid.NewGuid().ToString();

    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
