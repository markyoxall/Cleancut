using CleanCut.Application.DTOs;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public interface IProductApiClientV1
{
    Task<List<ProductInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductInfo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProductInfo>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ProductInfo> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductInfo> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
