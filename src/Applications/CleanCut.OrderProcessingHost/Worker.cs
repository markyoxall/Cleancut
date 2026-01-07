/*
 * Placeholder Worker Service
 * ==========================
 * 
 * NOTE: This is a placeholder/example worker that came with the .NET Worker Service template.
 * 
 * The actual order processing work is done by EmailAndRabbitWorker, which is registered via
 * AddBackgroundServices() in Program.cs.
 * 
 * This class can be safely removed, or repurposed for additional background tasks if needed.
 */

namespace CleanCut.OrderProcessingHost;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Placeholder - actual work done by EmailAndRabbitWorker
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("OrderProcessingHost heartbeat at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(60000, stoppingToken); // Every minute
        }
    }
}
