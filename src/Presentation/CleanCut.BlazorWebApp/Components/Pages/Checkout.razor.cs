using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;

namespace CleanCut.BlazorWebApp.Components.Pages;

public partial class Checkout : ComponentBase
{
    [Inject] private IShoppingCartService CartService { get; set; } = null!;
    [Inject] private IOrdersApiService OrdersApi { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<Checkout> Logger { get; set; } = null!;

    protected IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();
    protected decimal Total { get; set; } = 0;

    protected CheckoutModel Model { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Items = await CartService.GetItemsAsync();
        Total = await CartService.GetTotalAsync();
    }

    protected async Task PlaceOrder()
    {
        // For demo, assume first customer ID is used
        var items = Items.Select(i => new CleanCut.BlazorWebApp.Services.CreateOrderItemRequest(i.ProductId, i.Quantity)).ToList();
        var request = new CleanCut.BlazorWebApp.Services.CreateOrderRequest(Guid.Parse("11111111-1111-1111-1111-111111111111"), Model.ShippingAddress, Model.BillingAddress, items);
        var order = await OrdersApi.CreateOrderAsync(request);

        await CartService.ClearAsync();
        Navigation.NavigateTo($"/orders/details/{order.Id}");
    }

    protected void BackToBasket() => Navigation.NavigateTo("/cart");
    protected void Cancel() => Navigation.NavigateTo("/cart");

    public class CheckoutModel
    {
        public string ShippingAddress { get; set; } = "Fred";
        public string BillingAddress { get; set; } = "Quimby";
    }
}
