using AutoMapper;
using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Commands.Customers.CreateCustomer;

/// <summary>
/// Handler for CreateUserCommand (now creates a Customer)
/// </summary>
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerInfo>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerInfo> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existing = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Customer with email '{request.Email}' already exists");
        }

        // Create new customer using factory method with value object validation
        var customerResult = Customer.Create(request.FirstName, request.LastName, request.Email);
        if (!customerResult.IsSuccess)
        {
            throw new InvalidOperationException(customerResult.Error);
        }

        var customer = customerResult.Value!;

        // Add to repository
        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return mapped DTO
        return _mapper.Map<CustomerInfo>(customer);
    }
}
