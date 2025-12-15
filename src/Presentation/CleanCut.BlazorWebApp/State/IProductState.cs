using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;

namespace CleanCut.BlazorWebApp.State;

public interface IProductsState
{
    IReadOnlyList<ProductInfo> Products { get; }
    event Action? StateChanged;
    event Action<List<ProductInfo>>? ProductsChanged;

    bool IsLoading { get; }
    event Action<string, bool>? MessageChanged;

    // CancellationToken added for graceful cancellation
    Task LoadAllAsync(bool force = false, CancellationToken cancellationToken = default);
    Task LoadByCustomerAsync(Guid userId, bool force = false, CancellationToken cancellationToken = default);
    Task<ProductInfo?> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductInfo?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    void Invalidate();
}