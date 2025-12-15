using MediatR;
using AutoMapper;
using CleanCut.Domain.Repositories;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Exceptions;

namespace CleanCut.Application.Commands.Orders.UpdateOrder;

/// <summary>
/// Handler for UpdateOrderCommand
/// </summary>
public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderInfo>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateOrderCommandHandler(
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

    public async Task<OrderInfo> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        // Get existing order
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
        {
            throw new EntityNotFoundException("Order", request.Id);
        }

        // Update order properties
        order.UpdateShippingAddress(request.ShippingAddress);
        order.UpdateBillingAddress(request.BillingAddress);
        order.UpdateNotes(request.Notes);

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