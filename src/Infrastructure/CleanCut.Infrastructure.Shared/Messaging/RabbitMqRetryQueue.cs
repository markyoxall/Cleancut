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

        try
        {
            // Support both configuration styles: "Redis:ConnectionString" and ConnectionStrings:Redis
            var redisConn = configuration["Redis:ConnectionString"];
            if (string.IsNullOrWhiteSpace(redisConn))
            {
                redisConn = configuration.GetConnectionString("Redis");
            }

            if (!string.IsNullOrWhiteSpace(redisConn))
            {
                _redis = ConnectionMultiplexer.Connect(redisConn);
                _db = _redis.GetDatabase();
                _logger.LogInformation("Connected to Redis for retry queue using connection string: {Conn}", redisConn);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to Redis; falling back to in-memory retry queue");
        }

        _logger.LogInformation("Using in-memory retry queue (Redis not configured)");
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
