using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;

namespace CleanCut.BlazorWebApp.State;

public interface ICustomersState
{
    IReadOnlyList<CustomerInfo> Customers { get; }
    event Action? StateChanged;
    event Action<List<CustomerInfo>>? CustomersChanged;

  // UI support
  bool IsLoading { get; }
    event Action<string, bool>? MessageChanged;

    // CancellationToken added to support graceful shutdown & cancellation
 Task LoadAsync(bool force = false, CancellationToken cancellationToken = default);
  Task<CustomerInfo?> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
 Task<CustomerInfo?> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
 void Invalidate();
}