using CleanCut.Application.DTOs;

namespace CleanCut.WebApp.Services;

public interface ICustomerApiService
{
    Task<IEnumerable<CustomerInfo>> GetAllCustomersAsync();
    Task<CustomerInfo?> GetCustomerByIdAsync(Guid id);
    Task<CustomerInfo> CreateCustomerAsync(string firstName, string lastName, string email);
    Task<CustomerInfo> UpdateCustomerAsync(Guid id, string firstName, string lastName, string email);
    Task<bool> DeleteCustomerAsync(Guid id);
}

[System.Obsolete("UserApiService is renamed to CustomerApiService. Use ICustomerApiService instead.")]
public interface IUserApiService : ICustomerApiService { }

public class CustomerApiService : ICustomerApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerApiService> _logger;
    private readonly string _baseUrl;

    public CustomerApiService(HttpClient httpClient, ILogger<CustomerApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7142";
    }

    public async Task<IEnumerable<CustomerInfo>> GetAllCustomersAsync()
    {
        try
        {
            _logger.LogInformation("Calling API: GET {BaseUrl}/api/customers", _baseUrl);

            var response = await _httpClient.GetAsync($"{_baseUrl}/api/customers");
            response.EnsureSuccessStatusCode();

            var customers = await response.Content.ReadFromJsonAsync<List<CustomerInfo>>() ?? new List<CustomerInfo>();

            _logger.LogInformation("API returned {Count} customers", customers.Count);
            return customers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Customers API");
            throw;
        }
    }

    public async Task<CustomerInfo?> GetCustomerByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Calling API: GET {BaseUrl}/api/customers/{Id}", _baseUrl, id);
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/customers/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Customers API for ID {Id}", id);
            throw;
        }
    }

    public async Task<CustomerInfo> CreateCustomerAsync(string firstName, string lastName, string email)
    {
        try
        {
            _logger.LogInformation("Calling API: POST {BaseUrl}/api/customers", _baseUrl);

            var createRequest = new { FirstName = firstName, LastName = lastName, Email = email };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/customers", createRequest);
            response.EnsureSuccessStatusCode();

            var customer = await response.Content.ReadFromJsonAsync<CustomerInfo>();
            return customer ?? throw new InvalidOperationException("Failed to create customer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer via API");
            throw;
        }
    }

    public async Task<CustomerInfo> UpdateCustomerAsync(Guid id, string firstName, string lastName, string email)
    {
        try
        {
            _logger.LogInformation("Calling API: PUT {BaseUrl}/api/customers/{Id}", _baseUrl, id);
            var updateRequest = new { Id = id.ToString(), FirstName = firstName, LastName = lastName, Email = email };
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/customers/{id}", updateRequest);
            response.EnsureSuccessStatusCode();

            var customer = await response.Content.ReadFromJsonAsync<CustomerInfo>();
            return customer ?? throw new InvalidOperationException("Failed to update customer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer via API");
            throw;
        }
    }

    public async Task<bool> DeleteCustomerAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Calling API: DELETE {BaseUrl}/api/customers/{Id}", _baseUrl, id);
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/customers/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer via API");
            throw;
        }
    }
}