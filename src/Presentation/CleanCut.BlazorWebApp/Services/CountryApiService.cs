using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public class CountryApiService : ICountryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CountryApiService> _logger;

    public CountryApiService(HttpClient httpClient, ILogger<CountryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CountryInfo>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        var url = "/api/countries";
        _logger.LogInformation("Making GET request to: {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CountryInfo>>(cancellationToken: cancellationToken) ?? new List<CountryInfo>();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request to Country API - check token validity");
            throw new UnauthorizedAccessException("Token may be expired or invalid", ex);
        }
    }

    public async Task<CountryInfo?> GetCountryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/api/countries/{id}";
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
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request to Country API - check token validity");
            throw new UnauthorizedAccessException("Token may be expired or invalid", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching country {CountryId}", id);
            throw;
        }
    }
}