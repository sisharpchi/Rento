using Microsoft.Extensions.DependencyInjection;
using Rento.Application.Services;
using Rento.Contracts.Services;

namespace Rento.Application;

/// <summary>
/// Application layer service registrations.
/// </summary>
public static class Register
{
    /// <summary>
    /// Registers application services (e.g. ITelegramAuthService, future IAuthService, etc.).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITelegramAuthService, TelegramAuthService>();
        return services;
    }
}
