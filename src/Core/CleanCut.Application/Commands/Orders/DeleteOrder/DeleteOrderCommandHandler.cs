using MediatR;
using CleanCut.Domain.Repositories;
using CleanCut.Domain.Exceptions;

namespace CleanCut.Application.Commands.Orders.DeleteOrder;

/// <summary>
/// Handler for DeleteOrderCommand
/// </summary>
public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        // Check if order exists
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
        {
            throw new EntityNotFoundException("Order", request.Id);
        }

        // Business rule: Only allow deletion of pending orders
        if (order.Status != Domain.Entities.OrderStatus.Pending)
        {
            throw new BusinessRuleValidationException("OrderDeletionRule", $"Cannot delete order in {order.Status} status. Only pending orders can be deleted.");
        }

        // Delete order
        var result = await _orderRepository.DeleteAsync(request.Id, cancellationToken);

        if (result)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}