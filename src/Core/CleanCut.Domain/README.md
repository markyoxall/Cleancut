# CleanCut.Domain - Domain Layer

## Purpose in Clean Architecture

The **Domain Layer** is the **heart** of your application and represents the core business logic. It contains the fundamental business concepts, rules, and behaviors that define what your application does, independent of any external concerns like databases, UI, or frameworks.

## Key Principles

### 1. **Independence**
- **No external dependencies** - This layer should not reference any other projects or external libraries
- **Framework agnostic** - Should work regardless of the technology stack
- **Stable** - Changes here should be rare and driven only by business requirement changes

### 2. **Business-Centric**
- Contains the **business entities**, **value objects**, and **business rules**
- Represents the **ubiquitous language** shared between developers and domain experts
- Models real-world business concepts and their relationships

### 3. **Rich Domain Model**
- Entities contain behavior, not just data (avoid anemic domain model)
- Business logic lives close to the data it operates on
- Encapsulation ensures invariants are maintained

## Folder Structure

```
CleanCut.Domain/
??? Entities/          # Domain entities (aggregates roots)
??? ValueObjects/      # Value objects (immutable objects)
??? Aggregates/        # Aggregate boundaries and roots
??? DomainEvents/      # Domain events for cross-aggregate communication
??? DomainServices/    # Domain services for business logic that doesn't fit in entities
??? Repositories/      # Repository interfaces (contracts)
??? Specifications/    # Business rule specifications
??? Exceptions/        # Domain-specific exceptions
```

## What Goes Here

### Entities
- **Aggregate roots** (main entities that enforce business rules)
- Rich objects with behavior and business logic
- Example: `Customer`, `Order`, `Product`

### Value Objects
- **Immutable objects** defined by their attributes
- No identity, just values
- Example: `Address`, `Money`, `Email`

### Domain Events
- Events that capture business-significant occurrences
- Used for decoupling between aggregates
- Example: `OrderPlaced`, `CustomerRegistered`

### Repository Interfaces
- Contracts for data access (implementations live in Infrastructure)
- Collection-oriented interfaces
- Example: `ICustomerRepository`, `IOrderRepository`

### Domain Services
- Business logic that doesn't naturally fit within an entity
- Operations involving multiple entities
- Example: `PricingService`, `InventoryService`

### Specifications
- Encapsulate business rules and queries
- Reusable business logic
- Example: `VipCustomerSpecification`, `EligibleForDiscountSpecification`

## Example Patterns

### Entity with Business Logic
```csharp
public class Order : AggregateRoot
{
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    
    public void AddItem(Product product, int quantity)
    {
        // Business logic here
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify confirmed order");
            
        // Add domain event
        AddDomainEvent(new OrderItemAdded(Id, product.Id, quantity));
    }
}
```

### Value Object
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        Amount = amount;
        Currency = currency;
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

## Testing Strategy

- **Unit tests** for business logic in entities and domain services
- **Specification tests** for complex business rules
- Focus on **behavior verification**, not implementation details
- Use domain language in test names

## DDD Benefits Applied Here

1. **Ubiquitous Language**: Code reads like business language
2. **Bounded Context**: Clear boundaries around related concepts
3. **Aggregate Patterns**: Consistent business rule enforcement
4. **Domain Events**: Loose coupling between business processes
5. **Rich Domain Model**: Logic close to data, not scattered across layers

## Common Mistakes to Avoid

? **Anemic Domain Model** - Entities with only getters/setters
? **Database Concerns** - No Entity Framework attributes or SQL knowledge
? **Framework Dependencies** - No references to ASP.NET, WinForms, etc.
? **Infrastructure Leaking** - No logging, caching, or external service calls
? **UI Concerns** - No display formatting or presentation logic

? **Rich Behavior** - Methods that enforce business rules
? **Pure C#** - No external dependencies
? **Business Language** - Classes and methods named using domain terminology
? **Immutability** - Value objects and controlled state changes
? **Encapsulation** - Private setters and validation in constructors

This layer is the foundation of your clean architecture - keep it pure, focused, and stable!