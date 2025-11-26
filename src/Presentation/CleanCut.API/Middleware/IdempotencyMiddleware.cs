using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CleanCut.API.Middleware;

/// <summary>
/// Middleware to validate presence and format of Idempotency-Key header for POST requests
/// and reject requests with missing or invalid keys when required.
/// This middleware is intentionally conservative: it only enforces header presence for
/// routes that have [HttpPost] and content-type application/json if the client indicates they
/// want idempotency. For now we only log and propagate the header; future enforcement rules
/// can be added here.
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsPost(context.Request.Method))
        {
            if (!context.Request.Headers.ContainsKey("Idempotency-Key"))
            {
                // Not enforcing failure; just log. If you want to enforce, return 400 here.
                _logger.LogDebug("POST request without Idempotency-Key to {Path}", context.Request.Path);
            }
            else
            {
                var key = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogDebug("Received Idempotency-Key {Key} for {Path}", key, context.Request.Path);
                }
            }
        }

        await _next(context);
    }
}
