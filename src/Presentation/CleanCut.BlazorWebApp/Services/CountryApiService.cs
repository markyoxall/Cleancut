using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string BaseUrl = "https://localhost:7142";

    public CountryApiService(HttpClient httpClient, ILogger<CountryApiService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<bool> AttachAccessTokenAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // In Blazor Server with SaveTokens=true, use GetTokenAsync
                var accessToken = await httpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    _logger.LogDebug("Bearer token attached to Country API request (length: {Length})", accessToken.Length);
                    return true;
                }
                else
                {
                    _logger.LogWarning("User is authenticated but access token is null or empty in SaveTokens storage");
                }
            }
            else
            {
                _logger.LogWarning("User is not authenticated for Country API calls");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access token for Country API");
        }

        // Clear any existing authorization header if no token
        _httpClient.DefaultRequestHeaders.Authorization = null;
        return false;
    }

    public async Task<List<CountryInfo>> GetAllCountriesAsync(CancellationToken cancellationToken = default)
    {
        var hasToken = await AttachAccessTokenAsync();
        var url = $"{BaseUrl}/api/countries";
        _logger.LogInformation("Making GET request to: {Url} (Token: {HasToken})", url, hasToken);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CountryInfo>>(cancellationToken: cancellationToken) ?? new List<CountryInfo>();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request to Country API - user may need to login");
            throw new UnauthorizedAccessException("Please login to access the Country API", ex);
        }
    }

    public async Task<CountryInfo?> GetCountryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasToken = await AttachAccessTokenAsync();
            var url = $"{BaseUrl}/api/countries/{id}";
            _logger.LogInformation("Making GET request to: {Url} (Token: {HasToken})", url, hasToken);

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
            _logger.LogWarning("Unauthorized request to Country API - user may need to login");
            throw new UnauthorizedAccessException("Please login to access the Country API", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching country {CountryId}", id);
            throw;
        }
    }
}