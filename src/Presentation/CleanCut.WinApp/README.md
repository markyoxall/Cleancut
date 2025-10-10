# CleanCut WinForms Application

## Overview

This is a **Windows Forms desktop application** implementing the **MVP (Model-View-Presenter)** pattern with comprehensive **User and Product Management** features using the same enterprise-grade architecture as the API and Blazor projects.

## Architecture

### MVP Pattern Implementation

```
???????????????????????????????????????
?               Views                 ? ? WinForms UI
?  • UserListForm / UserEditForm     ?
?  • ProductListForm / ProductEditForm?
???????????????????????????????????????
?             Presenters              ? ? Business Logic Coordination
?  • UserListPresenter               ?
?  • UserEditPresenter               ?
?  • ProductListPresenter            ?
?  • ProductEditPresenter            ?
???????????????????????????????????????
?            Application              ? ? CQRS + MediatR + Caching
?  • User Commands & Queries         ?
?  • Product Commands & Queries      ?
?  • Domain Events                   ?
???????????????????????????????????????
?             Domain                  ? ? Pure Business Logic
?  • User & Product Entities         ?
?  • Domain Events                   ?
???????????????????????????????????????
?          Infrastructure             ? ? Data Access + Caching
?  • Entity Framework Core 9         ?
?  • Redis Cache + In-Memory Fallback?
???????????????????????????????????????
```

## Features

### ? **Complete User Management**
- **User List View**: Display all users with filtering capabilities
- **Add Users**: Create new users with validation
- **Edit Users**: Update existing user details
- **Delete Users**: Remove users with confirmation
- **Real-time Validation**: Client-side form validation
- **User Search & Filter**: Find users quickly

### ? **Complete Product Management**
- **Product List View**: Display products with owner information
- **Filter by User**: View products for specific users
- **Add Products**: Create new products with user assignment
- **Edit Products**: Update product details, price, availability
- **Delete Products**: Remove products with confirmation
- **Price Management**: Decimal price input with validation
- **Availability Toggle**: Mark products as available/unavailable

### ? **Enterprise Patterns**
- **MVP Pattern**: Clean separation of concerns
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **CQRS**: Commands and Queries via MediatR
- **Domain Events**: Automatic event publishing
- **Caching**: Redis with automatic fallback to in-memory
- **Validation**: FluentValidation integration
- **Logging**: Structured logging with Serilog

## User Interface Features

### ?? **User Management Interface**
```
?? User Management ?????????????????????????????????????
? [Add User] [Edit User] [Delete User]    [Refresh]   ?
????????????????????????????????????????????????????????
? First Name ? Last Name ? Email      ? Status ? Created?
? John       ? Doe       ? john@...   ? Active ? 2024-..?
? Jane       ? Smith     ? jane@...   ? Active ? 2024-..?
????????????????????????????????????????????????????????
```

### ??? **Product Management Interface**
```
?? Product Management ??????????????????????????????????
? Filter by User: [All Users     ?] [Filter]          ?
? [Add Product] [Edit Product] [Delete Product] [Refresh]?
????????????????????????????????????????????????????????
? Name    ? Description ? Price ? Status ? Owner ? Created?
? Widget  ? A useful... ? $29.99? Avail. ? John  ? 2024-..?
? Gadget  ? Advanced... ? $49.99? Unavail? Jane  ? 2024-..?
????????????????????????????????????????????????????????
```

### ?? **Form Dialogs**
- **User Edit Dialog**: First Name, Last Name, Email, Active Status
- **Product Edit Dialog**: Name, Description, Price, User Assignment, Availability

## Technical Implementation

### ??? **MVP Pattern Benefits**

#### **Separation of Concerns**
```csharp
// View: Pure UI Logic
public partial class ProductListForm : BaseForm, IProductListView
{
    public event EventHandler? AddProductRequested;
    public void DisplayProducts(IEnumerable<ProductDto> products) { ... }
}

// Presenter: Business Logic Coordination
public class ProductListPresenter : BasePresenter<IProductListView>
{
    private async void OnAddProductRequested(object? sender, EventArgs e)
    {
        await ExecuteAsync(async () => {
            // Coordinate with Application layer
            var users = await _mediator.Send(new GetAllUsersQuery());
            // Handle business logic
        });
    }
}
```

#### **Testability**
- Views implement interfaces ? Easy mocking
- Presenters contain testable business logic
- No UI dependencies in business logic

