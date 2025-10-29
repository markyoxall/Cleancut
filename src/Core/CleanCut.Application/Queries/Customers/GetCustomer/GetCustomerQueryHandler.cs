using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Queries.Customers.GetCustomer;

/// <summary>
/// Handler for GetCustomerQuery
/// </summary>
public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerInfo?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerInfo?> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
   var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);
        return customer != null ? _mapper.Map<CustomerInfo>(customer) : null;
    }
}