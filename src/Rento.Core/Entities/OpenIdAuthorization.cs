using OpenIddict.EntityFrameworkCore.Models;

namespace Rento.Core.Entities;

public class OpenIdAuthorization : OpenIddictEntityFrameworkCoreAuthorization<long, OpenIdApplication, OpenIdToken>
{
}
