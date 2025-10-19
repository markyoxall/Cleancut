using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;

namespace CleanCut.BlazorWebApp.State;

public interface IProductsState
{
    IReadOnlyList<ProductDto> Products { get; }
    event Action? StateChanged;
    event Action<List<ProductDto>>? ProductsChanged;

    bool IsLoading { get; }
    event Action<string, bool>? MessageChanged;

    // CancellationToken added for graceful cancellation
    Task LoadAllAsync(bool force = false, CancellationToken cancellationToken = default);
    Task LoadByUserAsync(Guid userId, bool force = false, CancellationToken cancellationToken = default);
    Task<ProductDto?> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    void Invalidate();
}