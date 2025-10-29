using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Queries.Customers.GetAllCustomers;

/// <summary>
/// Handler for GetAllCustomersQuery
/// </summary>
public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IReadOnlyList<CustomerInfo>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
  _mapper = mapper;
  }

    public async Task<IReadOnlyList<CustomerInfo>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CustomerInfo>>(customers);
    }
}