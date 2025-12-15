using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.Commands.Products.CreateProduct;

/// <summary>
/// Handler for CreateProductCommand
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductInfo>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService? _cacheService;
    private readonly ILogger<CreateProductCommandHandler>? _logger;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService? cacheService = null, ILogger<CreateProductCommandHandler>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ProductInfo> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Verify customer exists
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID '{request.CustomerId}' not found");
        }

        // Create new product
        var product = new Product(request.Name, request.Description, request.Price, request.CustomerId);

        // Add to repository
        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Defensive: invalidate product list cache to show new item
        try
        {
            if (_cacheService != null)
            {
                await _cacheService.RemoveAsync("products:all", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to remove products:all cache after create");
        }

        // Return mapped DTO
        return _mapper.Map<ProductInfo>(product);
    }
}
