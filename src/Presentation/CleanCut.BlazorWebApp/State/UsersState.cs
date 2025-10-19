using System.Threading;
using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;
using Microsoft.Extensions.Logging;

namespace CleanCut.BlazorWebApp.State;

public class UsersState : IUsersState
{
    private readonly IUserApiService _userApi;
    private readonly ILogger<UsersState> _logger;

    private List<UserDto> _users = new();
    private DateTime _lastLoaded = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    // UI state / messages
    private bool _isLoading;
    public bool IsLoading => _isLoading;
    public event Action<string, bool>? MessageChanged;

    public UsersState(IUserApiService userApi, ILogger<UsersState> logger)
    {
        _userApi = userApi;
        _logger = logger;
    }

    public IReadOnlyList<UserDto> Users => _users;
    public event Action? StateChanged;
    public event Action<List<UserDto>>? UsersChanged;

    public async Task LoadAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        if (!force && DateTime.UtcNow - _lastLoaded < _cacheExpiry && _users.Any())
        {
            _logger.LogDebug("UsersState: using cached users");
            return;
        }

        _isLoading = true;
        StateChanged?.Invoke();

        try
        {
            _logger.LogInformation("UsersState: loading users");
            _users = await _userApi.GetAllUsersAsync(cancellationToken);
            _lastLoaded = DateTime.UtcNow;

            var usersCopy = _users.ToList();
            UsersChanged?.Invoke(usersCopy);
            StateChanged?.Invoke();

            _logger.LogInformation("UsersState: loaded {Count} users", _users.Count);
            MessageChanged?.Invoke("Users loaded", true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UsersState: LoadAsync canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UsersState: error loading users");
            MessageChanged?.Invoke("Failed to load users", false);
            // swallow to keep UI stable; callers can react to MessageChanged
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task<UserDto?> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        _isLoading = true;
        StateChanged?.Invoke();
        try
        {
            var created = await _userApi.CreateUserAsync(request, cancellationToken);
            _users.Add(created);

            var usersCopy = _users.ToList();
            UsersChanged?.Invoke(usersCopy);
            StateChanged?.Invoke();
            MessageChanged?.Invoke($"User '{created.FirstName} {created.LastName}' created", true);
            return created;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UsersState: CreateAsync canceled for {Email}", request.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UsersState: error creating user");
            MessageChanged?.Invoke("Failed to create user", false);
            return null;
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        _isLoading = true;
        StateChanged?.Invoke();
        try
        {
            var updated = await _userApi.UpdateUserAsync(id, request, cancellationToken);
            var idx = _users.FindIndex(u => u.Id == id);
            if (idx >= 0) _users[idx] = updated;

            var usersCopy = _users.ToList();
            UsersChanged?.Invoke(usersCopy);
            StateChanged?.Invoke();
            MessageChanged?.Invoke($"User '{updated.FirstName} {updated.LastName}' updated", true);
            return updated;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UsersState: UpdateAsync canceled for {UserId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UsersState: error updating user");
            MessageChanged?.Invoke("Failed to update user", false);
            return null;
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        _isLoading = true;
        StateChanged?.Invoke();
        try
        {
            var success = await _userApi.DeleteUserAsync(id, cancellationToken);
            if (success)
            {
                _users.RemoveAll(u => u.Id == id);
                var usersCopy = _users.ToList();
                UsersChanged?.Invoke(usersCopy);
                StateChanged?.Invoke();
                MessageChanged?.Invoke("User deleted", true);
            }
            return success;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UsersState: DeleteAsync canceled for {UserId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UsersState: error deleting user");
            MessageChanged?.Invoke("Failed to delete user", false);
            return false;
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public void Invalidate()
    {
        _lastLoaded = DateTime.MinValue;
        StateChanged?.Invoke();
    }
}