using MediatR;
using AutoMapper;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Exceptions;
using CleanCut.Application.Common.Interfaces;

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
    private readonly IProductRepository _productRepository;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _productRepository = productRepository;
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

        // Create order and add line items (if any) using authoritative prices from products
        var order = new Order(
            request.CustomerId,
            request.ShippingAddress,
            request.BillingAddress,
            request.Notes);

        // Add requested items using server-side product prices
        if (request.Items != null)
        {
            foreach (var it in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(it.ProductId, cancellationToken);
                if (product == null)
                    throw new EntityNotFoundException("Product", it.ProductId);

                order.AddLineItem(product.Id, product.Name, product.Price, it.Quantity);
            }
        }

        // Add to repository and persist in a single unit of work
        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO with complete information
        var orderInfo = _mapper.Map<OrderInfo>(order);
        orderInfo.CustomerName = customer.GetFullName();
        orderInfo.CustomerEmail = customer.Email;

        // Domain events will handle RabbitMQ publishing
        // No direct publishing here - separation of concerns
        return orderInfo;
    }
}
