using MediatR;
using AutoMapper;
using CleanCut.Domain.Repositories;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Orders.GetOrdersByCustomer;

/// <summary>
/// Handler for GetOrdersByCustomerQuery
/// </summary>
public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, IReadOnlyList<OrderInfo>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public GetOrdersByCustomerQueryHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OrderInfo>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var orderInfos = _mapper.Map<List<OrderInfo>>(orders);

        // Get customer information
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer != null)
        {
            foreach (var orderInfo in orderInfos)
            {
                orderInfo.CustomerName = customer.GetFullName();
                orderInfo.CustomerEmail = customer.Email;
            }
        }

        return orderInfos;
    }
}