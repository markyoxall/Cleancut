using MediatR;
using AutoMapper;
using CleanCut.Domain.Repositories;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Orders.GetOrder;

/// <summary>
/// Handler for GetOrderQuery
/// </summary>
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderInfo?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public GetOrderQueryHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<OrderInfo?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
            return null;

        var orderInfo = _mapper.Map<OrderInfo>(order);

        // Get customer information
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customer != null)
        {
            orderInfo.CustomerName = customer.GetFullName();
            orderInfo.CustomerEmail = customer.Email;
        }

        return orderInfo;
    }
}