using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.DTOs;
using CleanCut.Infrastructure.Shared.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CleanCut.Infrastructure.BackgroundServices.Workers;

/// <summary>
/// Consumes order.created messages from RabbitMQ and sends confirmation emails directly.
/// No Redis queue - simpler, cleaner architecture.
/// </summary>
public class RabbitMqConsumerWorker : BackgroundService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RabbitMqConsumerWorker> _logger;
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private RabbitMQ.Client.IChannel? _channel;

    public RabbitMqConsumerWorker(IEmailSender emailSender, RabbitMqOptions options, ILogger<RabbitMqConsumerWorker> logger)
    {
        _emailSender = emailSender;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMqConsumerWorker starting - will send emails directly");

        // Wait for RabbitMQ container to be fully ready
        _logger.LogInformation("Waiting 15 seconds for RabbitMQ to be ready...");
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        try
        {
            await InitializeRabbitMqAsync(stoppingToken);
            if (_channel == null)
            {
                _logger.LogWarning("Failed to initialize RabbitMQ channel after all retries. Consumer will not receive messages.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogDebug("Received message from RabbitMQ: {Message}", message);

                    var order = JsonSerializer.Deserialize<OrderInfo>(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                    if (order != null)
                    {
                        _logger.LogInformation("Processing order {OrderId} for customer email {Email}", order.Id, order.CustomerEmail);

                        // Send email directly - no Redis queue
                        if (!string.IsNullOrEmpty(order.CustomerEmail))
                        {
                            try
                            {
                                var emailBody = BuildOrderConfirmationEmail(order);
                                await _emailSender.SendEmailAsync(order.CustomerEmail, "Order Confirmation", emailBody, stoppingToken);
                                _logger.LogInformation("✅ Sent order confirmation email to {Email} for order {OrderId}", order.CustomerEmail, order.Id);

                                // Acknowledge message only after successful email send
                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, "❌ Failed to send email for order {OrderId} to {Email}. Message will be requeued.", order.Id, order.CustomerEmail);
                                // Don't acknowledge - message will be redelivered by RabbitMQ
                                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Order {OrderId} has no customer email, skipping email. Acknowledging message.", order.Id);
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize order message. Acknowledging to avoid reprocessing.");
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from RabbitMQ");
                    try
                    {
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                    catch { }
                }
            };

            await _channel.BasicConsumeAsync(_options.OrderCreatedQueue, false, consumer, stoppingToken);
            _logger.LogInformation("✅ Listening to queue: {Queue}", _options.OrderCreatedQueue);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("RabbitMqConsumerWorker stopping gracefully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in consumer. Will not receive messages.");
        }
    }

    private string BuildOrderConfirmationEmail(OrderInfo order)
    {
        var sb = new StringBuilder();
        sb.Append($"<h2>Thank you for your order!</h2>");
        sb.Append($"<p>Order Number: <strong>{order.OrderNumber}</strong></p>");
        sb.Append($"<p>Order Date: {order.OrderDate:yyyy-MM-dd HH:mm}</p>");
        sb.Append("<table style=\"width:100%;border-collapse:collapse;margin-top:20px;\">");
        sb.Append("<thead><tr style=\"background-color:#f0f0f0;\"><th style=\"text-align:left;padding:10px;border:1px solid #ddd;\">Item</th><th style=\"text-align:right;padding:10px;border:1px solid #ddd;\">Unit Price</th><th style=\"text-align:right;padding:10px;border:1px solid #ddd;\">Qty</th><th style=\"text-align:right;padding:10px;border:1px solid #ddd;\">Total</th></tr></thead>");
        sb.Append("<tbody>");

        if (order.OrderLineItems != null)
        {
            foreach (var item in order.OrderLineItems)
            {
                sb.Append($"<tr><td style=\"padding:10px;border:1px solid #ddd;\">{System.Net.WebUtility.HtmlEncode(item.ProductName)}</td>");
                sb.Append($"<td style=\"text-align:right;padding:10px;border:1px solid #ddd;\">{item.UnitPrice:C}</td>");
                sb.Append($"<td style=\"text-align:right;padding:10px;border:1px solid #ddd;\">{item.Quantity}</td>");
                sb.Append($"<td style=\"text-align:right;padding:10px;border:1px solid #ddd;\">{item.LineTotal:C}</td></tr>");
            }
        }

        sb.Append("</tbody>");
        sb.Append($"<tfoot><tr style=\"background-color:#f0f0f0;font-weight:bold;\"><td colspan=\"3\" style=\"text-align:right;padding:10px;border:1px solid #ddd;\">Total:</td><td style=\"text-align:right;padding:10px;border:1px solid #ddd;\">{order.TotalAmount:C}</td></tr></tfoot>");
        sb.Append("</table>");
        sb.Append("<p style=\"margin-top:20px;\">We'll notify you when your order ships.</p>");

        return sb.ToString();
    }

    private async Task InitializeRabbitMqAsync(CancellationToken ct)
    {
        var retryCount = 0;
        var maxRetries = 10;

        _logger.LogInformation("Starting RabbitMQ connection initialization (up to {MaxRetries} attempts)", maxRetries);

        while (retryCount < maxRetries && !ct.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Attempting RabbitMQ connection (attempt {Attempt}/{MaxRetries}) to {Host}:{Port}", 
                    retryCount + 1, maxRetries, _options.Hostname, _options.Port);

                var factory = new ConnectionFactory
                {
                    HostName = _options.Hostname,
                    Port = _options.Port,
                    VirtualHost = _options.VirtualHost,
                    UserName = _options.Username,
                    Password = _options.Password,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    AutomaticRecoveryEnabled = true
                };

                // Diagnostics: resolve DNS and perform a quick TCP connect to give clearer failure reason
                try
                {
                    var addresses = await System.Net.Dns.GetHostAddressesAsync(_options.Hostname, ct).ConfigureAwait(false);
                    _logger.LogDebug("Resolved RabbitMQ hostname '{Host}' to: {Addresses}", _options.Hostname, string.Join(',', addresses));
                }
                catch (Exception dnsEx)
                {
                    _logger.LogWarning(dnsEx, "Failed to resolve RabbitMQ hostname {Host}", _options.Hostname);
                }

                try
                {
                    using var tcp = new System.Net.Sockets.TcpClient();
                    var connectTask = tcp.ConnectAsync(_options.Hostname, _options.Port);
                    var completed = await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(5), ct)).ConfigureAwait(false);
                    if (completed != connectTask || !tcp.Connected)
                    {
                        _logger.LogWarning("TCP connect to RabbitMQ {Host}:{Port} timed out or failed", _options.Hostname, _options.Port);
                        throw new System.TimeoutException($"TCP connect to {_options.Hostname}:{_options.Port} timed out");
                    }
                    _logger.LogDebug("TCP connect to RabbitMQ {Host}:{Port} succeeded", _options.Hostname, _options.Port);
                }
                catch (Exception tcpEx)
                {
                    _logger.LogWarning(tcpEx, "TCP check failed for RabbitMQ {Host}:{Port}", _options.Hostname, _options.Port);
                    throw;
                }

                factory.ClientProvidedName = "CleanCut.Consumer";

                _connection = await factory.CreateConnectionAsync(ct);
                _logger.LogInformation("RabbitMQ connection established, creating channel...");

                // Use CreateChannelAsync to obtain IChannel (project uses IChannel API)
                _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
                _logger.LogInformation("Channel created, declaring exchange and queue...");

                await _channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, true, cancellationToken: ct);
                await _channel.QueueDeclareAsync(_options.OrderCreatedQueue, true, false, false, cancellationToken: ct);
                await _channel.QueueBindAsync(_options.OrderCreatedQueue, _options.Exchange, _options.OrderCreatedRoutingKey, cancellationToken: ct);

                _logger.LogInformation("RabbitMQ initialized successfully - Exchange: {Exchange}, Queue: {Queue}", 
                    _options.Exchange, _options.OrderCreatedQueue);
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to initialize RabbitMQ (attempt {Attempt}/{MaxRetries}). Error: {Message}", 
                    retryCount, maxRetries, ex.Message);

                if (retryCount < maxRetries)
                {
                    _logger.LogInformation("Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                }
            }
        }

        _logger.LogError("Failed to initialize RabbitMQ after {MaxRetries} attempts. Consumer will not receive messages.", maxRetries);
    }
}
