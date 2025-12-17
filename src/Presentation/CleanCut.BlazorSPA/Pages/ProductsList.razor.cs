using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Models;
using CleanCut.BlazorSPA.Pages.State;

namespace CleanCut.BlazorSPA.Pages;

public partial class ProductsList : ComponentBase
{
    protected List<SimpleProduct> Products { get; private set; } = new();

    [Inject] protected IProductState ProductState { get; set; } = null!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Products = await ProductState.GetAllAsync();
    }

    public void EditProduct(Guid id) => NavigationManager.NavigateTo($"/products/edit/{id}");
    public void CreateProduct() => NavigationManager.NavigateTo("/products/edit");
    public async Task DeleteProduct(Guid id)
    {
        await ProductState.DeleteAsync(id);
        await LoadAsync();
    }
}
