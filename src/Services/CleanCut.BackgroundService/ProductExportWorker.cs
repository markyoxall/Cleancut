using CleanCut.BackgroundService.Configuration;
using CleanCut.BackgroundService.Services;
using Microsoft.Extensions.Options;

namespace CleanCut.BackgroundService.Workers;

/// <summary>
/// Background service that periodically fetches products from the API and exports them to CSV
/// </summary>
public class ProductExportWorker : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly ILogger<ProductExportWorker> _logger;
    private readonly IAuthenticationService _authService;
    private readonly IApiService _apiService;
    private readonly ICsvExportService _csvService;
    private readonly BackgroundServiceSettings _settings;

    public ProductExportWorker(
        ILogger<ProductExportWorker> logger,
        IAuthenticationService authService,
        IApiService apiService,
        ICsvExportService csvService,
        IOptions<BackgroundServiceSettings> settings)
    {
        _logger = logger;
        _authService = authService;
        _apiService = apiService;
        _csvService = csvService;
        _settings = settings.Value;
 }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
        _logger.LogInformation("ProductExportWorker starting up. Will run every {Interval} minutes.", _settings.IntervalMinutes);

        // Initial delay to allow services to start up
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExportProductsAsync(stoppingToken);
            }
    catch (OperationCanceledException)
            {
   _logger.LogInformation("ProductExportWorker is stopping due to cancellation request");
        break;
            }
            catch (Exception ex)
    {
         _logger.LogError(ex, "An error occurred during product export");
  }

            // Wait for the next interval
         var delay = TimeSpan.FromMinutes(_settings.IntervalMinutes);
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

    private async Task ExportProductsAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Starting product export at {StartTime}", startTime);

        try
     {
            // Step 1: Authenticate with IdentityServer
     _logger.LogInformation("Step 1: Authenticating with IdentityServer");
            var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);
   _logger.LogInformation("Successfully authenticated with IdentityServer");

         // Step 2: Fetch products from API
  _logger.LogInformation("Step 2: Fetching products from CleanCut API");
 var products = await _apiService.GetAllProductsAsync(accessToken, cancellationToken);
   _logger.LogInformation("Successfully fetched {Count} products from API", products.Count);

       // Step 3: Export to CSV
            _logger.LogInformation("Step 3: Exporting products to CSV");
         var csvFilePath = await _csvService.ExportProductsAsync(products, cancellationToken);
    
            var duration = DateTime.UtcNow - startTime;
       _logger.LogInformation("Product export completed successfully in {Duration}. CSV file saved to: {FilePath}", 
      duration, csvFilePath);
        }
        catch (UnauthorizedAccessException ex)
        {
     _logger.LogError(ex, "Authentication failed. Check IdentityServer configuration and client credentials");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API call failed. Check API availability and configuration");
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Operation timed out. Check network connectivity and API response times");
        }
      catch (Exception ex)
        {
      _logger.LogError(ex, "Unexpected error during product export");
          throw; // Re-throw for the outer catch block
     }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ProductExportWorker is starting with the following configuration:");
    _logger.LogInformation("- IdentityServer Authority: {Authority}", _settings.IdentityServer.Authority);
        _logger.LogInformation("- IdentityServer Client ID: {ClientId}", _settings.IdentityServer.ClientId);
        _logger.LogInformation("- API Base URL: {BaseUrl}", _settings.Api.BaseUrl);
  _logger.LogInformation("- Export Interval: {Interval} minutes", _settings.IntervalMinutes);
        _logger.LogInformation("- CSV Output Directory: {Directory}", _settings.Csv.OutputDirectory);

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ProductExportWorker is stopping");
        await base.StopAsync(cancellationToken);
    }
}