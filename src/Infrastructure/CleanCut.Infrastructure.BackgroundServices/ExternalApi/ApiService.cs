using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CleanCut.Application.DTOs;


namespace CleanCut.Infrastructure.BackgroundServices.ExternalApi;

/// <summary>
/// Service for calling the CleanCut API
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Fetches all products from the API
    /// </summary>
  Task<IReadOnlyList<ProductInfo>> GetAllProductsAsync(string accessToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of API service for CleanCut API
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _settings;
  private readonly ILogger<ApiService> _logger;
  private readonly JsonSerializerOptions _jsonOptions;

public ApiService(
      HttpClient httpClient,
        IOptions<ApiSettings> settings,
 ILogger<ApiService> logger)
 {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Configure JSON options for case-insensitive deserialization
        _jsonOptions = new JsonSerializerOptions
        {
PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
   };

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
   _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<IReadOnlyList<ProductInfo>> GetAllProductsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
      _logger.LogInformation("Fetching products from API: {Endpoint}", _settings.ProductsEndpoint);

   try
 {
    // Add authorization header
     _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

       // Make API call
            var response = await _httpClient.GetAsync(_settings.ProductsEndpoint, cancellationToken);

   // Check for success
          if (!response.IsSuccessStatusCode)
     {
         _logger.LogError("API call failed with status code: {StatusCode}, Reason: {ReasonPhrase}", 
  response.StatusCode, response.ReasonPhrase);
   
      if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
         throw new UnauthorizedAccessException("API call was unauthorized. Check access token and API configuration.");
                }

      throw new HttpRequestException($"API call failed with status code: {response.StatusCode}");
     }

            // Read and deserialize response
  var content = await response.Content.ReadAsStringAsync(cancellationToken);
       
  if (string.IsNullOrWhiteSpace(content))
      {
          _logger.LogWarning("API returned empty response");
       return Array.Empty<ProductInfo>();
       }

      var products = JsonSerializer.Deserialize<List<ProductInfo>>(content, _jsonOptions);
      
            if (products == null)
      {
     _logger.LogWarning("Failed to deserialize products from API response");
   return Array.Empty<ProductInfo>();
       }

     _logger.LogInformation("Successfully fetched {Count} products from API", products.Count);
  return products.AsReadOnly();
        }
    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
       _logger.LogError(ex, "API call timed out after {Timeout} seconds", _settings.TimeoutSeconds);
     throw new TimeoutException($"API call timed out after {_settings.TimeoutSeconds} seconds", ex);
 }
      catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is HttpRequestException || ex is TimeoutException))
   {
            _logger.LogError(ex, "Unexpected error while calling API");
    throw new InvalidOperationException("Failed to fetch products from API", ex);
        }
  finally
 {
     // Clear authorization header for security
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
 }
}