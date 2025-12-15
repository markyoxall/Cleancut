using MediatR;
using AutoMapper;
using CleanCut.Domain.Repositories;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Exceptions;

namespace CleanCut.Application.Commands.Orders.AddOrderLineItem;

/// <summary>
/// Handler for AddOrderLineItemCommand
/// </summary>
public class AddOrderLineItemCommandHandler : IRequestHandler<AddOrderLineItemCommand, OrderInfo>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddOrderLineItemCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderInfo> Handle(AddOrderLineItemCommand request, CancellationToken cancellationToken)
    {
        // Get existing order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new EntityNotFoundException("Order", request.OrderId);
        }

        // Get product
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new EntityNotFoundException("Product", request.ProductId);
        }

        // Verify product is available
        if (!product.IsAvailable)
        {
            throw new BusinessRuleValidationException("Cannot add unavailable product to order");
        }

        // Add line item to order
        order.AddLineItem(product.Id, product.Name, product.Price, request.Quantity);

        // Update in repository
        await _orderRepository.UpdateAsync(order, cancellationToken);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get customer info for response
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);

        // Map to DTO
        var orderInfo = _mapper.Map<OrderInfo>(order);
        if (customer != null)
        {
            orderInfo.CustomerName = customer.GetFullName();
            orderInfo.CustomerEmail = customer.Email;
        }

        return orderInfo;
    }
}