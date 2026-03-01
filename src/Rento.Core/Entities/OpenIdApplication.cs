using OpenIddict.EntityFrameworkCore.Models;

namespace Rento.Core.Entities;

public class OpenIdApplication : OpenIddictEntityFrameworkCoreApplication<long, OpenIdAuthorization, OpenIdToken>
{
}
