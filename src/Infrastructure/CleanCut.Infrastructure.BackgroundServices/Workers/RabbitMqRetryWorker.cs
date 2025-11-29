using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanCut.Infrastructure.BackgroundServices.Workers;

/// <summary>
/// Background worker that periodically retries failed RabbitMQ publishes.
/// For this demo it will poll a simple in-memory retry queue provided by IRabbitMqRetryQueue (implemented below as a simple service).
/// </summary>
public class RabbitMqRetryWorker : BackgroundService
{
    private readonly IRabbitMqRetryQueue _retryQueue;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<RabbitMqRetryWorker> _logger;

    public RabbitMqRetryWorker(IRabbitMqRetryQueue retryQueue, IRabbitMqPublisher publisher, ILogger<RabbitMqRetryWorker> logger)
    {
        _retryQueue = retryQueue;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMqRetryWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var order = await _retryQueue.DequeueAsync(stoppingToken);
                if (order == null)
                {
                    // No queued item, wait a bit
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                var success = await _publisher.TryPublishOrderCreatedAsync(order, stoppingToken);
                if (!success)
                {
                    // Re-enqueue with backoff
                    await _retryQueue.EnqueueAsync(order, stoppingToken);
                    _logger.LogWarning("Re-enqueued order {OrderId} for retry", order.Id);
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                else
                {
                    _logger.LogInformation("Successfully retried publish for order {OrderId}", order.Id);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RabbitMqRetryWorker loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("RabbitMqRetryWorker stopping");
    }
}
