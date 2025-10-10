using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.Services;

public interface IProductApiService
{
    Task<List<ProductDto>> GetProductsByUserAsync(Guid userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(Guid id);
}

public class ProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiService> _logger;
    private const string BaseUrl = "https://localhost:7142";

    public ProductApiService(HttpClient httpClient, ILogger<ProductApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ProductDto>> GetProductsByUserAsync(Guid userId)
    {
        try
        {
            var url = $"{BaseUrl}/api/v1/products/user/{userId}";
            _logger.LogInformation("Making GET request to: {Url}", url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new();
            _logger.LogInformation("Fetched {Count} products for user {UserId}", products.Count, userId);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var url = $"{BaseUrl}/api/users";
            _logger.LogInformation("Making GET request to: {Url}", url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new();
            _logger.LogInformation("Fetched {Count} users", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            throw;
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        try
        {
            var url = $"{BaseUrl}/api/v1/products/{id}";
            _logger.LogInformation("Making GET request to: {Url}", url);
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId}", id);
            throw;
        }
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var url = $"{BaseUrl}/api/v1/products";
            _logger.LogInformation("Making POST request to: {Url} for product {ProductName}", url, request.Name);
            var response = await _httpClient.PostAsJsonAsync(url, request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
            }
            
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<ProductDto>() 
                ?? throw new InvalidOperationException("Failed to create product");
            
            _logger.LogInformation("Successfully created product {ProductId}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {ProductName}", request.Name);
            throw;
        }
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        try
        {
            var url = $"{BaseUrl}/api/v1/products/{id}";
            _logger.LogInformation("Making PUT request to: {Url} for product {ProductName}", url, request.Name);
            
            // Create the command structure that matches the API expectation
            var command = new
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };
            
            var response = await _httpClient.PutAsJsonAsync(url, command);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
            }
            
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<ProductDto>() 
                ?? throw new InvalidOperationException("Failed to update product");
            
            _logger.LogInformation("Successfully updated product {ProductId}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        try
        {
            var url = $"{BaseUrl}/api/v1/products/{id}";
            _logger.LogInformation("Making DELETE request to: {Url}", url);
            
            // Add timeout for debugging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await _httpClient.DeleteAsync(url, cts.Token);
            
            _logger.LogInformation("DELETE request completed. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                response.StatusCode, response.ReasonPhrase);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Product {ProductId} not found during delete - may have already been deleted", id);
                return false; // Product not found
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                _logger.LogInformation("Product {ProductId} deleted successfully", id);
                return true; // Successfully deleted
            }
            
            // Log other non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Delete failed for product {ProductId}. Status: {StatusCode}, Error: {ErrorContent}", 
                    id, response.StatusCode, errorContent);
                throw new HttpRequestException($"Delete failed with status {response.StatusCode}: {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout while deleting product {ProductId}", id);
            throw new HttpRequestException($"Request timeout while deleting product {id}", ex);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error while deleting product {ProductId}. Message: {Message}", id, httpEx.Message);
            // Re-throw HTTP exceptions so they can be handled specifically
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting product {ProductId}", id);
            throw;
        }
    }
}

// Local request models for API calls
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
