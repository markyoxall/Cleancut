using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.Services;

public interface IUserApiService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
}

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserApiService> _logger;
    private const string BaseUrl = "https://localhost:7142";

    public UserApiService(HttpClient httpClient, ILogger<UserApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var url = $"{BaseUrl}/api/users/{id}";
            _logger.LogInformation("Making GET request to: {Url}", url);
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var url = $"{BaseUrl}/api/users";
            _logger.LogInformation("Making POST request to: {Url} for user {Email}", url, request.Email);
            
            // Create the command structure that matches the API expectation
            var command = new
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };
            
            var response = await _httpClient.PostAsJsonAsync(url, command);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
            }
            
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserDto>() 
                ?? throw new InvalidOperationException("Failed to create user");
            
            _logger.LogInformation("Successfully created user {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", request.Email);
            throw;
        }
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        try
        {
            var url = $"{BaseUrl}/api/users/{id}";
            _logger.LogInformation("Making PUT request to: {Url} for user {Email}", url, request.Email);
            
            // Create the command structure that matches the API expectation
            var command = new
            {
                Id = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };
            
            var response = await _httpClient.PutAsJsonAsync(url, command);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
            }
            
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserDto>() 
                ?? throw new InvalidOperationException("Failed to update user");
            
            _logger.LogInformation("Successfully updated user {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        try
        {
            var url = $"{BaseUrl}/api/users/{id}";
            _logger.LogInformation("Making DELETE request to: {Url}", url);
            
            // Add timeout for debugging
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await _httpClient.DeleteAsync(url, cts.Token);
            
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
            
            // Log other non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Delete failed for user {UserId}. Status: {StatusCode}, Error: {ErrorContent}", 
                    id, response.StatusCode, errorContent);
                throw new HttpRequestException($"Delete failed with status {response.StatusCode}: {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout while deleting user {UserId}", id);
            throw new HttpRequestException($"Request timeout while deleting user {id}", ex);
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
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}