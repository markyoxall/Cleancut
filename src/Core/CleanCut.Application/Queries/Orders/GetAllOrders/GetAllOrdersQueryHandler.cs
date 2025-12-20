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

        // Batch-fetch customer information to avoid N+1 queries
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();

        if (customerIds.Any())
        {
            var customers = await _customerRepository.GetByIdsAsync(customerIds, cancellationToken);
            var customerDict = customers.ToDictionary(c => c.Id);

            // Populate customer information
            foreach (var orderInfo in orderInfos)
            {
                if (customerDict.TryGetValue(orderInfo.CustomerId, out var customer))
                {
                    orderInfo.CustomerName = customer.GetFullName();
                    orderInfo.CustomerEmail = customer.Email;
                }
            }
        }

        return orderInfos;
    }
}
