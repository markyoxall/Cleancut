using CleanCut.Application.DTOs;
using System.Net.Http.Json;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public interface ICountryApiService
{
    Task<List<CountryInfo>> GetAllCountriesAsync(CancellationToken cancellationToken = default);
    Task<CountryInfo?> GetCountryByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class CountryApiService : ICountryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CountryApiService> _logger;
    private const string BaseUrl = "https://localhost:7142";

    public CountryApiService(HttpClient httpClient, ILogger<CountryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CountryInfo>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/api/countries";
        _logger.LogInformation("Making GET request to: {Url}", url);
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CountryInfo>>(cancellationToken: cancellationToken) ?? new List<CountryInfo>();
    }

    public async Task<CountryInfo?> GetCountryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{BaseUrl}/api/countries/{id}";
            _logger.LogInformation("Making GET request to: {Url}", url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CountryInfo>(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetCountryByIdAsync canceled for {CountryId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching country {CountryId}", id);
            throw;
        }
    }
}