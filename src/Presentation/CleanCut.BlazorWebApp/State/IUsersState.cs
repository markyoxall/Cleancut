using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;

namespace CleanCut.BlazorWebApp.State;

public interface IUsersState
{
    IReadOnlyList<UserInfo> Users { get; }
    event Action? StateChanged;
    event Action<List<UserInfo>>? UsersChanged;

    // UI support
    bool IsLoading { get; }
    event Action<string, bool>? MessageChanged;

    // CancellationToken added to support graceful shutdown & cancellation
    Task LoadAsync(bool force = false, CancellationToken cancellationToken = default);
    Task<UserInfo?> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserInfo?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    void Invalidate();
}