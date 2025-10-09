# CleanCut.Infrastructure.Data - Data Access Layer

## Purpose in Clean Architecture

The **Infrastructure.Data Layer** is responsible for **data persistence and retrieval**. It implements the repository interfaces defined in the Domain Layer and handles all database-related concerns. This layer translates between your rich domain objects and the database's relational model.

## Key Principles

### 1. **Implementation of Domain Contracts**
- Implements repository interfaces from the Domain Layer
- Provides concrete data access implementations
- Maintains the **dependency inversion principle**

### 2. **Data Mapping & Translation**
- Maps between domain entities and database entities
- Handles the **object-relational impedance mismatch**
- Preserves domain object integrity during persistence

### 3. **Database Technology Abstraction**
- Encapsulates all database-specific logic
- Allows switching database providers without affecting other layers
- Centralizes connection management and configuration

### 4. **Performance & Optimization**
- Handles query optimization and database performance
- Manages connections, transactions, and caching
- Implements efficient data access patterns

## Folder Structure

```
CleanCut.Infrastructure.Data/
??? Repositories/         # Repository implementations
?   ??? CustomerRepository.cs
?   ??? OrderRepository.cs
?   ??? ProductRepository.cs
??? Configurations/       # EF Core entity configurations
?   ??? CustomerConfiguration.cs
?   ??? OrderConfiguration.cs
?   ??? ProductConfiguration.cs
??? Migrations/          # Database schema migrations
?   ??? 20241001_InitialCreate.cs
?   ??? 20241002_AddCustomerIndex.cs
??? Context/             # DbContext and database context
?   ??? CleanCutDbContext.cs
?   ??? ICleanCutDbContext.cs
??? Seeds/               # Initial data seeding
?   ??? CustomerSeed.cs
?   ??? ProductSeed.cs
??? Extensions/          # Extension methods for data access
    ??? QueryExtensions.cs
```

## What Goes Here

### Repository Implementations
- Concrete implementations of domain repository interfaces
- Handle CRUD operations and complex queries
- Translate between domain and data models

### Entity Configurations
- EF Core fluent API configurations
- Define database schema, relationships, and constraints
- Map domain entities to database tables

### DbContext
- Central hub for EF Core operations
- Manages entity tracking and change detection
- Handles database connections and transactions

### Migrations
- Version control for database schema
- Handle schema changes over time
- Enable automated deployments

### Data Seeding
- Initial data for development and testing
- Reference data that the application needs
- Demo data for showcasing features

## Example Patterns

### Repository Implementation
```csharp
public class CustomerRepository : ICustomerRepository
{
    private readonly CleanCutDbContext _context;
    
    public CustomerRepository(CleanCutDbContext context)
    {
        _context = context;
    }
    
    public async Task<Customer> GetByIdAsync(CustomerId id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<IEnumerable<Customer>> GetVipCustomersAsync()
    {
        return await _context.Customers
            .Where(c => c.IsVip)
            .ToListAsync();
    }
    
    public void Add(Customer customer)
    {
        _context.Customers.Add(customer);
    }
    
    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }
    
    public void Delete(Customer customer)
    {
        _context.Customers.Remove(customer);
    }
}
```

### Entity Configuration
```csharp
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Table mapping
        builder.ToTable("Customers");
        
        // Primary key
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new CustomerId(value));
        
        // Value object mapping
        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.PostalCode).HasMaxLength(20);
        });
        
        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasConversion(
                email => email.Value,
                value => new Email(value));
        
        // Indexes
        builder.HasIndex(c => c.Email).IsUnique();
        
        // Relationships
        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId);
    }
}
```

