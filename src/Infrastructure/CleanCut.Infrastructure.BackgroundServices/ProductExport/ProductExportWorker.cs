using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CleanCut.Infrastructure.BackgroundServices.ProductExport;

/// <summary>
/// Background service that periodically exports products with enhanced error handling and resilience
/// </summary>
public class ProductExportWorker : BackgroundService
{
    private readonly ILogger<ProductExportWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ProductExportConfiguration _settings;
 private int _consecutiveFailures = 0;

    public ProductExportWorker(
  ILogger<ProductExportWorker> logger,
    IServiceProvider serviceProvider,
        IOptions<ProductExportConfiguration> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProductExportWorker starting up. Will run every {Interval} minutes with enhanced error handling.", _settings.IntervalMinutes);

    // Initial delay to allow services to start up
        var initialDelay = TimeSpan.FromSeconds(_settings.InitialDelaySeconds);
  _logger.LogInformation("Waiting {InitialDelay} for services to initialize...", initialDelay);
        await Task.Delay(initialDelay, stoppingToken);

  while (!stoppingToken.IsCancellationRequested)
        {
            try
 {
             await ProcessExportWithRetryAsync(stoppingToken);
         _consecutiveFailures = 0; // Reset failure count on success
        }
     catch (OperationCanceledException)
       {
           _logger.LogInformation("ProductExportWorker is stopping due to cancellation request");
              break;
            }
            catch (Exception ex)
            {
     _consecutiveFailures++;
  _logger.LogError(ex, "Critical error in ProductExportWorker (consecutive failures: {FailureCount})", _consecutiveFailures);

       if (!_settings.ContinueOnError)
        {
              _logger.LogCritical("ContinueOnError is disabled, stopping the worker");
         break;
 }
            }

            // Calculate delay with exponential backoff on consecutive failures
   var delay = CalculateDelay();
            _logger.LogInformation("Next product export scheduled in {Delay}", delay);

    try
   {
                await Task.Delay(delay, stoppingToken);
            }
       catch (OperationCanceledException)
  {
 _logger.LogInformation("ProductExportWorker is stopping due to cancellation request during delay");
          break;
            }
 }

        _logger.LogInformation("ProductExportWorker has stopped");
    }

 private async Task ProcessExportWithRetryAsync(CancellationToken stoppingToken)
    {
        var attempt = 1;
        Exception? lastException = null;

      while (attempt <= _settings.MaxRetryAttempts && !stoppingToken.IsCancellationRequested)
        {
  try
         {
      // Check if we should skip due to API unavailability
     if (_settings.SkipOnApiUnavailable && !await IsApiAvailableAsync(stoppingToken))
      {
          _logger.LogWarning("API is not available, skipping export (attempt {Attempt}/{MaxAttempts})", attempt, _settings.MaxRetryAttempts);
         
     if (attempt < _settings.MaxRetryAttempts)
     {
      var retryDelay = CalculateRetryDelay(attempt);
      _logger.LogInformation("Waiting {RetryDelay} before next API availability check...", retryDelay);
             await Task.Delay(retryDelay, stoppingToken);
  attempt++;
            continue;
              }
    else
  {
       _logger.LogWarning("Max retry attempts reached, will try again in next scheduled interval");
       return;
 }
            }

                // Attempt the export
    await PerformExportAsync(stoppingToken);
         _logger.LogInformation("Product export completed successfully (attempt {Attempt})", attempt);
         return; // Success, exit retry loop
          }
     catch (HttpRequestException httpEx) when (httpEx.Message.Contains("refused") || httpEx.Message.Contains("timeout"))
        {
            lastException = httpEx;
_logger.LogWarning("API connection failed on attempt {Attempt}/{MaxAttempts}: {ErrorMessage}", 
            attempt, _settings.MaxRetryAttempts, httpEx.Message);
      }
     catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException || stoppingToken.IsCancellationRequested)
     {
                if (stoppingToken.IsCancellationRequested)
      {
                    _logger.LogInformation("Export cancelled due to shutdown request");
   throw new OperationCanceledException("Export cancelled due to shutdown", tcEx, stoppingToken);
      }

  lastException = tcEx;
                _logger.LogWarning("API request timed out on attempt {Attempt}/{MaxAttempts}", attempt, _settings.MaxRetryAttempts);
  }
         catch (Exception ex)
            {
       lastException = ex;
      _logger.LogError(ex, "Unexpected error during export attempt {Attempt}/{MaxAttempts}", attempt, _settings.MaxRetryAttempts);
            }

       // If we have more attempts, wait before retrying
 if (attempt < _settings.MaxRetryAttempts)
    {
                var retryDelay = CalculateRetryDelay(attempt);
      _logger.LogInformation("Retrying in {RetryDelay} (attempt {NextAttempt}/{MaxAttempts})", 
        retryDelay, attempt + 1, _settings.MaxRetryAttempts);
         await Task.Delay(retryDelay, stoppingToken);
     }

          attempt++;
        }

