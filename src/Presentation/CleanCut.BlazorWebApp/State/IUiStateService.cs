using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.State;

public interface IUiStateService
{
    bool IsLoading { get; }
    string? CurrentMessage { get; }
    bool IsSuccess { get; }

    event Action? StateChanged;
    event Action<string, bool>? MessageChanged;

    void SetLoading(bool isLoading);
    void SetMessage(string message, bool isSuccess = true);
    void ClearMessage();

    // Optional selection surface
    CustomerInfo? SelectedUser { get; }
    ProductInfo? SelectedProduct { get; }
    void SetSelectedUser(CustomerInfo? user);
    void SetSelectedProduct(ProductInfo? product);
}
