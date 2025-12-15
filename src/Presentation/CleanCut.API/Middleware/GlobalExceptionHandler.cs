using CleanCut.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CleanCut.API.Middleware;

/// <summary>
/// Global exception handler
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            ValidationException validationEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation Failed",
                Detail = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)),
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            EntityNotFoundException notFoundEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Resource Not Found",
                Detail = notFoundEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            },
            EntityAlreadyExistsException existsEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Title = "Resource Already Exists",
                Detail = existsEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            },
            BusinessRuleValidationException businessEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.UnprocessableEntity,
                Title = "Business Rule Violation",
                Detail = businessEx.Message,
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2"
            },
            DomainException domainEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Domain Error",
                Detail = domainEx.Message,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            },
            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An error occurred",
                Detail = "An unexpected error occurred. Please try again later.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        };

        // Add common extensions
        problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);
        problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}