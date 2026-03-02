using Microsoft.OpenApi.Models;

namespace Rento.AppHost.ApiService.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Rento API",
                Version = "v1",
                Description = "Rento API documentation"
            });
        });
        return services;
    }

    public static WebApplication UseSwaggerUi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rento API v1");
            options.RoutePrefix = "swagger";
        });
        return app;
    }
}
