using CleanCut.Application.Events;
using MediatR;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using CleanCut.Domain.Repositories;
using AutoMapper;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.API.EventHandlers;

/// <summary>
/// Handles OrderCreated domain event and publishes to RabbitMQ for email processing
/// </summary>
public class OrderCreatedNotificationHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly IIntegrationEventPublisher _publisher;
    private readonly ILogger<OrderCreatedNotificationHandler> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public OrderCreatedNotificationHandler(
        IIntegrationEventPublisher publisher,
        ILogger<OrderCreatedNotificationHandler> logger,
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IMapper mapper)
    {
        _publisher = publisher;
        _logger = logger;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;

        // Fetch complete order with line items
        var fullOrder = await _orderRepository.GetByIdAsync(order.Id, cancellationToken);
        if (fullOrder == null)
        {
            _logger.LogWarning("Order {OrderId} not found", order.Id);
            return;
        }

        // Fetch customer for email
        var customer = await _customerRepository.GetByIdAsync(fullOrder.CustomerId, cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", fullOrder.CustomerId);
            return;
        }

        // Build complete OrderInfo
        var orderInfo = _mapper.Map<OrderInfo>(fullOrder);
        orderInfo.CustomerName = customer.GetFullName();
        orderInfo.CustomerEmail = customer.Email;

        // Publish to RabbitMQ
        await _publisher.PublishAsync("order.created", orderInfo, cancellationToken);
        _logger.LogInformation("✅ Published order.created for {OrderId}", orderInfo.Id);
    }
}
