using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public interface IUserApiService
{
    Task<List<UserInfo>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserInfo?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserInfo> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserInfo> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
}

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserApiService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string BaseUrl = "https://localhost:7142";

    public UserApiService(HttpClient httpClient, ILogger<UserApiService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task AttachAccessTokenAsync()
    {
        var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(accessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    public async Task<List<UserInfo>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        try
        {
            var url = $"{BaseUrl}/api/users";
            _logger.LogInformation("Making GET request to: {Url}", url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<List<UserInfo>>(cancellationToken: cancellationToken) ?? new();
            _logger.LogInformation("Fetched {Count} users", users.Count);
            return users;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllUsersAsync canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            throw;
        }
    }

    public async Task<UserInfo?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        try
        {
            var url = $"{BaseUrl}/api/users/{id}";
            _logger.LogInformation("Making GET request to: {Url}", url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetUserByIdAsync canceled for {UserId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", id);
            throw;
        }
    }

    public async Task<UserInfo> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        try
        {
            var url = $"{BaseUrl}/api/users";
            _logger.LogInformation("Making POST request to: {Url} for user {Email}", url, request.Email);

            var command = new
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            var response = await _httpClient.PostAsJsonAsync(url, command, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
            }

            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to create user");

            _logger.LogInformation("Successfully created user {UserId}", user.Id);
            return user;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateUserAsync canceled for {Email}", request.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", request.Email);
            throw;
        }
    }

    public async Task<UserInfo> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        try
        {
            var url = $"{BaseUrl}/api/users/{id}";
            _logger.LogInformation("Making PUT request to: {Url} for user {Email}", url, request.Email);

            var command = new
            {
                Id = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            var response = await _httpClient.PutAsJsonAsync(url, command, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
            }

            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to update user");

            _logger.LogInformation("Successfully updated user {UserId}", user.Id);
            return user;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateUserAsync canceled for {UserId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        try
        {
            var url = $"{BaseUrl}/api/users/{id}";
            _logger.LogInformation("Making DELETE request to: {Url}", url);

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

            var response = await _httpClient.DeleteAsync(url, linked.Token);

            _logger.LogInformation("DELETE request completed. Status: {StatusCode}, Reason: {ReasonPhrase}",
                response.StatusCode, response.ReasonPhrase);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User {UserId} not found during delete - may have already been deleted", id);
                return false; // User not found
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                _logger.LogInformation("User {UserId} deleted successfully", id);
                return true; // Successfully deleted
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Delete failed for user {UserId}. Status: {StatusCode}, Error: {ErrorContent}",
                    id, response.StatusCode, errorContent);
                throw new HttpRequestException($"Delete failed with status {response.StatusCode}: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteUserAsync canceled for {UserId}", id);
            throw;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error while deleting user {UserId}. Message: {Message}", id, httpEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting user {UserId}", id);
            throw;
        }
    }
}

// Local request models for API calls

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}