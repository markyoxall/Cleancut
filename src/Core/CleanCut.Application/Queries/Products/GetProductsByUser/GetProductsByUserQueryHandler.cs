using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Queries.Products.GetProductsByUser;

/// <summary>
/// Handler for GetProductsByUserQuery
/// </summary>
public class GetProductsByUserQueryHandler : IRequestHandler<GetProductsByUserQuery, IReadOnlyList<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsByUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsByUserQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetByUserIdAsync(request.UserId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ProductDto>>(products);
    }
}