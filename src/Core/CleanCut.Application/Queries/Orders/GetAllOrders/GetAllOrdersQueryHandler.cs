using MediatR;
using AutoMapper;
using CleanCut.Domain.Repositories;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Orders.GetAllOrders;

/// <summary>
/// Handler for GetAllOrdersQuery
/// </summary>
public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, IReadOnlyList<OrderInfo>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OrderInfo>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        var orderInfos = _mapper.Map<List<OrderInfo>>(orders);

        // Get customer information for each order
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
        var customers = new Dictionary<Guid, Domain.Entities.Customer>();

        foreach (var customerId in customerIds)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer != null)
            {
                customers[customerId] = customer;
            }
        }

        // Populate customer information
        foreach (var orderInfo in orderInfos)
        {
            if (customers.TryGetValue(orderInfo.CustomerId, out var customer))
            {
                orderInfo.CustomerName = customer.GetFullName();
                orderInfo.CustomerEmail = customer.Email;
            }
        }

        return orderInfos;
    }
}