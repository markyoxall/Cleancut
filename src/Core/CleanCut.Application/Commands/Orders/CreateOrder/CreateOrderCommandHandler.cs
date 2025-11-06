using MediatR;
using AutoMapper;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Exceptions;

namespace CleanCut.Application.Commands.Orders.CreateOrder;

/// <summary>
/// Handler for CreateOrderCommand
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderInfo>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderInfo> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Verify customer exists
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new EntityNotFoundException("Customer", request.CustomerId);
        }

        // Verify customer is active
        if (!customer.IsActive)
        {
            throw new BusinessRuleValidationException("Cannot create order for inactive customer");
        }

        // Create order
        var order = new Order(
            request.CustomerId,
            request.ShippingAddress,
            request.BillingAddress,
            request.Notes);

        // Add to repository
        await _orderRepository.AddAsync(order, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var orderInfo = _mapper.Map<OrderInfo>(order);
        orderInfo.CustomerName = customer.GetFullName();
        orderInfo.CustomerEmail = customer.Email;

        return orderInfo;
    }
}