using CleanCut.Application.DTOs;
using CleanCut.Domain.Entities;

namespace CleanCut.WebApp.Services;

public interface IProductApiService
{
    Task<List<ProductInfo>> GetAllProductsAsync();
    Task<IEnumerable<ProductInfo>> GetProductsByCustomerAsync(Guid customerId);
    Task<ProductInfo?> GetProductByIdAsync(Guid id);
    Task<ProductInfo> CreateProductAsync(string name, string description, decimal price, Guid customerId);
    Task<ProductInfo> UpdateProductAsync(Guid id, string name, string description, decimal price);
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

    public async Task<List<ProductInfo>> GetAllProductsAsync()
    {
     
        try
        {
            _logger.LogInformation("Calling API: GET {BaseUrl}/api/v1/products}", _baseUrl);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/products");
            response.EnsureSuccessStatusCode();

            var products = await response.Content.ReadFromJsonAsync<List<ProductInfo>>() ?? new List<ProductInfo>();

            _logger.LogInformation("API returned {ProductCount} products", products.Count);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Products API for all products");
            throw;
        }



    }


    public async Task<IEnumerable<ProductInfo>> GetProductsByCustomerAsync(Guid customerId)
    {
        try
        {
            // Fixed: changed from "customer" to "user" to match API endpoint
            _logger.LogInformation("Calling API: GET {BaseUrl}/api/v1/products/user/{CustomerId}", _baseUrl, customerId);
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/products/user/{customerId}");
            response.EnsureSuccessStatusCode();
            
            var products = await response.Content.ReadFromJsonAsync<List<ProductInfo>>() ?? new List<ProductInfo>();
            
            _logger.LogInformation("API returned {ProductCount} products for customer {CustomerId}", products.Count, customerId);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Products API for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<ProductInfo?> GetProductByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Calling API: GET {BaseUrl}/api/v1/products/{ProductId}", _baseUrl, id);
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/products/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<ProductInfo>();
            
            _logger.LogInformation("? API returned product: {ProductName}", product?.Name);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Products API for ID {ProductId}", id);
            throw;
        }
    }

    public async Task<ProductInfo> CreateProductAsync(string name, string description, decimal price, Guid customerId)
    {
        try
        {
            _logger.LogInformation("Calling API: POST {BaseUrl}/api/v1/products", _baseUrl);
            
            var createRequest = new
            {
                Name = name,
                Description = description,
                Price = price,
                CustomerId = customerId
            };
            
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/v1/products", createRequest);
            response.EnsureSuccessStatusCode();
            
            var product = await response.Content.ReadFromJsonAsync<ProductInfo>();
            
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

    public async Task<ProductInfo> UpdateProductAsync(Guid id, string name, string description, decimal price)
    {
        try
        {
            _logger.LogInformation("Calling API: PUT {BaseUrl}/api/v1/products/{ProductId}", _baseUrl, id);
            
            var updateRequest = new
            {
                Name = name,
                Description = description,
                Price = price
            };
            
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/v1/products/{id}", updateRequest);
            response.EnsureSuccessStatusCode();
            
            var product = await response.Content.ReadFromJsonAsync<ProductInfo>();
            
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
            _logger.LogInformation("Calling API: DELETE {BaseUrl}/api/v1/products/{ProductId}", _baseUrl, id);
            
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