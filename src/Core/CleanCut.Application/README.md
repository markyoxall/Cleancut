# CleanCut.Application - Application Layer

## Purpose in Clean Architecture

The **Application Layer** orchestrates the execution of business use cases. It coordinates between the Domain Layer and the outside world, handling the application's business workflows without containing business logic itself. This layer defines **what** the application does, while the Domain Layer defines **how** the business works.

## Key Principles

### 1. **Use Case Driven**
- Each use case is implemented as a separate handler
- Follows the **Single Responsibility Principle**
- Maps directly to user stories and business requirements

### 2. **Technology Independent**
- No knowledge of databases, web frameworks, or UI concerns
- Only depends on the Domain Layer and defines abstractions for infrastructure
- Can be unit tested without external dependencies

### 3. **Coordination, Not Business Logic**
- Orchestrates calls to domain entities and services
- Handles cross-cutting concerns like validation, authorization, and transactions
- Delegates business decisions to the Domain Layer

### 4. **CQRS Pattern**
- **Commands** for write operations (state changes)
- **Queries** for read operations (data retrieval)
- Clear separation of concerns

## Folder Structure

```
CleanCut.Application/
??? Commands/          # Write operations (Create, Update, Delete)
?   ??? CreateOrder/
?   ??? UpdateCustomer/
?   ??? DeleteProduct/
??? Queries/           # Read operations (Get, List, Search)
?   ??? GetCustomer/
?   ??? ListOrders/
?   ??? SearchProducts/
??? Handlers/          # Command and Query handlers (business workflows)
??? DTOs/              # Data Transfer Objects for external communication
??? Interfaces/        # Abstractions for infrastructure services
??? Services/          # Application services (cross-cutting concerns)
??? Behaviors/         # Cross-cutting behaviors (logging, validation, caching)
??? Validators/        # Input validation rules
??? Mappings/          # Object mapping configurations
```

## What Goes Here

### Commands & Queries (CQRS)
- **Commands**: Represent write operations with business intent
- **Queries**: Represent read operations with specific data needs
- **Handlers**: Execute the use case logic

### DTOs (Data Transfer Objects)
- Objects for transferring data across boundaries
- No business logic, just data containers
- Separate from domain entities to avoid coupling

### Application Services
- Cross-cutting concerns like email, notifications, file handling
- Interfaces defined here, implementations in Infrastructure
- Example: `IEmailService`, `IFileStorageService`

### Behaviors
- Cross-cutting pipeline behaviors using MediatR
- Logging, validation, caching, transaction management
- Applied to all commands/queries automatically

### Validators
- Input validation using FluentValidation
- Separate from business validation (which lives in Domain)
- Ensures commands/queries have valid structure

## Example Patterns

### Command Pattern
```csharp
// Command
public class CreateOrderCommand : IRequest<OrderDto>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch domain entities
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        
        // 2. Create domain entity (business logic happens here)
        var order = Order.Create(customer, request.Items.Select(i => /* map to domain */));
        
        // 3. Persist changes
        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync();
        
        // 4. Return DTO
        return _mapper.Map<OrderDto>(order);
    }
}
```

### Query Pattern
```csharp
// Query
public class GetCustomerQuery : IRequest<CustomerDto>
{
    public Guid CustomerId { get; set; }
}

// Handler
public class GetCustomerHandler : IRequestHandler<GetCustomerQuery, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    
    public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        return _mapper.Map<CustomerDto>(customer);
    }
}
```

### Cross-Cutting Behavior
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();
            
        if (failures.Any())
            throw new ValidationException(failures);
            
        return await next();
    }
}
```

## Key Dependencies

### Required NuGet Packages
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation" Version="11.8.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
```

### Project References
- **CleanCut.Domain** (only dependency - the core business logic)

## Dependency Injection Setup

The Application Layer should register:
- MediatR handlers
- Validators
- Behaviors
- AutoMapper profiles

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddAutoMapper(Assembly.GetExecutingAssembly());
```

## Testing Strategy

### Unit Tests
- Test handlers in isolation using mocked repositories
- Focus on the coordination logic, not business rules (those are tested in Domain)
- Verify correct calls to domain entities and infrastructure services

### Integration Tests
- Test the entire use case flow with real dependencies
- Verify that commands/queries work end-to-end
- Use in-memory database for fast execution

## Benefits of This Approach

1. **Clear Use Cases**: Each handler represents a specific business scenario
2. **Testability**: Easy to unit test without external dependencies
3. **Flexibility**: Can change infrastructure without affecting use cases
4. **Performance**: CQRS allows optimization of reads vs writes
5. **Maintainability**: Changes to one use case don't affect others

## Common Patterns

### Command/Query Separation
- **Commands**: Return success/failure status, maybe an ID
- **Queries**: Return DTOs with the exact data needed by the client
- Never mix read and write operations in the same handler

### Error Handling
```csharp
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Implementation
            return Result<OrderDto>.Success(orderDto);
        }
        catch (DomainException ex)
        {
            return Result<OrderDto>.Failure(ex.Message);
        }
    }
}
```

## Common Mistakes to Avoid

? **Business Logic in Application Layer** - Keep it in Domain
? **Direct Database Access** - Use repository abstractions
? **Framework Coupling** - No references to ASP.NET, Entity Framework, etc.
? **Fat Handlers** - Keep handlers focused on coordination
? **Mixing Commands and Queries** - Separate read from write operations

? **Pure Coordination** - Orchestrate domain objects and infrastructure services
? **Interface Abstractions** - Define contracts for infrastructure services
? **Single Responsibility** - One handler per use case
? **Clear Intent** - Command/query names express business operations
? **Cross-Cutting Concerns** - Use behaviors for logging, validation, etc.

This layer is your application's control center - keep it focused on orchestrating business workflows!