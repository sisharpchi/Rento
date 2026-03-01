using Microsoft.Extensions.DependencyInjection;
using Rento.TelegramBot.Configuration;
using Rento.TelegramBot.HostedServices;
using Rento.TelegramBot.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<TelegramBotOptions>(builder.Configuration.GetSection(TelegramBotOptions.SectionName));
builder.Services.Configure<RentoApiOptions>(builder.Configuration.GetSection(RentoApiOptions.SectionName));

var botToken = builder.Configuration["TelegramBot:BotToken"] ?? "";
if (string.IsNullOrEmpty(botToken))
    Console.WriteLine("WARNING: TelegramBot:BotToken is not set. Set it in appsettings.json or environment.");

builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
var rentoApiBaseUrl = builder.Configuration["RentoApi:BaseUrl"] ?? "";
builder.Services.AddHttpClient("RentoApi", (sp, client) =>
{
    // Standalone: use RentoApi:BaseUrl (e.g. https://localhost:5001). Under AppHost service discovery is used.
    if (!string.IsNullOrWhiteSpace(rentoApiBaseUrl))
        client.BaseAddress = new Uri(rentoApiBaseUrl);
    else
        client.BaseAddress = new Uri("https+http://apiservice");
});
builder.Services.AddScoped<IRentoApiClient, RentoApiClient>();
builder.Services.AddScoped<StartHandler>();
builder.Services.AddScoped<CallbackQueryHandler>();
builder.Services.AddHostedService<TelegramBotWorker>();

var host = builder.Build();
await host.RunAsync();
