using System.Text.Json;
using Microsoft.Extensions.Logging;
using MediatR;

namespace CleanCut.Application.Behaviors;

/// <summary>
/// Cache-backed idempotency behavior that can be registered in the Web API project's DI container.
/// This file remains in Application for visibility, but registration with IHttpContextAccessor must be
/// done by the API host where ASP.NET Core types are available.
/// </summary>
public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Func<object?> _getIdempotencyKey;
    private readonly CleanCut.Application.Common.Interfaces.IIdempotencyRepository _repository;
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    // To avoid taking a hard dependency on IHttpContextAccessor in the application project,
    // accept a delegate that will be provided by the API host. The delegate should return the
    // Idempotency-Key string or null.
    public IdempotencyBehavior(
        Func<object?> getIdempotencyKey,
        CleanCut.Application.Common.Interfaces.IIdempotencyRepository repository,
        ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    {
        _getIdempotencyKey = getIdempotencyKey;
        _repository = repository;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only attempt idempotency if a key is provided by host
        var rawKey = _getIdempotencyKey?.Invoke()?.ToString();
        if (string.IsNullOrWhiteSpace(rawKey))
            return await next();

        // Build a cache key using request type + key
        var cacheKey = $"idempotency:{typeof(TRequest).FullName}:{rawKey}";

        // Check DB for existing idempotency record
        try
        {
            var record = await _repository.GetByKeyAsync(cacheKey, cancellationToken);
            if (record != null)
            {
                if (!string.IsNullOrEmpty(record.ResponsePayload))
                {
                    _logger.LogInformation("Idempotency DB hit for key {CacheKey}", cacheKey);
                    var deserialized = JsonSerializer.Deserialize<TResponse>(record.ResponsePayload, _jsonOptions);
                    if (deserialized != null)
                        return deserialized;
                }

                // If record exists but no payload, it's a reserved in-flight marker
                if (string.IsNullOrEmpty(record.ResponsePayload))
                {
                    // Simple approach: fail fast so caller can retry. Optionally, could poll/wait.
                    throw new InvalidOperationException("Request with this Idempotency-Key is already in progress");
                }
            }
            else
            {
                // Create a reservation record to indicate in-flight request
                await _repository.AddAsync(new CleanCut.Application.Common.Models.IdempotencyEntry
                {
                    Key = cacheKey,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read/create idempotency record for key {CacheKey}", cacheKey);
        }

        var response = await next();

        try
        {
            var serialized = JsonSerializer.Serialize(response, _jsonOptions);

            var entry = new CleanCut.Application.Common.Models.IdempotencyEntry
            {
                Key = cacheKey,
                CreatedAt = DateTime.UtcNow,
                ResponsePayload = serialized,
                ResponseStatus = 200
            };

            // Upsert: try update first, otherwise add
            var existing = await _repository.GetByKeyAsync(cacheKey, cancellationToken);
            if (existing != null)
            {
                await _repository.UpdateAsync(entry, cancellationToken);
            }
            else
            {
                await _repository.AddAsync(entry, cancellationToken);
            }

            _logger.LogInformation("Stored idempotent response in DB for key {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write idempotency record for key {CacheKey}", cacheKey);
        }

        return response;
    }
}
