using System.Text;
using System.Text.Json;
using CleanCut.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CleanCut.Infrastructure.Shared.Messaging;

/// <summary>
/// Simple integration event publisher that uses RabbitMQ to publish JSON payloads to a topic exchange.
/// Best-effort: logs but doesn't throw on publish failures.
/// </summary>
public class IntegrationEventPublisher : IIntegrationEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<IntegrationEventPublisher> _logger;
    private RabbitMQ.Client.IConnection? _connection;
    private RabbitMQ.Client.IChannel? _channel;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1,1);

    public IntegrationEventPublisher(RabbitMqOptions options, ILogger<IntegrationEventPublisher> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task PublishAsync(string routingKey, object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false))
            {
                _logger.LogWarning("RabbitMQ not available; skipping publish for routingKey={RoutingKey}", routingKey);
                return;
            }

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var body = Encoding.UTF8.GetBytes(json);

            await _channel!.BasicPublishAsync(exchange: _options.Exchange, routingKey: routingKey, body: body).ConfigureAwait(false);
            _logger.LogInformation("Published integration event to {Exchange} routingKey={RoutingKey}", _options.Exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish integration event routingKey={RoutingKey}", routingKey);
        }
    }

    public void Dispose()
    {
        try { _channel?.Dispose(); } catch { }
        try { _connection?.Dispose(); } catch { }
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
                ClientProvidedName = "CleanCut.IntegrationPublisher"
            };

            _connection = await factory.CreateConnectionAsync().ConfigureAwait(false);
            _channel = await _connection.CreateChannelAsync().ConfigureAwait(false);
            await _channel.ExchangeDeclareAsync(_options.Exchange, RabbitMQ.Client.ExchangeType.Topic, durable: true).ConfigureAwait(false);

            _initialized = true;
            _logger.LogInformation("IntegrationEventPublisher initialized; exchange={Exchange}", _options.Exchange);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EnsureConnectedAsync failed to connect to RabbitMQ");
            _initialized = false;
            return false;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
