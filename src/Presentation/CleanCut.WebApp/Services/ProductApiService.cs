using CleanCut.Application.DTOs;

namespace CleanCut.WebApp.Services;

public interface IProductApiService
{
    Task<IEnumerable<ProductDto>> GetProductsByUserAsync(Guid userId);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(string name, string description, decimal price, Guid userId);
    Task<ProductDto> UpdateProductAsync(Guid id, string name, string description, decimal price);
    Task<bool> DeleteProductAsync(Guid id);
}

public class ProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiService> _logger;
    private readonly string _baseUrl;

    public ProductApiService(HttpClient httpClient, ILogger<ProductApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7142";
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByUserAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("?? Calling API: GET {BaseUrl}/api/v1/products?userId={UserId}", _baseUrl, userId);
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/products?userId={userId}");
            response.EnsureSuccessStatusCode();
            
            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new List<ProductDto>();
            
            _logger.LogInformation("? API returned {ProductCount} products for user {UserId}", products.Count, userId);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Products API for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("?? Calling API: GET {BaseUrl}/api/v1/products/{ProductId}", _baseUrl, id);
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/products/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<ProductDto>();
            
            _logger.LogInformation("? API returned product: {ProductName}", product?.Name);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Products API for ID {ProductId}", id);
            throw;
        }
    }

    public async Task<ProductDto> CreateProductAsync(string name, string description, decimal price, Guid userId)
    {
        try
        {
            _logger.LogInformation("?? Calling API: POST {BaseUrl}/api/v1/products", _baseUrl);
            
            var createRequest = new
            {
                Name = name,
                Description = description,
                Price = price,
                UserId = userId
            };
            
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/v1/products", createRequest);
            response.EnsureSuccessStatusCode();
            
            var product = await response.Content.ReadFromJsonAsync<ProductDto>();
            
            _logger.LogInformation("? API created product: {ProductName} with ID {ProductId}", 
                product?.Name, product?.Id);
            
            return product ?? throw new InvalidOperationException("Failed to create product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error creating product via API");
            throw;
        }
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, string name, string description, decimal price)
    {
        try
        {
            _logger.LogInformation("?? Calling API: PUT {BaseUrl}/api/v1/products/{ProductId}", _baseUrl, id);
            
            var updateRequest = new
            {
                Name = name,
                Description = description,
                Price = price
            };
            
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/v1/products/{id}", updateRequest);
            response.EnsureSuccessStatusCode();
            
            var product = await response.Content.ReadFromJsonAsync<ProductDto>();
            
            _logger.LogInformation("? API updated product: {ProductName}", product?.Name);
            
            return product ?? throw new InvalidOperationException("Failed to update product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error updating product via API");
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("?? Calling API: DELETE {BaseUrl}/api/v1/products/{ProductId}", _baseUrl, id);
            
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/v1/products/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("? API deleted product with ID {ProductId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error deleting product via API");
            throw;
        }
    }
}