### DbContext
```csharp
public class CleanCutDbContext : DbContext, IUnitOfWork
{
    public CleanCutDbContext(DbContextOptions<CleanCutDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Global query filters
        modelBuilder.Entity<Customer>()
            .HasQueryFilter(c => !c.IsDeleted);
        
        base.OnModelCreating(modelBuilder);
    }
    
    public async Task<int> SaveChangesAsync()
    {
        // Handle domain events before saving
        await DispatchDomainEventsAsync();
        
        // Handle audit fields
        HandleAuditFields();
        
        return await base.SaveChangesAsync();
    }
    
    private void HandleAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));
        
        foreach (var entry in entries)
        {
            var entity = (IAuditableEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.ModifiedAt = DateTime.UtcNow;
        }
    }
}
```

## Key Technologies & Packages

### Required NuGet Packages
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

### Project References
- **CleanCut.Domain** (implements domain repository interfaces)
- **CleanCut.Application** (implements application interfaces like IUnitOfWork)

## Database Patterns

### Unit of Work Pattern
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Implemented by DbContext
public class CleanCutDbContext : DbContext, IUnitOfWork
{
    // Implementation
}
```

### Specification Pattern with EF Core
```csharp
public static class QueryExtensions
{
    public static IQueryable<T> Specify<T>(this IQueryable<T> query, ISpecification<T> spec)
        where T : class
    {
        query = query.Where(spec.Criteria);
        
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        
        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);
        
        return query;
    }
}
```

### Value Objects in EF Core
```csharp
// Owned types for value objects
builder.OwnsOne(c => c.Address);

// Value converters for simple value objects
builder.Property(c => c.Email)
    .HasConversion(
        email => email.Value,
        value => new Email(value));
```

## Migration Management

### Creating Migrations
```bash
# Add new migration
dotnet ef migrations add AddCustomerIndex

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script
```

### Seeding Data
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<Product>().HasData(
        new Product { Id = 1, Name = "Sample Product", Price = 29.99m },
        new Product { Id = 2, Name = "Another Product", Price = 49.99m }
    );
}
```

## Performance Considerations

### Query Optimization
- Use `AsNoTracking()` for read-only queries
- Include related data efficiently with `Include()`
- Implement pagination for large datasets
- Use compiled queries for frequently executed queries

### Connection Management
- Use connection pooling
- Implement proper disposal patterns
- Handle connection timeouts and retries

## Testing Strategy

### Unit Tests
- Test repository implementations with in-memory database
- Mock DbContext for pure unit tests
- Test entity configurations separately

### Integration Tests
- Test against real database (SQL Server, PostgreSQL)
- Use test containers for isolated testing
- Verify migrations work correctly

## Common Patterns

### Repository Base Class
```csharp
public abstract class Repository<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly CleanCutDbContext Context;
    
    protected Repository(CleanCutDbContext context)
    {
        Context = context;
    }
    
    public virtual async Task<T> GetByIdAsync(object id)
    {
        return await Context.Set<T>().FindAsync(id);
    }
    
    public virtual void Add(T entity)
    {
        Context.Set<T>().Add(entity);
    }
    
    // Common implementations
}
```

### Query Object Pattern
```csharp
public class CustomerQueries : ICustomerQueries
{
    private readonly CleanCutDbContext _context;
    
    public async Task<CustomerDto> GetCustomerAsync(Guid id)
    {
        return await _context.Customers
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email.Value
            })
            .FirstOrDefaultAsync();
    }
}
```

## Common Mistakes to Avoid

? **Domain Logic in Repositories** - Keep repositories focused on data access
? **Leaking EF Core to Domain** - Don't expose DbContext or IQueryable
? **N+1 Query Problems** - Use Include() or projection to avoid multiple queries
? **Missing Indexes** - Add indexes for frequently queried columns
? **Ignoring Transactions** - Use Unit of Work for consistency

? **Clean Abstractions** - Hide EF Core details behind repository interfaces
? **Proper Mapping** - Configure entities properly with Fluent API
? **Performance Awareness** - Monitor and optimize queries
? **Database Independence** - Use EF Core features that work across providers
? **Migration Management** - Keep migrations clean and reviewable

This layer is your data gateway - keep it efficient, clean, and focused on data access concerns!