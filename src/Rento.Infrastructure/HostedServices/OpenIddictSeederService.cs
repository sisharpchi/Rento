using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Rento.Infrastructure.HostedServices;

public class OpenIddictSeederService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OpenIddictSeederService> _logger;

    public OpenIddictSeederService(IServiceProvider serviceProvider, ILogger<OpenIddictSeederService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        await SeedWebClientAsync(applicationManager, stoppingToken);

        _logger.LogInformation("OpenIddict clients seeded successfully.");
    }

    private async Task SeedWebClientAsync(IOpenIddictApplicationManager applicationManager, CancellationToken ct)
    {
        const string clientId = "rento-web-client";

        if (await applicationManager.FindByClientIdAsync(clientId, ct) is not null)
        {
            _logger.LogInformation("Client '{ClientId}' already exists.", clientId);
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = "Rento Web UI",
            ConsentType = ConsentTypes.Implicit,
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Revocation,
                Permissions.GrantTypes.Password,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.ResponseTypes.Token,
                Scopes.OfflineAccess,
            }
        };

        await applicationManager.CreateAsync(descriptor, ct);
        _logger.LogInformation("Created OpenIddict client: {ClientId}", clientId);
    }
}
