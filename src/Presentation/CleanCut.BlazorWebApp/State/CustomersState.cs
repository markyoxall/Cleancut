using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;

namespace CleanCut.BlazorWebApp.State;

public class CustomersState : ICustomersState, IAsyncDisposable
{
    private readonly ICustomerApiService _customerApi;
    private readonly ILogger<CustomersState> _logger;
    // SignalR connection removed - notifications are handled by centralized domain-event pipeline

    private List<CustomerInfo> _users = new();
    private DateTime _lastLoaded = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    private bool _isLoading;
    public bool IsLoading => _isLoading;
    public event Action<string, bool>? MessageChanged;

    public CustomersState(ICustomerApiService customerApi, ILogger<CustomersState> logger)
    {
        _customerApi = customerApi;
        _logger = logger;
    }

    public IReadOnlyList<CustomerInfo> Customers => _users;
    public event Action? StateChanged;
    public event Action<List<CustomerInfo>>? CustomersChanged;

    public async Task LoadAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        if (!force && DateTime.UtcNow - _lastLoaded < _cacheExpiry && _users.Any())
        {
            _logger.LogDebug("CustomersState: using cached customers");
            return;
        }

        _isLoading = true;
        StateChanged?.Invoke();

        try
        {
            _logger.LogInformation("CustomersState: loading customers");
            _users = await _customerApi.GetAllCustomersAsync(cancellationToken);
            _lastLoaded = DateTime.UtcNow;

            var usersCopy = _users.ToList();
            CustomersChanged?.Invoke(usersCopy);
            StateChanged?.Invoke();

            _logger.LogInformation("CustomersState: loaded {Count} customers", _users.Count);
            MessageChanged?.Invoke("Customers loaded", true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CustomersState: LoadAsync canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CustomersState: error loading customers");
            MessageChanged?.Invoke("Failed to load customers", false);
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task<CustomerInfo?> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        _isLoading = true;
        StateChanged?.Invoke();
        try
        {
            var created = await _customerApi.CreateCustomerAsync(request, cancellationToken);
            _users.Add(created);

            var usersCopy = _users.ToList();
            CustomersChanged?.Invoke(usersCopy);
            StateChanged?.Invoke();
            MessageChanged?.Invoke($"Customer '{created.FirstName} {created.LastName}' created", true);
            return created;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CustomersState: CreateAsync canceled for {Email}", request.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CustomersState: error creating customer");
            MessageChanged?.Invoke("Failed to create customer", false);
            return null;
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task<CustomerInfo?> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        _isLoading = true;
        StateChanged?.Invoke();
        try
        {
            var updated = await _customerApi.UpdateCustomerAsync(id, request, cancellationToken);
            var idx = _users.FindIndex(u => u.Id == id);
            if (idx >= 0) _users[idx] = updated;

            var usersCopy = _users.ToList();
            CustomersChanged?.Invoke(usersCopy);
            StateChanged?.Invoke();
            MessageChanged?.Invoke($"Customer '{updated.FirstName} {updated.LastName}' updated", true);
            return updated;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CustomersState: UpdateAsync canceled for {CustomerId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CustomersState: error updating customer");
            MessageChanged?.Invoke("Failed to update customer", false);
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
            var success = await _customerApi.DeleteCustomerAsync(id, cancellationToken);
            if (success)
            {
                _users.RemoveAll(u => u.Id == id);
                var usersCopy = _users.ToList();
                CustomersChanged?.Invoke(usersCopy);
                StateChanged?.Invoke();
                MessageChanged?.Invoke("Customer deleted", true);
            }
            return success;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CustomersState: DeleteAsync canceled for {CustomerId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CustomersState: error deleting customer");
            MessageChanged?.Invoke("Failed to delete customer", false);
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

    private void InitializeSignalR()
    {
        // Intentionally left empty. SignalR client logic removed in favor of server-driven cache invalidation and DI-driven state updates.
    }

    public async ValueTask DisposeAsync()
    {
        // No resources to dispose
    }
}
