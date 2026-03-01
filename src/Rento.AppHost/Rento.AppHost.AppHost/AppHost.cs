var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres");
var rentoDb = postgres.AddDatabase("DefaultConnection");

var apiService = builder.AddProject<Projects.Rento_AppHost_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(rentoDb)
    .WaitFor(rentoDb);

builder.Build().Run();
