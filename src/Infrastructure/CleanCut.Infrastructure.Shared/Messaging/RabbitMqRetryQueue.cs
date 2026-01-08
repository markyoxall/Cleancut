using System.Text.Json;
using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CleanCut.Infrastructure.Shared.Messaging;

public class RabbitMqRetryQueue : IRabbitMqRetryQueue
{
    private readonly ILogger<RabbitMqRetryQueue> _logger;
    private readonly ConnectionMultiplexer? _redis;
    private readonly IDatabase? _db;
    private readonly string _queueKey;
    private readonly string _idsKey;
    private readonly Queue<OrderInfo> _fallback = new();
    private readonly HashSet<Guid> _fallbackIds = new();

    public RabbitMqRetryQueue(IConfiguration configuration, ILogger<RabbitMqRetryQueue> logger)
    {
        _logger = logger;
        _queueKey = configuration["RabbitMQ:RetryQueueKey"] ?? "cleancut:retry:orders";
        _idsKey = _queueKey + ":ids";

        // Support multiple configuration styles:
        // 1. Manual/default configuration (ConnectionStrings:Redis or Redis:ConnectionString)
        // 2. Aspire-injected connection string (ConnectionStrings:redis - lowercase)
        var redisConn = configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConn))
        {
            redisConn = configuration["Redis:ConnectionString"];
        }
        if (string.IsNullOrWhiteSpace(redisConn))
        {
            redisConn = configuration.GetConnectionString("redis");
        }

        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            _logger.LogInformation("Attempting to connect to Redis using connection string: {ConnectionString}", redisConn);

            try
            {
                // Add abortConnect=false to allow background reconnection attempts
                var configOptions = ConfigurationOptions.Parse(redisConn);
                configOptions.AbortOnConnectFail = false;

                _redis = ConnectionMultiplexer.Connect(configOptions);
                _db = _redis.GetDatabase();

                if (_redis.IsConnected)
                {
                    _logger.LogInformation("Connected to Redis for retry queue at {Endpoint}", configOptions.EndPoints[0]);
                }
                else
                {
                    _logger.LogWarning("Redis configured at {Endpoint} but not currently available. Will retry in background. Using in-memory queue until connected.", configOptions.EndPoints[0]);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to initialize Redis connection: {Message}. Using in-memory retry queue.", ex.Message);
            }
        }
        else
        {
            _logger.LogInformation("Redis not configured. Using in-memory retry queue.");
        }
    }

    public async Task EnqueueAsync(OrderInfo order, CancellationToken cancellationToken = default)
    {
        if (_db != null)
        {
            try
            {
                // Use a Redis Set to deduplicate order IDs in the queue
                var added = await _db.SetAddAsync(_idsKey, order.Id.ToString());
                if (!added)
                {
                    // already queued
                    _logger.LogDebug("Order {OrderId} already in retry queue, skipping enqueue", order.Id);
                    return;
                }

                var payload = JsonSerializer.Serialize(order, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                await _db.ListRightPushAsync(_queueKey, payload);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to push order to Redis queue, falling back to in-memory");
            }
        }

        lock (_fallback)
        {
            if (_fallbackIds.Contains(order.Id))
            {
                _logger.LogDebug("Order {OrderId} already in in-memory retry queue, skipping enqueue", order.Id);
                return;
            }

            _fallback.Enqueue(order);
            _fallbackIds.Add(order.Id);
        }
    }

    public async Task<OrderInfo?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        if (_db != null)
        {
            try
            {
                var val = await _db.ListLeftPopAsync(_queueKey);
                if (val.IsNullOrEmpty) return null;
                var json = (string)val!;
                var order = JsonSerializer.Deserialize<OrderInfo>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                try
                {
                    // remove id from set so it can be retried later if needed
                    if (order != null)
                        await _db.SetRemoveAsync(_idsKey, order.Id.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to remove order id from Redis ids set");
                }
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to pop order from Redis queue");
            }
        }

        lock (_fallback)
        {
            if (_fallback.Count == 0) return null;
            var order = _fallback.Dequeue();
            if (order != null)
                _fallbackIds.Remove(order.Id);
            return order;
        }
    }
}
