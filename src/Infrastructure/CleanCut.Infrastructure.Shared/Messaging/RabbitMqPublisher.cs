using System.Text;
using System.Text.Json;
using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
namespace CleanCut.Infrastructure.Shared.Messaging;

public class RabbitMqOptions
{
    public string Hostname { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Exchange { get; set; } = "cleancut.events";
    public string OrderCreatedRoutingKey { get; set; } = "order.created";
        // Optional: ensure a durable queue is declared and bound to the exchange for the order-created routing key.
        public string OrderCreatedQueue { get; set; } = "cleancut.order.created";
}

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private IConnection? _connection;
    private RabbitMQ.Client.IChannel? _channel;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private bool _initialized;
    private readonly System.Threading.SemaphoreSlim _initLock = new(1, 1);

    public RabbitMqPublisher(RabbitMqOptions options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options;
        _logger = logger;
        _initialized = false;
    }

    public async Task PublishOrderCreatedAsync(OrderInfo order, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false))
            {
                _logger.LogWarning("RabbitMQ not available; skipping publish for Order {OrderId}", order.Id);
                return;
            }

            var payload = JsonSerializer.Serialize(order, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var body = Encoding.UTF8.GetBytes(payload);

            // Use async-first publish (no properties)
            await _channel!.BasicPublishAsync(exchange: _options.Exchange, routingKey: _options.OrderCreatedRoutingKey, body: body).ConfigureAwait(false);
            _logger.LogInformation("Published OrderCreated for OrderId={OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish OrderCreated for OrderId={OrderId}", order.Id);
        }
    }

    public async Task<bool> TryPublishOrderCreatedAsync(OrderInfo order, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false)) return false;

            var payload = JsonSerializer.Serialize(order, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var body = Encoding.UTF8.GetBytes(payload);

            await _channel!.BasicPublishAsync(exchange: _options.Exchange, routingKey: _options.OrderCreatedRoutingKey, body: body).ConfigureAwait(false);
            _logger.LogInformation("Published OrderCreated for OrderId={OrderId}", order.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TryPublish failed for OrderId={OrderId}", order.Id);
            return false;
        }
    }

    public void Dispose()
    {
        try
        {
            try { (_channel as System.IDisposable)?.Dispose(); } catch { }
            try { (_connection as System.IDisposable)?.Dispose(); } catch { }
        }
        catch { }
    }

    private async Task<bool> EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized && _connection != null && _connection.IsOpen && _channel != null) return true;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_initialized && _connection != null && _connection.IsOpen && _channel != null) return true;

            var factory = new ConnectionFactory
            {
                HostName = _options.Hostname,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.Username,
                Password = _options.Password,
                AutomaticRecoveryEnabled = true,
                ClientProvidedName = "CleanCut.Publisher"
            };

            // Use async-first API from RabbitMQ.Client 7.x
            _connection = await factory.CreateConnectionAsync().ConfigureAwait(false);
            _channel = await _connection.CreateChannelAsync().ConfigureAwait(false);

            // declare exchange (idempotent)
            await _channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, durable: true).ConfigureAwait(false);

            // Ensure a durable queue exists and is bound for the order-created routing key so messages are not dropped.
            if (!string.IsNullOrWhiteSpace(_options.OrderCreatedQueue))
            {
                // Declare durable, non-exclusive, non-auto-delete queue
                await _channel.QueueDeclareAsync(_options.OrderCreatedQueue, durable: true, exclusive: false, autoDelete: false).ConfigureAwait(false);

                // Bind queue to exchange with the configured routing key
                await _channel.QueueBindAsync(_options.OrderCreatedQueue, _options.Exchange, _options.OrderCreatedRoutingKey).ConfigureAwait(false);
            }

            _initialized = true;
            _logger.LogInformation("RabbitMQ publisher initialized; exchange={Exchange}", _options.Exchange);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EnsureConnectedAsync failed to connect to RabbitMQ {Host}:{Port} vhost={VHost}", _options.Hostname, _options.Port, _options.VirtualHost);
            _initialized = false;
            return false;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
