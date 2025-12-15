using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CleanCut.Infrastructure.BackgroundServices.Authentication;
using CleanCut.Infrastructure.BackgroundServices.ExternalApi;
using CleanCut.Infrastructure.BackgroundServices.FileExport;

namespace CleanCut.Infrastructure.BackgroundServices.ProductExport;

/// <summary>
/// Service for managing product export operations
/// </summary>
public interface IProductExportService
{
    /// <summary>
    /// Exports products from API to CSV
    /// </summary>
    Task ExportProductsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of product export service
/// </summary>
public class ProductExportService : IProductExportService
{
    private readonly IAuthenticationService _authService;
    private readonly IApiService _apiService;
    private readonly ICsvExportService _csvService;
    private readonly ILogger<ProductExportService> _logger;

 public ProductExportService(
      IAuthenticationService authService,
        IApiService apiService,
  ICsvExportService csvService,
        ILogger<ProductExportService> logger)
    {
     _authService = authService;
        _apiService = apiService;
        _csvService = csvService;
        _logger = logger;
    }

    public async Task ExportProductsAsync(CancellationToken cancellationToken = default)
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
   throw;
        }
catch (HttpRequestException ex)
        {
     _logger.LogError(ex, "API call failed. Check API availability and configuration");
            throw;
 }
        catch (TimeoutException ex)
        {
          _logger.LogError(ex, "Operation timed out. Check network connectivity and API response times");
     throw;
 }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during product export");
          throw;
      }
    }
}