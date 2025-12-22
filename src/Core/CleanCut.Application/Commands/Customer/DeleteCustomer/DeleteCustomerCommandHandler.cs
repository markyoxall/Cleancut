using MediatR;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Commands.Customers.DeleteCustomer;

/// <summary>
/// Handler for DeleteCustomerCommand
/// </summary>
public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, bool>
{
 private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
 {
        // Get existing customer
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
{
            return false; // Customer not found
    }

        // Check if customer has any associated products
 var customerProducts = await _unitOfWork.Products.GetByCustomerIdAsync(request.Id, false, cancellationToken);
   if (customerProducts.Any())
   {
            throw new InvalidOperationException($"Cannot delete customer '{customer.FirstName} {customer.LastName}' because they have {customerProducts.Count()} associated products. Please delete or reassign the products first.");
        }

        // Delete customer
 await _unitOfWork.Customers.DeleteAsync(customer, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