        // All retry attempts failed
        if (lastException != null)
        {
    _logger.LogError(lastException, "All {MaxAttempts} export attempts failed. Will try again in next scheduled interval.", _settings.MaxRetryAttempts);
     throw new InvalidOperationException($"Export failed after {_settings.MaxRetryAttempts} attempts", lastException);
        }
    }

    private async Task<bool> IsApiAvailableAsync(CancellationToken cancellationToken)
    {
        try
 {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;
    
     // Try to get an HTTP client configured for the API
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            if (httpClientFactory == null)
    {
            _logger.LogDebug("HttpClientFactory not available, assuming API is available");
      return true;
   }

         using var httpClient = httpClientFactory.CreateClient();
     httpClient.Timeout = TimeSpan.FromSeconds(_settings.HealthCheckTimeoutSeconds);

            // Get API base URL from configuration
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var apiBaseUrl = configuration.GetValue<string>("Api:BaseUrl") ?? "https://localhost:7142";
            
            // Try to reach the API base URL or a simple endpoint - we'll use the products endpoint as health check
     var healthCheckUrl = $"{apiBaseUrl}/api/v1/products";

      var response = await httpClient.GetAsync(healthCheckUrl, cancellationToken);
 var isAvailable = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
            
    _logger.LogDebug("API availability check: {IsAvailable} (Status: {StatusCode})", isAvailable, response.StatusCode);
            return isAvailable;
      }
   catch (HttpRequestException httpEx) when (httpEx.Message.Contains("refused") || httpEx.Message.Contains("timeout"))
        {
   _logger.LogDebug("API availability check failed due to connection issue: {Message}", httpEx.Message);
 return false;
        }
        catch (TaskCanceledException)
    {
            _logger.LogDebug("API availability check timed out");
return false;
        }
     catch (Exception ex)
     {
            _logger.LogDebug(ex, "API availability check failed, assuming API is unavailable");
          return false;
        }
    }

    private async Task PerformExportAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var productExportService = scope.ServiceProvider.GetRequiredService<IProductExportService>();

      _logger.LogDebug("Starting product export process...");
        await productExportService.ExportProductsAsync(stoppingToken);
        _logger.LogDebug("Product export process completed successfully");
    }

    private TimeSpan CalculateDelay()
    {
        var baseDelay = TimeSpan.FromMinutes(_settings.IntervalMinutes);

        // Add exponential backoff for consecutive failures
   if (_consecutiveFailures > 0)
        {
  var additionalDelay = Math.Min(
                TimeSpan.FromSeconds(_settings.RetryDelaySeconds * Math.Pow(2, _consecutiveFailures - 1)).TotalSeconds,
                TimeSpan.FromSeconds(_settings.MaxRetryDelaySeconds).TotalSeconds
            );

            baseDelay = baseDelay.Add(TimeSpan.FromSeconds(additionalDelay));
          _logger.LogInformation("Applying exponential backoff due to {FailureCount} consecutive failures: additional {AdditionalDelay}", 
         _consecutiveFailures, TimeSpan.FromSeconds(additionalDelay));
  }

        return baseDelay;
    }

    private TimeSpan CalculateRetryDelay(int attempt)
    {
        // Exponential backoff: base delay * 2^(attempt-1)
        var delaySeconds = _settings.RetryDelaySeconds * Math.Pow(2, attempt - 1);
        var maxDelaySeconds = _settings.MaxRetryDelaySeconds;
        
    var finalDelaySeconds = Math.Min(delaySeconds, maxDelaySeconds);
        return TimeSpan.FromSeconds(finalDelaySeconds);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ProductExportWorker is starting with interval: {Interval} minutes, max retries: {MaxRetries}", 
         _settings.IntervalMinutes, _settings.MaxRetryAttempts);
  await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ProductExportWorker is stopping");
  await base.StopAsync(cancellationToken);
    }
}