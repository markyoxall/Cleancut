# Background Service Improvements - Implementation Guide

## Overview

This guide explains the improvements made to the CleanCut Background Service to handle API connection failures and provide better resilience when the dependent services (IdentityServer and API) are not available.

## Problem

The original background service was failing with connection errors:

```
System.Net.Http.HttpRequestException: No connection could be made because the target machine actively refused it. (localhost:7142)
```

This occurred because:
1. The background service was running but the API server (localhost:7142) was not started
2. There was no retry logic or error handling for transient failures
3. The service would fail completely instead of gracefully handling unavailable dependencies

## Solution

### 1. Enhanced ProductExportSettings

Created a comprehensive configuration class with resilience settings:

```csharp
public class ProductExportSettings
{
    public int IntervalMinutes { get; set; } = 5;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 30;
    public int MaxRetryDelaySeconds { get; set; } = 300;
    public bool ContinueOnError { get; set; } = true;
    public int InitialDelaySeconds { get; set; } = 30;
    public bool SkipOnApiUnavailable { get; set; } = true;
    public int HealthCheckTimeoutSeconds { get; set; } = 10;
}
```

### 2. Improved ProductExportWorker

Enhanced the background worker with:

#### Retry Logic with Exponential Backoff
```csharp
private async Task ProcessExportWithRetryAsync(CancellationToken stoppingToken)
{
    var attempt = 1;
    while (attempt <= _settings.MaxRetryAttempts && !stoppingToken.IsCancellationRequested)
    {
        try
        {
            if (_settings.SkipOnApiUnavailable && !await IsApiAvailableAsync(stoppingToken))
     {
     // Handle API unavailability gracefully
       continue;
   }
        
   await PerformExportAsync(stoppingToken);
            return; // Success
        }
        catch (HttpRequestException httpEx) when (httpEx.Message.Contains("refused"))
        {
       // Handle connection refused errors
    if (attempt < _settings.MaxRetryAttempts)
            {
     var retryDelay = CalculateRetryDelay(attempt);
                await Task.Delay(retryDelay, stoppingToken);
       }
   }
   attempt++;
    }
}
```

#### API Health Check
```csharp
private async Task<bool> IsApiAvailableAsync(CancellationToken cancellationToken)
{
    try
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(_settings.HealthCheckTimeoutSeconds);
        
   var response = await httpClient.GetAsync(healthCheckUrl, cancellationToken);
        // Consider both success and unauthorized (401) as "available"
 return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized;
}
    catch (HttpRequestException httpEx) when (httpEx.Message.Contains("refused"))
    {
        return false; // API is definitely not available
    }
}
```

#### Exponential Backoff Strategy
```csharp
private TimeSpan CalculateRetryDelay(int attempt)
{
    var delaySeconds = _settings.RetryDelaySeconds * Math.Pow(2, attempt - 1);
    var maxDelaySeconds = _settings.MaxRetryDelaySeconds;
    return TimeSpan.FromSeconds(Math.Min(delaySeconds, maxDelaySeconds));
}
```

### 3. HTTP Client Resilience with Polly

Added Polly policies for HTTP clients:

```csharp
services.AddHttpClient<IApiService, ApiService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
   .CircuitBreakerAsync(
       handledEventsAllowedBeforeBreaking: 3,
    durationOfBreak: TimeSpan.FromSeconds(30));
}
```

### 4. Enhanced Configuration

Updated `appsettings.json` with resilience settings:

```json
{
  "ProductExport": {
  "IntervalMinutes": 5,
    "MaxRetryAttempts": 3,
  "RetryDelaySeconds": 30,
    "MaxRetryDelaySeconds": 300,
    "ContinueOnError": true,
    "InitialDelaySeconds": 30,
    "SkipOnApiUnavailable": true,
    "HealthCheckTimeoutSeconds": 10
  }
}
```

## Benefits

### 1. Graceful Degradation
- Service continues running even when API is unavailable
- Proper error logging without flooding logs
- Configurable behavior for different scenarios

### 2. Resilience Patterns
- **Retry with Exponential Backoff**: Automatic retry with increasing delays
- **Circuit Breaker**: Prevents cascading failures
- **Health Checks**: Proactive API availability checking
- **Timeout Handling**: Prevents hanging requests

### 3. Operational Excellence
- **Comprehensive Logging**: Detailed logs for troubleshooting
- **Configurable Behavior**: Easy to adjust for different environments
- **Monitoring Ready**: Metrics and status information available

## Usage Instructions

### 1. Development Environment

1. **Start services in order**:
   ```bash
   # Terminal 1: Start IdentityServer
   dotnet run --project src/Infrastructure/CleanCut.Infrastructure.Identity
   
   # Terminal 2: Start API
   dotnet run --project src/Presentation/CleanCut.API
   
   # Terminal 3: Start Background Service
   dotnet run --project src/Applications/CleanCut.ProductExportHost
   ```

2. **Monitor logs**: Check for successful exports and any retry attempts

### 2. Production Deployment

1. **Configure health check endpoints** in your API
2. **Adjust retry settings** based on your infrastructure
3. **Set up monitoring** for background service health
4. **Configure appropriate timeouts** for your network conditions

### 3. Troubleshooting

#### Service Won't Start
- Check if ports 5001 (IdentityServer) and 7142 (API) are available
- Verify configuration settings in `appsettings.json`
- Review application logs for startup errors

#### Export Failures
- Check API connectivity: `curl https://localhost:7142/api/v1/products`
- Verify authentication configuration
- Review retry settings and adjust if needed

#### Performance Issues
- Monitor retry frequency and success rates
- Adjust `IntervalMinutes` and retry settings
- Check for resource constraints

## Configuration Options

| Setting | Description | Default | Recommended |
|---------|-------------|---------|-------------|
| `IntervalMinutes` | Export frequency | 5 | 5-15 minutes |
| `MaxRetryAttempts` | Max retry attempts per export | 3 | 3-5 attempts |
| `RetryDelaySeconds` | Base retry delay | 30 | 30-60 seconds |
| `MaxRetryDelaySeconds` | Maximum retry delay | 300 | 300-600 seconds |
| `ContinueOnError` | Keep running after errors | true | true for production |
| `SkipOnApiUnavailable` | Skip when API down | true | true for resilience |
| `HealthCheckTimeoutSeconds` | Health check timeout | 10 | 5-15 seconds |

## Monitoring and Alerts

### Key Metrics to Monitor
- Export success rate
- Average retry attempts per export
- API availability percentage
- Background service uptime

### Recommended Alerts
- Export failure rate > 50% over 30 minutes
- Background service stopped
- Consecutive API failures > 10
- Average export time > expected threshold

## Future Enhancements

1. **Dead Letter Queue**: Store failed export requests for manual retry
2. **Metrics Collection**: Detailed performance metrics with Prometheus
3. **Dynamic Configuration**: Runtime configuration updates without restart
4. **Multi-tenant Support**: Different settings per tenant/customer
5. **Partial Exports**: Resume from last successful point in large datasets

This implementation provides a robust foundation for background processing that can handle real-world reliability challenges while maintaining excellent operational visibility.