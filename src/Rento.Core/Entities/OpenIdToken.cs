using OpenIddict.EntityFrameworkCore.Models;

namespace Rento.Core.Entities;

public class OpenIdToken : OpenIddictEntityFrameworkCoreToken<long, OpenIdApplication, OpenIdAuthorization>
{
}
