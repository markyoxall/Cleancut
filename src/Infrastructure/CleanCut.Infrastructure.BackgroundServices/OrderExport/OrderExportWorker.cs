using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CleanCut.Infrastructure.BackgroundServices.OrderExport;

public class OrderExportConfiguration
{
    public int IntervalMinutes { get; set; } = 5;
    public int InitialDelaySeconds { get; set; } = 10;
    public bool ContinueOnError { get; set; } = true;
}

public class OrderExportWorker : BackgroundService
{
    private readonly ILogger<OrderExportWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly OrderExportConfiguration _settings;

    public OrderExportWorker(ILogger<OrderExportWorker> logger, IServiceProvider serviceProvider, IOptions<OrderExportConfiguration> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderExportWorker starting up. Will run every {Interval} minutes.", _settings.IntervalMinutes);

        await Task.Delay(TimeSpan.FromSeconds(_settings.InitialDelaySeconds), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IOrderExportService>();
                await svc.ExportNewOrdersAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OrderExportWorker stopping due to cancellation");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during order export");
                if (!_settings.ContinueOnError)
                {
                    _logger.LogCritical("ContinueOnError disabled, stopping worker");
                    break;
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_settings.IntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("OrderExportWorker has stopped");
    }
}
