using Microsoft.AspNetCore.Identity;
using Rento.Core.Entities.Common;

namespace Rento.Core.Entities;

public class User : IdentityUser, IEntity<string>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Code { get; set; }
    public string FullName => LastName + " " + FirstName;
}
