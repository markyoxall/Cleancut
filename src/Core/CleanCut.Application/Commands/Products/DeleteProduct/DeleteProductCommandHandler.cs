using MediatR;
using CleanCut.Domain.Repositories;
using CleanCut.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.Commands.Products.DeleteProduct;

/// <summary>
/// Handler for DeleteProductCommand
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService? _cacheService;
    private readonly ILogger<DeleteProductCommandHandler>? _logger;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork, ICacheService? cacheService = null, ILogger<DeleteProductCommandHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        // Get existing product
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return false; // Product not found
        }

        // Delete product
        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Detect common FK violation message from SQL Server and wrap it with a clearer message.
            var msg = ex.Message ?? string.Empty;
            var innerMsg = ex.InnerException?.Message ?? string.Empty;
            if (msg.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
                || innerMsg.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("The DELETE statement conflicted", StringComparison.OrdinalIgnoreCase)
                || innerMsg.Contains("The DELETE statement conflicted", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot delete product because it is referenced by existing order records.", ex);
            }

            throw;
        }

        // Defensive: remove cache entries if distributed cache available via DI
        try
        {
            if (_cacheService != null)
            {
                await _cacheService.RemoveAsync("products:all", cancellationToken);
                await _cacheService.RemoveAsync($"products:{product.Id}", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to remove cache after delete for product {ProductId}", product.Id);
        }

        return true;
    }
}
