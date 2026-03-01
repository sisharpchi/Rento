using Microsoft.EntityFrameworkCore;
using Rento.Application;
using Rento.Infrastructure;
using Rento.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddControllers();

var app = builder.Build();

// Apply pending EF Core migrations at startup (creates/updates DB schema).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MainDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Database migration failed. Ensure PostgreSQL is running and ConnectionStrings:DefaultConnection is correct. " +
            "When using AppHost, start the AppHost project so Postgres starts first.");
        throw;
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
