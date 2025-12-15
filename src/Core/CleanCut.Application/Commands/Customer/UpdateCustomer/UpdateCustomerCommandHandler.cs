using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Commands.Customers.UpdateCustomer;

/// <summary>
/// Handler for UpdateUserCommand
/// </summary>
public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerInfo>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerInfo> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Get existing customer
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            throw new InvalidOperationException($"Customer with ID '{request.Id}' not found");
        }

        // Check if email is being changed and if new email already exists
        if (customer.Email != request.Email.ToLowerInvariant())
        {
            var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
            if (existingCustomer != null)
            {
                throw new InvalidOperationException($"Customer with email '{request.Email}' already exists");
            }
        }

        // Update customer properties
        customer.UpdateName(request.FirstName, request.LastName);
        customer.UpdateEmail(request.Email);

        // Save changes
        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return mapped DTO
        return _mapper.Map<CustomerInfo>(customer);
    }
}