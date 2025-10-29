using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Queries.Products.GetProductsByCustomer;

/// <summary>
/// Handler for GetProductsByCustomerQuery
/// </summary>
public class GetProductsByCustomerQueryHandler : IRequestHandler<GetProductsByCustomerQuery, IReadOnlyList<ProductInfo>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsByCustomerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
     _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductInfo>> Handle(GetProductsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ProductInfo>>(products);
  }
}