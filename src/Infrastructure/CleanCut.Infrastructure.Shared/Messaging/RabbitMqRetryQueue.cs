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
    private readonly Queue<OrderInfo> _fallback = new();

    public RabbitMqRetryQueue(IConfiguration configuration, ILogger<RabbitMqRetryQueue> logger)
    {
        _logger = logger;
        _queueKey = configuration["RabbitMQ:RetryQueueKey"] ?? "cleancut:retry:orders";

        try
        {
            var redisConn = configuration["Redis:ConnectionString"]; // e.g. localhost:6379
            if (!string.IsNullOrWhiteSpace(redisConn))
            {
                _redis = ConnectionMultiplexer.Connect(redisConn);
                _db = _redis.GetDatabase();
                _logger.LogInformation("Connected to Redis for retry queue");
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
            _fallback.Enqueue(order);
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
            return _fallback.Dequeue();
        }
    }
}
