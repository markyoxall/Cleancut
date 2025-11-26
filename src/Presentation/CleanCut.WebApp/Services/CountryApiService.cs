using CleanCut.Application.DTOs;

namespace CleanCut.WebApp.Services;

public interface ICountryApiService
{
    Task<List<CountryInfo>> GetAllCountriesAsync();

    Task<CountryInfo?> GetCountryByIdAsync(Guid id);



    Task<CountryInfo> CreateCountryAsync(string name, string description, decimal price, Guid customerId);
    Task<CountryInfo> UpdateCountryAsync(Guid id, string name, string description, decimal price);
    Task<bool> DeleteCountryAsync(Guid id);

}


public class CountryApiService : ICountryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CountryApiService> _logger;

    public CountryApiService(HttpClient httpClient, ILogger<CountryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CountryInfo>> GetAllCountriesAsync()
    {
        try
        {
            _logger.LogInformation("Calling API: GET /api/countries");

            var response = await _httpClient.GetAsync("/api/countries");
            response.EnsureSuccessStatusCode();

            var countries = await response.Content.ReadFromJsonAsync<List<CountryInfo>>() ?? new List<CountryInfo>();

            _logger.LogInformation("API returned {CountryCount} countries", countries.Count);
            return countries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Countrys API for all countries");
            throw;
        }
    }

    public async Task<CountryInfo?> GetCountryByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Calling API: GET /api/v1/countries/{CountryId}", id);

            var response = await _httpClient.GetAsync($"/api/v1/countries/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<CountryInfo>();

            _logger.LogInformation("? API returned product: {CountryName}", product?.Name);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Countrys API for ID {CountryId}", id);
            throw;
        }
    }

    public async Task<CountryInfo> CreateCountryAsync(string name, string description, decimal price, Guid customerId)
    {
        try
        {
            _logger.LogInformation("Calling API: POST /api/v1/countries");

            var createRequest = new
            {

            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/countries", createRequest);
            response.EnsureSuccessStatusCode();

            var product = await response.Content.ReadFromJsonAsync<CountryInfo>();

            _logger.LogInformation("? API created product: {CountryName} with ID {CountryId}",
                product?.Name, product?.Id);

            return product ?? throw new InvalidOperationException("Failed to create product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error creating product via API");
            throw;
        }
    }

    public async Task<CountryInfo> UpdateCountryAsync(Guid id, string name, string description, decimal price)
    {
        try
        {
            _logger.LogInformation("Calling API: PUT /api/v1/countries/{CountryId}", id);

            var updateRequest = new
            {

            };

            var response = await _httpClient.PutAsJsonAsync($"/api/v1/countries/{id}", updateRequest);
            response.EnsureSuccessStatusCode();

            var product = await response.Content.ReadFromJsonAsync<CountryInfo>();

            _logger.LogInformation("? API updated product: {CountryName}", product?.Name);

            return product ?? throw new InvalidOperationException("Failed to update product");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error updating product via API");
            throw;
        }
    }

    public async Task<bool> DeleteCountryAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Calling API: DELETE /api/v1/countries/{CountryId}", id);

            var response = await _httpClient.DeleteAsync($"/api/v1/countries/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            _logger.LogInformation("? API deleted product with ID {CountryId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error deleting product via API");
            throw;
        }
    }
}
