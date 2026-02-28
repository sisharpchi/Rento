var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.Rento_AppHost_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
