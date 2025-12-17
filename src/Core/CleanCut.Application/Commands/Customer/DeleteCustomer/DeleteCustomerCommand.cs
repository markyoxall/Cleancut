using MediatR;

namespace CleanCut.Application.Commands.Customers.DeleteCustomer;

/// <summary>
/// Command to delete a customer
/// </summary>
public record DeleteCustomerCommand(Guid Id) : IRequest<bool>;