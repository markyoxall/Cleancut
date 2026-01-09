using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanCut.Infrastructure.BackgroundServices.Workers;

/// <summary>
/// Background worker that sends order confirmation emails.
/// Consumes orders from the Redis email queue (populated by RabbitMqConsumerWorker) and sends emails.
/// Single Responsibility: Only handles email sending, not RabbitMQ publishing.
/// </summary>
public class OrderEmailWorker : BackgroundService
{
    private readonly IEmailSender _emailSender;
    private readonly IRabbitMqRetryQueue _retryQueue;
    private readonly ILogger<OrderEmailWorker> _logger;

    public OrderEmailWorker(IEmailSender emailSender, IRabbitMqRetryQueue retryQueue, ILogger<OrderEmailWorker> logger)
    {
        _emailSender = emailSender;
        _retryQueue = retryQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderEmailWorker started");

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

                // Send email directly - no RabbitMQ publishing (consumer already handled that)
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
                        await _emailSender.SendEmailAsync(order.CustomerEmail, "Order Confirmation", body, stoppingToken);
                        _logger.LogInformation("Sent order confirmation email to {Email} for order {OrderId}", order.CustomerEmail, order.Id);
                    }
                    catch (Exception ex)
                    {
                        // Log and re-enqueue on email failure to retry later
                        _logger.LogWarning(ex, "Failed to send email for order {OrderId}, re-enqueuing", order.Id);
                        await _retryQueue.EnqueueAsync(order, stoppingToken);
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                }
                else
                {
                    _logger.LogWarning("Order {OrderId} has no customer email, skipping email", order.Id);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderEmailWorker loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("OrderEmailWorker stopping");
    }
}
