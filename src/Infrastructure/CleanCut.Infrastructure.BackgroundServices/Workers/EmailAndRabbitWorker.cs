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

                // Publish to RabbitMQ first (best-effort). If publish fails, re-enqueue and do NOT send email.
                var published = false;
                if (_publisher != null)
                {
                    try
                    {
                        published = await _publisher.TryPublishOrderCreatedAsync(order, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Exception while trying to publish order {OrderId}", order.Id);
                        published = false;
                    }

                    if (!published)
                    {
                        await _retryQueue.EnqueueAsync(order, stoppingToken);
                        _logger.LogWarning("Publish failed, re-enqueued order {OrderId}", order.Id);
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                        continue; // skip sending email until publish succeeds
                    }
                    else
                    {
                        _logger.LogInformation("Published order {OrderId} to RabbitMQ", order.Id);
                    }
                }

                // Send email only after successful publish (prevents emails being sent repeatedly if RabbitMQ is down)
                if (!string.IsNullOrEmpty(order.CustomerEmail))
                {
                    try
                    {
                        // Build an itemized HTML table for the order
                        var sb = new System.Text.StringBuilder();
                        sb.Append($"<p>Thank you for your order <strong>{order.OrderNumber}</strong>.</p>");
                        sb.Append("<table style=\"width:100%;border-collapse:collapse;\">" +
                                  "<thead><tr><th style=\"text-align:left;border-bottom:1px solid #ddd;\">Item</th><th style=\"text-align:right;border-bottom:1px solid #ddd;\">Unit</th><th style=\"text-align:right;border-bottom:1px solid #ddd;\">Qty</th><th style=\"text-align:right;border-bottom:1px solid #ddd;\">Line Total</th></tr></thead>");
                        sb.Append("<tbody>");

                        foreach (var li in order.OrderLineItems ?? new System.Collections.Generic.List<OrderLineItemInfo>())
                        {
                            sb.Append($"<tr><td style=\"padding:8px 0;\">{System.Net.WebUtility.HtmlEncode(li.ProductName)}</td>");
                            sb.Append($"<td style=\"text-align:right;padding:8px 0;\">{li.UnitPrice:C}</td>");
                            sb.Append($"<td style=\"text-align:right;padding:8px 0;\">{li.Quantity}</td>");
                            sb.Append($"<td style=\"text-align:right;padding:8px 0;\">{li.LineTotal:C}</td></tr>");
                        }

                        sb.Append("</tbody>");
                        sb.Append($"<tfoot><tr><td colspan=\"3\" style=\"text-align:right;padding-top:8px;font-weight:bold;\">Total:</td><td style=\"text-align:right;padding-top:8px;font-weight:bold;\">{order.TotalAmount:C}</td></tr></tfoot>");
                        sb.Append("</table>");

                        var body = sb.ToString();
                        await _emailSender.SendEmailAsync(order.CustomerEmail, "Order received", body, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // Log and continue - do not re-enqueue the publish or email to avoid duplicate publishes/emails
                        _logger.LogWarning(ex, "Failed to send email for order {OrderId}", order.Id);
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
