using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanCut.Infrastructure.BackgroundServices.Workers;

/// <summary>
/// Background worker that sends emails and publishes order-created messages to RabbitMQ.
/// It consumes a lightweight in-memory queue (suitable for demo). For production, replace with durable queue.
/// </summary>
public class EmailAndRabbitWorker : BackgroundService
{
    private readonly IEmailSender _emailSender;
    private readonly IRabbitMqPublisher? _publisher;
    private readonly IRabbitMqRetryQueue _retryQueue;
    private readonly ILogger<EmailAndRabbitWorker> _logger;

    public EmailAndRabbitWorker(IEmailSender emailSender, IRabbitMqPublisher? publisher, IRabbitMqRetryQueue retryQueue, ILogger<EmailAndRabbitWorker> logger)
    {
        _emailSender = emailSender;
        _publisher = publisher;
        _retryQueue = retryQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailAndRabbitWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var order = await _retryQueue.DequeueAsync(stoppingToken);
                if (order == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                    continue;
                }

                // Send email (best-effort)
                if (!string.IsNullOrEmpty(order.CustomerEmail))
                {
                    var body = $"<p>Thank you for your order <strong>{order.OrderNumber}</strong>.</p><p>Amount: {order.TotalAmount:C}</p>";
                    await _emailSender.SendEmailAsync(order.CustomerEmail, "Order received", body, stoppingToken);
                }

                // Publish to RabbitMQ; if fails, re-enqueue
                if (_publisher != null)
                {
                    var ok = await _publisher.TryPublishOrderCreatedAsync(order, stoppingToken);
                    if (!ok)
                    {
                        await _retryQueue.EnqueueAsync(order, stoppingToken);
                        _logger.LogWarning("Publish failed, re-enqueued order {OrderId}", order.Id);
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation("Published order {OrderId} to RabbitMQ", order.Id);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EmailAndRabbitWorker loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("EmailAndRabbitWorker stopping");
    }
}
