using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Queries.Products.GetProduct;

/// <summary>
/// Handler for GetProductQuery
/// </summary>
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductInfo?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductInfo?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        return product != null ? _mapper.Map<ProductInfo>(product) : null;
    }
}