### ?? **Async Operations with Error Handling**
```csharp
protected async Task ExecuteAsync(Func<Task> operation)
{
    try
    {
        View.SetLoading(true);
        await operation();
    }
    catch (Exception ex)
    {
        HandleError(ex);
    }
    finally
    {
        View.SetLoading(false);
    }
}
```

### ?? **Data Integration**
- **Same Commands/Queries** as API project
- **Consistent DTOs** across all presentation layers
- **Automatic caching** with cache invalidation
- **Domain events** for business logic decoupling

### ?? **Validation & Error Handling**
```csharp
public Dictionary<string, string> ValidateForm()
{
    var errors = new Dictionary<string, string>();
    
    if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
        errors.Add("Name", "Product name is required");
    
    if (_priceNumericUpDown.Value < 0)
        errors.Add("Price", "Price cannot be negative");
    
    return errors;
}
```

## Running the Application

### Prerequisites
- **.NET 10 SDK** (preview)
- **SQL Server LocalDB** (for database)
- **Redis** (optional - will fallback to in-memory cache)

### Setup Database
The application will automatically:
1. **Create database** if it doesn't exist
2. **Run migrations** to set up schema
3. **Seed test data** with sample users and products

### Setup Redis (Optional)
```bash
# Using Docker
docker run -d -p 6379:6379 redis:latest

# Or install Redis locally
# Windows: Download from https://redis.io/download
# The app will automatically fallback to in-memory cache if Redis is unavailable
```

### Run Application
```bash
dotnet run --project src/Presentation/CleanCut.WinApp/CleanCut.WinApp.csproj
```

## Configuration

### Database Connections
- **Development**: `CleanCutDb_Dev` (LocalDB) - **Shared with API and other applications**
- **Production**: `CleanCutDb` (LocalDB) - **Shared with API and other applications**

### Logging Configuration
- **Console**: Real-time logging during development
- **File**: Persistent logs in `logs/cleancut-winapp-*.txt`
- **Structured**: JSON-formatted logs with correlation IDs

## Enterprise Features

### ?? **Domain Events Integration**
When entities are modified, domain events are automatically published:
```csharp
// User entity raises events
user.UpdateName("New", "Name"); // ? UserUpdatedEvent published

// Product entity raises events  
product.UpdatePrice(99.99m); // ? ProductUpdatedEvent published
```

### ?? **Caching Integration**
- **Redis caching** for improved performance
- **Automatic cache invalidation** when data changes
- **Fallback to in-memory** caching when Redis unavailable
- **Cache key management** with consistent naming

### ?? **Performance Optimizations**
- **Async/await** throughout the application
- **Loading indicators** for long-running operations
- **Efficient data binding** with minimal UI updates
- **Memory management** with proper presenter cleanup

## Extending the Application

### Adding New Entity Management
1. **Create View Interface** inheriting from `IView`
2. **Implement Form** inheriting from `BaseForm`
3. **Create Presenter** inheriting from `BasePresenter<T>`
4. **Add to ServiceConfiguration** for dependency injection
5. **Update MainForm** navigation menu

### Example: Adding Category Management
```csharp
// 1. Interface
public interface ICategoryListView : IView { ... }

// 2. Form
public class CategoryListForm : BaseForm, ICategoryListView { ... }

// 3. Presenter  
public class CategoryListPresenter : BasePresenter<ICategoryListView> { ... }

// 4. Register services
services.AddScoped<ICategoryListView, CategoryListForm>();
services.AddScoped<CategoryListPresenter>();

// 5. Add menu item
categoryMenuItem.Click += OnCategoryManagementClicked;
```

## Architecture Benefits

### ??? **Maintainability**
- **Clear separation** between UI and business logic
- **Consistent patterns** across all features
- **Easy to extend** with new functionality

### ?? **Testability**
- **Mock views** for presenter unit tests
- **Test business logic** without UI dependencies
- **Isolated component testing**

### ?? **Consistency**
- **Same business rules** as web API
- **Consistent data models** across platforms
- **Unified caching strategy**

### ?? **Performance**
- **Efficient async operations**
- **Smart caching** with automatic invalidation
- **Minimal UI updates** with proper data binding

This WinForms application demonstrates how desktop applications can leverage the same enterprise patterns as modern web applications, providing a rich user experience while maintaining clean architecture principles and comprehensive business functionality! ??