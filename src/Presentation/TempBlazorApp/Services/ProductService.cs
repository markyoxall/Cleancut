using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace TempBlazorApp.Services;

public interface IProductService
{
    Task<List<ProductInfo>> GetProductsAsync();
}

public class ProductService : IProductService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductService(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<ProductService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ProductInfo>> GetProductsAsync()
    {
        try
        {
          // Create a new HttpClient for this request to avoid disposal issues
        using var httpClient = _httpClientFactory.CreateClient();
      
     // Get API base URL from configuration
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
      var apiUrl = $"{apiBaseUrl}/api/v1/products";

          _logger.LogInformation("Calling API: {ApiUrl}", apiUrl);

  // ? Get user's access token from authentication context
 var httpContext = _httpContextAccessor.HttpContext;
            
        if (httpContext != null && httpContext.User.Identity?.IsAuthenticated == true)
        {
       // Get the user's access token from the authentication session
     var accessToken = await httpContext.GetTokenAsync("access_token");

      if (!string.IsNullOrEmpty(accessToken))
           {
         // Set the authorization header with user's token
     httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
          _logger.LogDebug("User Bearer token attached to Product API request");
             }
     else
       {
       _logger.LogWarning("User is authenticated but no access token found in session");
return new List<ProductInfo>();
         }
            }
        else
          {
         _logger.LogWarning("User not authenticated - cannot call API");
         return new List<ProductInfo>();
        }

         // Call the API with the user's token
            var response = await httpClient.GetAsync(apiUrl);
        
    if (!response.IsSuccessStatusCode)
            {
              _logger.LogWarning("API call failed with status: {StatusCode}, Reason: {ReasonPhrase}", 
           response.StatusCode, response.ReasonPhrase);

 // If it's 401, user might need to re-authenticate
    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
     {
          _logger.LogWarning("API returned 401 Unauthorized - user may need to re-authenticate");
     return new List<ProductInfo>();
             }

    response.EnsureSuccessStatusCode();
    }

    // Deserialize the response to ProductInfo (the correct DTO)
     var products = await response.Content.ReadFromJsonAsync<List<ProductInfo>>();
            
       _logger.LogInformation("Retrieved {ProductCount} products from API", products?.Count ?? 0);
return products ?? new List<ProductInfo>();
 }
      catch (Exception ex)
        {
  _logger.LogError(ex, "Error calling products API");
 return new List<ProductInfo>();
 }
    }
}