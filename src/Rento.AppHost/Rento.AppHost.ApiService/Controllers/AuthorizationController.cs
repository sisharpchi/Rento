using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Rento.Core.Entities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Rento.AppHost.ApiService.Controllers;

[ApiController]
[Route("security/oauth")]
public class AuthorizationController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IOpenIddictTokenManager _tokenManager;

    public AuthorizationController(
        UserManager<User> userManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IOpenIddictTokenManager tokenManager)
    {
        _userManager = userManager;
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _tokenManager = tokenManager;
    }

    [HttpPost("token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange(CancellationToken cancellationToken = default)
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            return await ExchangeClientCredentialsAsync(request, cancellationToken);
        }

        if (request.IsPasswordGrantType())
        {
            return await ExchangePasswordAsync(request, cancellationToken);
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await ExchangeRefreshTokenAsync(request, cancellationToken);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }

    [HttpPost("revoke")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Revoke(CancellationToken cancellationToken = default)
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request is null)
        {
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        }

        // OpenIddict 5.x: revocation sends "token" in form body; no IsRevocationRequest() / RevokeOpenIddictTokensAsync
        var tokenToRevoke = Request.Form["token"].ToString();
        if (string.IsNullOrEmpty(tokenToRevoke))
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "The token to revoke is required."
            });
        }

        var token = await _tokenManager.FindByReferenceIdAsync(tokenToRevoke, cancellationToken);
        if (token is not null)
        {
            await _tokenManager.TryRevokeAsync(token, cancellationToken);
        }
        // RFC 7009: always return 200 OK even if token was not found or already revoked
        return Ok();
    }

    private async Task<IActionResult> ExchangeClientCredentialsAsync(
        OpenIddictRequest request,
        CancellationToken cancellationToken)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!, cancellationToken)
            ?? throw new InvalidOperationException("The application details cannot be found in the database.");

        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);

        identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application, cancellationToken));
        identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application, cancellationToken));
        identity.SetScopes(request.GetScopes());
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangePasswordAsync(
        OpenIddictRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "Username and password are required."
            });
        }

        var user = await _userManager.FindByNameAsync(request.Username)
            ?? await _userManager.FindByEmailAsync(request.Username);

        if (user is null)
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "User not found."
            });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (user.Code == request.Password)
        {
            isPasswordValid = true;
        }

        if (!isPasswordValid)
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "Invalid password."
            });
        }

        var identity = CreatePrincipal(user, request);
        identity.SetClaim(Claims.Subject, user.Id);
        identity.SetClaim(ClaimTypes.NameIdentifier, user.Id);

        var scopes = new HashSet<string>(request.GetScopes());
        scopes.Add(Scopes.OfflineAccess);
        identity.SetScopes(scopes);
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangeRefreshTokenAsync(
        OpenIddictRequest request,
        CancellationToken cancellationToken)
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = result.Principal;

        if (principal is null || principal.Identity?.IsAuthenticated != true)
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "The refresh token is invalid."
            });
        }

        var userId = principal.GetClaim(Claims.Subject);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var identity = CreatePrincipal(user, request);
        identity.SetClaim(Claims.Subject, user.Id);
        identity.SetClaim(ClaimTypes.NameIdentifier, user.Id);
        identity.SetScopes(principal.GetScopes());
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static ClaimsIdentity CreatePrincipal(User user, OpenIddictRequest openIddictRequest)
    {
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);

        identity.SetClaim(Claims.JwtId, Guid.NewGuid().ToString());
        identity.SetClaim(Claims.Subject, user.Id);
        identity.SetClaim(Claims.GivenName, user.FirstName ?? string.Empty);
        identity.SetClaim(Claims.FamilyName, user.LastName ?? string.Empty);
        identity.SetDestinations(GetDestinations);
        identity.SetScopes(openIddictRequest.GetScopes());

        return identity;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            Claims.Name or Claims.Subject => [Destinations.AccessToken, Destinations.IdentityToken],
            _ => [Destinations.AccessToken]
        };
    }
}
