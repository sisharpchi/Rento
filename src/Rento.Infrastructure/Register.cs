using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using OpenIddict.EntityFrameworkCore;
using Rento.Core.Entities;
using Rento.Core.Persistence;
using Rento.Infrastructure.Data;
using Rento.Infrastructure.Persistence;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Rento.Infrastructure;

public static class Register
{
    private const string DefaultConnectionSection = "DefaultConnection";

    private static void AddMainDatabase(
        IServiceCollection services,
        IConfiguration configuration,
        string connectionSection,
        Action<DbContextOptionsBuilder>? optionsAction = null)
    {
        var connectionString = configuration.GetConnectionString(connectionSection)
            ?? "Host=localhost;Port=5432;Database=rento;Username=postgres;Password=postgres";

        services.AddDbContext<MainDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseOpenIddict<OpenIdApplication, OpenIdAuthorization, OpenIdScope, OpenIdToken, long>();
            optionsAction?.Invoke(options);
        });
    }

    private static void AddIdentity(
        IServiceCollection services,
        Action<IdentityBuilder>? configureIdentity = null)
    {
        var identityBuilder = services.AddIdentity<User, UserRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<MainDbContext>()
            .AddDefaultTokenProviders();

        configureIdentity?.Invoke(identityBuilder);
    }

    /// <summary>
    /// Adds Infrastructure layer: Main database, Identity, repositories and Unit of Work.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="configureIdentity">Optional Identity builder configuration.</param>
    /// <param name="optionsAction">Optional DbContext options configuration (e.g. interceptors).</param>
    /// <param name="connectionSection">Configuration key for connection string. Default: "DefaultConnection".</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IdentityBuilder>? configureIdentity = null,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        string connectionSection = DefaultConnectionSection)
    {
        AddIdentity(services, configureIdentity);
        AddMainDatabase(services, configuration, connectionSection, optionsAction);
        services.AddOpenIddictServer();
        services.AddHostedService<HostedServices.OpenIddictSeederService>();

        services.AddScoped<IMainRepository, MainRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork<MainDbContext>>();
        services.AddScoped<IRepository>(sp => sp.GetRequiredService<IMainRepository>());

        return services;
    }

    public static IServiceCollection AddOpenIddictServer(this IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<MainDbContext>()
                    .ReplaceDefaultEntities<OpenIdApplication, OpenIdAuthorization, OpenIdScope, OpenIdToken, long>();
            })
            .AddServer(options =>
            {
                options.RegisterScopes(Scopes.OfflineAccess);
                options.SetTokenEndpointUris("security/oauth/token")
                    .SetRevocationEndpointUris("security/oauth/revoke");

                options.AllowClientCredentialsFlow()
                    .AllowPasswordFlow()
                    .AllowRefreshTokenFlow();

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .DisableTransportSecurityRequirement()
                    .EnableTokenEndpointPassthrough();
                    //.EnableRevocationEndpointPassthrough();

                options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));
                options.SetAccessTokenLifetime(TimeSpan.FromHours(24));
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }
}
