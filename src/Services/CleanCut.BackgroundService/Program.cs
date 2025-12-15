using CleanCut.BackgroundService;
using CleanCut.BackgroundService.Configuration;
using CleanCut.BackgroundService.Services;
using CleanCut.BackgroundService.Workers;

var builder = Host.CreateApplicationBuilder(args);

// Configure settings
builder.Services.Configure<BackgroundServiceSettings>(
    builder.Configuration.GetSection(BackgroundServiceSettings.SectionName));

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Configure HttpClient for authentication
builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) =>
    {
        // Only allow this in development for self-signed certificates
        return builder.Environment.IsDevelopment();
    }
});

// Configure HttpClient for API calls
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) =>
    {
        // Only allow this in development for self-signed certificates
        return builder.Environment.IsDevelopment();
    }
});

// Register services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<ICsvExportService, CsvExportService>();

// Register the background service
builder.Services.AddHostedService<ProductExportWorker>();

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("CleanCut Background Service starting up");
logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application terminated unexpectedly");
}
finally
{
    logger.LogInformation("CleanCut Background Service shutting down");
}
