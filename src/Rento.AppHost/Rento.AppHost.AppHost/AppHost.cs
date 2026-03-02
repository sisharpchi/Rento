var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisCommander();

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();
var rentoDb = postgres.AddDatabase("DefaultConnection");

var apiService = builder.AddProject<Projects.Rento_AppHost_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(rentoDb)
    .WaitFor(rentoDb);

var telegramBot = builder.AddProject<Projects.Rento_TelegramBot>("telegrambot")
    .WithReference(apiService);

builder.Build().Run();
