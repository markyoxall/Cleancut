using CleanCut.Application.DTOs;

namespace CleanCut.WebApp.Services;

public interface IUserApiService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(string firstName, string lastName, string email);
    Task<UserDto> UpdateUserAsync(Guid id, string firstName, string lastName, string email);
    Task<bool> DeleteUserAsync(Guid id);
}

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserApiService> _logger;
    private readonly string _baseUrl;

    public UserApiService(HttpClient httpClient, ILogger<UserApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7142";
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("?? Calling API: GET {BaseUrl}/api/users", _baseUrl);
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/users");
            response.EnsureSuccessStatusCode();
            
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
            
            _logger.LogInformation("? API returned {UserCount} users", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Users API");
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("?? Calling API: GET {BaseUrl}/api/users/{UserId}", _baseUrl, id);
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/users/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            
            _logger.LogInformation("? API returned user: {UserName}", user?.FirstName + " " + user?.LastName);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error calling Users API for ID {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto> CreateUserAsync(string firstName, string lastName, string email)
    {
        try
        {
            _logger.LogInformation("?? Calling API: POST {BaseUrl}/api/users", _baseUrl);
            
            var createRequest = new
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
            
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/users", createRequest);
            response.EnsureSuccessStatusCode();
            
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            
            _logger.LogInformation("? API created user: {UserName} with ID {UserId}", 
                user?.FirstName + " " + user?.LastName, user?.Id);
            
            return user ?? throw new InvalidOperationException("Failed to create user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error creating user via API");
            throw;
        }
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, string firstName, string lastName, string email)
    {
        try
        {
            _logger.LogInformation("?? Calling API: PUT {BaseUrl}/api/users/{UserId}", _baseUrl, id);
            
            var updateRequest = new
            {
                Id = id.ToString(),
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
            
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/users/{id}", updateRequest);
            response.EnsureSuccessStatusCode();
            
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            
            _logger.LogInformation("? API updated user: {UserName}", user?.FirstName + " " + user?.LastName);
            
            return user ?? throw new InvalidOperationException("Failed to update user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error updating user via API");
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("?? Calling API: DELETE {BaseUrl}/api/users/{UserId}", _baseUrl, id);
            
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/users/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("? API deleted user with ID {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error deleting user via API");
            throw;
        }
    }
}