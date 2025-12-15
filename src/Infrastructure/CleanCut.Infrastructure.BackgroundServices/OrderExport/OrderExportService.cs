using Microsoft.Extensions.Logging;
using CleanCut.Infrastructure.BackgroundServices.FileExport;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;
using CleanCut.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using CleanCut.Application.DTOs;
using AutoMapper;

namespace CleanCut.Infrastructure.BackgroundServices.OrderExport;

public interface IOrderExportService
{
    Task<string> ExportNewOrdersAsync(CancellationToken cancellationToken = default);
}

public class OrderExportService : IOrderExportService
{
    private readonly CleanCutDbContext _dbContext;
    private readonly ICsvExportService _csvService;
    private readonly ILogger<OrderExportService> _logger;
    private readonly IMapper _mapper;

    public OrderExportService(
        CleanCutDbContext dbContext,
        ICsvExportService csvService,
        ILogger<OrderExportService> logger,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _csvService = csvService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<string> ExportNewOrdersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Preparing to export new orders");

        // Get IDs of already exported orders
        var exportedIds = await _dbContext.ExportRecords
            .Select(r => r.OrderId)
            .ToListAsync(cancellationToken);

        // Fetch new orders that have not been exported yet
        var orders = await _dbContext.Orders
            .Include(o => o.OrderLineItems)
            .Include(o => o.Customer)
            .Where(o => !exportedIds.Contains(o.Id))
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            _logger.LogInformation("No new orders to export");
            return string.Empty;
        }

        // Map to DTOs
        var orderInfos = _mapper.Map<List<OrderInfo>>(orders);

        // Use CSV service to export - reuse product CSV method by overloading
        var filePath = await _csvService.ExportOrdersAsync(orderInfos, cancellationToken);

        // Record exported orders transactionally
        using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var o in orders)
            {
                _dbContext.ExportRecords.Add(new CleanCut.Infrastructure.Data.Entities.ExportRecord
                {
                    OrderId = o.Id,
                    ExportType = "orders",
                    ExportedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to record exported orders after CSV creation");
            throw;
        }

        _logger.LogInformation("Exported {Count} orders to {Path}", orders.Count, filePath);
        return filePath;
    }
}
