using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.State;

/// <summary>
/// Typed actions for state management
/// </summary>
public abstract record StateAction;

// User Actions
public record LoadUsersAction : StateAction;
public record UsersLoadedAction(List<UserDto> Users) : StateAction;
public record UserCreatedAction(UserDto User) : StateAction;
public record UserUpdatedAction(UserDto User) : StateAction;
public record UserDeletedAction(Guid UserId) : StateAction;
public record SelectUserAction(UserDto? User) : StateAction;

// Product Actions
public record LoadProductsAction(Guid? UserId = null) : StateAction;
public record ProductsLoadedAction(List<ProductDto> Products) : StateAction;
public record ProductCreatedAction(ProductDto Product) : StateAction;
public record ProductUpdatedAction(ProductDto Product) : StateAction;
public record ProductDeletedAction(Guid ProductId) : StateAction;
public record SelectProductAction(ProductDto? Product) : StateAction;

// UI Actions
public record SetLoadingAction(bool IsLoading) : StateAction;
public record SetMessageAction(string Message, bool IsSuccess = true) : StateAction;
public record ClearMessageAction : StateAction;

/// <summary>
/// Application state container
/// </summary>
public record AppState
{
    public List<UserDto> Users { get; init; } = new();
    public List<ProductDto> Products { get; init; } = new();
    public UserDto? SelectedUser { get; init; }
    public ProductDto? SelectedProduct { get; init; }
    public bool IsLoading { get; init; }
    public string? Message { get; init; }
    public bool IsSuccess { get; init; }
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;

    // Computed properties
    public int UserCount => Users.Count;
    public int ProductCount => Products.Count;
    public decimal TotalProductValue => Products.Sum(p => p.Price);
    public int ActiveUserCount => Users.Count(u => u.IsActive);
    public int AvailableProductCount => Products.Count(p => p.IsAvailable);
}

/// <summary>
/// State container with reducer pattern
/// </summary>
public interface IStateContainer
{
    AppState State { get; }
    event Action<AppState>? StateChanged;
    
    void Dispatch(StateAction action);
    Task DispatchAsync(StateAction action);
}

public class StateContainer : IStateContainer
{
    private AppState _state = new();
    private readonly ILogger<StateContainer> _logger;

    public StateContainer(ILogger<StateContainer> logger)
    {
        _logger = logger;
    }

    public AppState State => _state;
    public event Action<AppState>? StateChanged;

    public void Dispatch(StateAction action)
    {
        _logger.LogDebug("Dispatching action: {ActionType}", action.GetType().Name);
        
        var newState = Reduce(_state, action);
        if (!ReferenceEquals(newState, _state))
        {
            _state = newState with { LastUpdated = DateTime.UtcNow };
            StateChanged?.Invoke(_state);
        }
    }

    public async Task DispatchAsync(StateAction action)
    {
        await Task.Run(() => Dispatch(action));
    }

    private static AppState Reduce(AppState state, StateAction action)
    {
        return action switch
        {
            // User actions
            LoadUsersAction => state with { IsLoading = true },
            UsersLoadedAction a => state with { Users = a.Users, IsLoading = false },
            UserCreatedAction a => state with 
            { 
                Users = state.Users.Concat(new[] { a.User }).ToList(),
                SelectedUser = a.User,
                Message = $"User '{a.User.FirstName} {a.User.LastName}' created successfully",
                IsSuccess = true
            },
            UserUpdatedAction a => state with 
            { 
                Users = state.Users.Select(u => u.Id == a.User.Id ? a.User : u).ToList(),
                SelectedUser = a.User,
                Message = $"User '{a.User.FirstName} {a.User.LastName}' updated successfully",
                IsSuccess = true
            },
            UserDeletedAction a => state with 
            { 
                Users = state.Users.Where(u => u.Id != a.UserId).ToList(),
                SelectedUser = state.SelectedUser?.Id == a.UserId ? null : state.SelectedUser,
                Message = "User deleted successfully",
                IsSuccess = true
            },
            SelectUserAction a => state with { SelectedUser = a.User },

            // Product actions
            LoadProductsAction => state with { IsLoading = true },
            ProductsLoadedAction a => state with { Products = a.Products, IsLoading = false },
            ProductCreatedAction a => state with 
            { 
                Products = state.Products.Concat(new[] { a.Product }).ToList(),
                SelectedProduct = a.Product,
                Message = $"Product '{a.Product.Name}' created successfully",
                IsSuccess = true
            },
            ProductUpdatedAction a => state with 
            { 
                Products = state.Products.Select(p => p.Id == a.Product.Id ? a.Product : p).ToList(),
                SelectedProduct = a.Product,
                Message = $"Product '{a.Product.Name}' updated successfully",
                IsSuccess = true
            },
            ProductDeletedAction a => state with 
            { 
                Products = state.Products.Where(p => p.Id != a.ProductId).ToList(),
                SelectedProduct = state.SelectedProduct?.Id == a.ProductId ? null : state.SelectedProduct,
                Message = "Product deleted successfully",
                IsSuccess = true
            },
            SelectProductAction a => state with { SelectedProduct = a.Product },

            // UI actions
            SetLoadingAction a => state with { IsLoading = a.IsLoading },
            SetMessageAction a => state with { Message = a.Message, IsSuccess = a.IsSuccess },
            ClearMessageAction => state with { Message = null },

            _ => state
        };
    }
}