/*
 * CleanCut Order Processing Host
 * ===============================
 * 
 * Background worker service responsible for asynchronous order processing.
 * 
 * RESPONSIBILITIES:
 * -----------------
 * 1. ORDER EVENT PUBLISHING - Publishes order-created events to RabbitMQ message queue
 * 2. EMAIL NOTIFICATIONS - Sends order confirmation emails to customers via SMTP
 * 3. RETRY HANDLING - Processes failed operations from Redis-backed retry queue
 * 4. RELIABLE DELIVERY - Ensures messages are published and emails sent even if initial attempt fails
 * 
 * ARCHITECTURE:
 * -------------
 * • Runs as a standalone .NET Worker Service (separate from API/Web apps)
 * • Uses EmailAndRabbitWorker background service for queue processing
 * • Connects to Redis for durable retry queue storage
 * • Connects to RabbitMQ for event publishing
 * • Connects to SMTP server (MailHog in dev) for email delivery
 * 
 * CONFIGURATION:
 * --------------
 * Requires appsettings.Development.json with:
 * • ConnectionStrings:Redis - Redis connection for retry queue
 * • RabbitMQ:* - RabbitMQ connection settings (host, port, credentials, exchange)
 * • Smtp:* - SMTP server settings (host, port, credentials)
 * 
 * STARTUP ORDER (Aspire):
 * -----------------------
 * 1. Redis container starts
 * 2. RabbitMQ container starts
 * 3. MailHog container starts
 * 4. This worker service starts (depends on all three)
 * 
 * See: EmailAndRabbitWorker in CleanCut.Infrastructure.BackgroundServices
 */

using CleanCut.Infrastructure.BackgroundServices.Extensions;
using CleanCut.OrderProcessingHost;
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

// Add background services from infrastructure (EmailAndRabbitWorker)
CleanCut.Infrastructure.BackgroundServices.Extensions.ServiceCollectionExtensions.AddBackgroundServices(builder.Services, builder.Configuration);

// Register the placeholder Worker so its ExecuteAsync runs (keeps the template heartbeat)
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Log startup information
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("CleanCut Order Processing Host starting up");
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
    logger.LogInformation("CleanCut Order Processing Host shutting down");
}
