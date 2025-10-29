using TempBlazorApp.Models;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace TempBlazorApp.Services;

public interface IProductService
{
    Task<List<Product>> GetProductsAsync();
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductService> _logger;
    private readonly ITokenService _tokenService;

    public ProductService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<ProductService> logger,
        ITokenService tokenService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _tokenService = tokenService;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            // Get API base URL from configuration
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            var apiUrl = $"{apiBaseUrl}/api/v1/products";

            _logger.LogInformation("Calling API: {ApiUrl}", apiUrl);

            // Get fresh access token from TokenService
            var accessToken = await _tokenService.GetAccessTokenAsync();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No access token available from TokenService");
                return new List<Product>();
            }

            // Set the authorization header with fresh token
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _logger.LogDebug("Bearer token attached to Product API request (from TokenService)");

            // Call the API with the fresh token
            var response = await _httpClient.GetAsync(apiUrl);
     
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API call failed with status: {StatusCode}, Reason: {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
 
                // If it's 401, token might be invalid
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("API returned 401 Unauthorized - token may be invalid");
                    return new List<Product>();
                }
 
                response.EnsureSuccessStatusCode();
            }

            // Deserialize the response
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
    
            _logger.LogInformation("Retrieved {ProductCount} products from API", products?.Count ?? 0);
            return products ?? new List<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling products API");
            return new List<Product>();
        }
    }
}