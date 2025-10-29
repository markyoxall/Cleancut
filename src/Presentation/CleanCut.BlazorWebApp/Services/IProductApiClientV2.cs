using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public interface IProductApiClientV2
{
    Task<V2ProductListResponse> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<V2ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<V2ProductListResponse> GetByCustomerAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<V2StatsResponse> GetStatisticsAsync(CancellationToken cancellationToken = default);
}