using CleanCut.Infrastructure.BackgroundServices.Extensions;
using CleanCut.ProductExportHost;
using CleanCut.Infrastructure.Data;
using CleanCut.Application;
using CleanCut.Infrastructure.Shared;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Register application & infrastructure services the host needs (DbContext, repositories, AutoMapper)
builder.Services.AddDataInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Register shared infra (SMTP, RabbitMQ, etc.) so background services have required dependencies
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Add background services from infrastructure (explicit static call to avoid ambiguity)
CleanCut.Infrastructure.BackgroundServices.Extensions.ServiceCollectionExtensions.AddBackgroundServices(builder.Services, builder.Configuration);

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("CleanCut Product Export Host starting up");
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
    logger.LogInformation("CleanCut Product Export Host shutting down");
}
