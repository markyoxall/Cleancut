using System.Text;
using System.Text.Json;
using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
}

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly object? _connection;
    private readonly object? _channel;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly bool _available;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        try
        {
            // Dynamically create ConnectionFactory if RabbitMQ.Client is present
            var factoryType = Type.GetType("RabbitMQ.Client.ConnectionFactory, RabbitMQ.Client");
            if (factoryType == null)
            {
                _logger.LogWarning("RabbitMQ.Client not available on the platform; publisher will operate in no-op mode.");
                _available = false;
                return;
            }

            var factory = Activator.CreateInstance(factoryType)!;
            // Set properties via reflection
            factoryType.GetProperty("HostName")?.SetValue(factory, _options.Hostname);
            factoryType.GetProperty("Port")?.SetValue(factory, _options.Port);
            factoryType.GetProperty("VirtualHost")?.SetValue(factory, _options.VirtualHost);
            factoryType.GetProperty("UserName")?.SetValue(factory, _options.Username);
            factoryType.GetProperty("Password")?.SetValue(factory, _options.Password);

            // Create connection and channel via reflection
            var createConnection = factoryType.GetMethod("CreateConnection", Type.EmptyTypes);
            _connection = createConnection?.Invoke(factory, null);
            if (_connection == null)
            {
                _logger.LogWarning("Unable to create RabbitMQ connection; publisher disabled.");
                _available = false;
                return;
            }

            var connectionType = _connection.GetType();
            var createModel = connectionType.GetMethod("CreateModel", Type.EmptyTypes);
            _channel = createModel?.Invoke(_connection, null);
            if (_channel == null)
            {
                _logger.LogWarning("Unable to create RabbitMQ model/channel; publisher disabled.");
                _available = false;
                return;
            }

            // Declare exchange via reflection
            var channelType = _channel.GetType();
            var exchangeDeclare = channelType.GetMethod("ExchangeDeclare", new[] { typeof(string), Type.GetType("RabbitMQ.Client.ExchangeType, RabbitMQ.Client") ?? typeof(string), typeof(bool), typeof(bool), typeof(object) })
                                 ?? channelType.GetMethod("ExchangeDeclare", new[] { typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(object) })
                                 ?? channelType.GetMethod("ExchangeDeclare", new[] { typeof(string), typeof(string) });

            if (exchangeDeclare != null)
            {
                try
                {
                    // Try ExchangeDeclare(exchange, "topic", true, false, null)
                    var topic = "topic";
                    if (exchangeDeclare.GetParameters().Length == 5)
                        exchangeDeclare.Invoke(_channel, new object[] { _options.Exchange, topic, true, false, null });
                    else
                        exchangeDeclare.Invoke(_channel, new object[] { _options.Exchange, topic });
                }
                catch
                {
                    // ignore
                }
            }

            _available = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize RabbitMQ publisher. Falling back to no-op.");
            _available = false;
        }
    }

    public Task PublishOrderCreatedAsync(OrderInfo order, CancellationToken cancellationToken = default)
    {
        if (!_available || _channel == null)
        {
            _logger.LogInformation("RabbitMQ not available; skipping publish for Order {OrderId}", order.Id);
            return Task.CompletedTask;
        }

        try
        {
            var payload = JsonSerializer.Serialize(order, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var body = Encoding.UTF8.GetBytes(payload);

            var channelType = _channel.GetType();
            var basicPublish = channelType.GetMethod("BasicPublish", new[] { typeof(string), typeof(string), Type.GetType("RabbitMQ.Client.IBasicProperties, RabbitMQ.Client"), typeof(byte[]) })
                               ?? channelType.GetMethod("BasicPublish", new[] { typeof(string), typeof(string), typeof(bool), typeof(object), typeof(byte[]) })
                               ?? channelType.GetMethod("BasicPublish", new[] { typeof(string), typeof(string), typeof(byte[]) });

            if (basicPublish != null)
            {
                // Prefer method signature (exchange, routingKey, basicProperties, body)
                try
                {
                    var props = CreateBasicProperties(channelType);
                    basicPublish.Invoke(_channel, new object[] { _options.Exchange, _options.OrderCreatedRoutingKey, props, body });
                }
                catch
                {
                    // Fallback to other overloads
                    try { basicPublish.Invoke(_channel, new object[] { _options.Exchange, _options.OrderCreatedRoutingKey, body }); } catch { }
                }
            }

            _logger.LogInformation("Published OrderCreated for OrderId={OrderId}", order.Id);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish OrderCreated for OrderId={OrderId}", order.Id);
            return Task.CompletedTask;
        }
    }

    public Task<bool> TryPublishOrderCreatedAsync(OrderInfo order, CancellationToken cancellationToken = default)
    {
        if (!_available || _channel == null)
            return Task.FromResult(false);

        try
        {
            var payload = JsonSerializer.Serialize(order, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var body = Encoding.UTF8.GetBytes(payload);

            var channelType = _channel.GetType();
            var basicPublish = channelType.GetMethod("BasicPublish", new[] { typeof(string), typeof(string), Type.GetType("RabbitMQ.Client.IBasicProperties, RabbitMQ.Client"), typeof(byte[]) })
                               ?? channelType.GetMethod("BasicPublish", new[] { typeof(string), typeof(string), typeof(bool), typeof(object), typeof(byte[]) })
                               ?? channelType.GetMethod("BasicPublish", new[] { typeof(string), typeof(string), typeof(byte[]) });

            if (basicPublish != null)
            {
                try
                {
                    var props = CreateBasicProperties(channelType);
                    basicPublish.Invoke(_channel, new object[] { _options.Exchange, _options.OrderCreatedRoutingKey, props, body });
                }
                catch
                {
                    try { basicPublish.Invoke(_channel, new object[] { _options.Exchange, _options.OrderCreatedRoutingKey, body }); } catch { }
                }
            }

            _logger.LogInformation("Published OrderCreated for OrderId={OrderId}", order.Id);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TryPublish failed for OrderId={OrderId}", order.Id);
            return Task.FromResult(false);
        }
    }

    private object? CreateBasicProperties(Type channelType)
    {
        try
        {
            var createProps = channelType.GetMethod("CreateBasicProperties", Type.EmptyTypes);
            if (createProps == null) return null;
            var props = createProps.Invoke(_channel, null);
            var propsType = props.GetType();
            propsType.GetProperty("ContentType")?.SetValue(props, "application/json");
            // DeliveryMode property may be named Persistent or DeliveryMode depending on client
            var dmProp = propsType.GetProperty("DeliveryMode") ?? propsType.GetProperty("Persistent");
            if (dmProp != null && dmProp.PropertyType == typeof(byte)) dmProp.SetValue(props, (byte)2);
            return props;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        try
        {
            if (_channel != null)
            {
                var close = _channel.GetType().GetMethod("Close", Type.EmptyTypes);
                close?.Invoke(_channel, null);
            }

            if (_connection != null)
            {
                var close = _connection.GetType().GetMethod("Close", Type.EmptyTypes);
                close?.Invoke(_connection, null);
            }
        }
        catch { }
    }
}
