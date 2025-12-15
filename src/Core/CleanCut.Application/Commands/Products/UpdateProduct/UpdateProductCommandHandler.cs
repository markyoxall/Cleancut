using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;
using CleanCut.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.Commands.Products.UpdateProduct;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductInfo>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductCommandHandler> _logger;
    private readonly ICacheService? _cacheService;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateProductCommandHandler> logger, ICacheService? cacheService = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<ProductInfo> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Get existing product
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{request.Id}' not found");
        }

        // Update product properties
        product.UpdateDetails(request.Name, request.Description);
        product.UpdatePrice(request.Price);

        // Save changes
        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} updated and cache invalidation via pipeline should occur", request.Id);

        // Defensive: explicitly remove product cache entries to avoid stale reads
        try
        {
            if (_cacheService != null)
            {
                await _cacheService.RemoveAsync("products:all", cancellationToken);
                await _cacheService.RemoveAsync($"products:{product.Id}", cancellationToken);
                _logger.LogInformation("Explicit cache removal executed for product {ProductId}", product.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to explicitly remove cached keys for product {ProductId}. Pipeline invalidation may still occur.", product.Id);
        }

        // Return mapped DTO
        return _mapper.Map<ProductInfo>(product);
    }
}
