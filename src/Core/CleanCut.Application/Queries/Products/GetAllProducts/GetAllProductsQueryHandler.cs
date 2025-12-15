using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.Application.Queries.Customers.GetAllCustomers;
using CleanCut.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCut.Application.Queries.Products.GetAllProducts;



/// <summary>
/// Handler for GetAllUsersQuery
/// </summary>
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductInfo>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductInfo>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ProductInfo>>(products);
    }

}