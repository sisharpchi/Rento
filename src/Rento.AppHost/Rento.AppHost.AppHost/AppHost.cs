using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisCommander();

var apiService = builder.AddProject<Projects.Rento_AppHost_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");
    //.WithReference(rentoDb)
    //.WaitFor(rentoDb);

//var postgres = builder.AddPostgres("postgres")
//    .WithPgAdmin();
//var rentoDb = postgres.AddDatabase("DefaultConnection");

if (builder.Environment.IsProduction())
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection");

    var externalDb = builder.AddConnectionString(
        "DefaultConnection",
        connectionString!);

    apiService.WithReference(externalDb);
}

if (builder.Environment.IsDevelopment())
{
    var postgres = builder.AddPostgres("postgres")
        .WithDataVolume()
        .WithPgAdmin();

    var rentoDb = postgres.AddDatabase("DefaultConnection");

    apiService
        .WithReference(rentoDb)
        .WaitFor(rentoDb);
}

var telegramBot = builder.AddProject<Projects.Rento_TelegramBot>("telegrambot")
    .WithReference(apiService);

builder.Build().Run();
