using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Models;
using CleanCut.BlazorSPA.Pages.State;

namespace CleanCut.BlazorSPA.Pages;

public partial class CustomersList : ComponentBase
{
    protected List<SimpleCustomer> Customers { get; private set; } = new();

    [Inject]
    protected ICustomerState CustomerState { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Customers = await CustomerState.GetAllAsync();
        StateHasChanged();
    }

    public async Task AddCustomerInline()
    {
        var next = new SimpleCustomer { Id = System.Guid.NewGuid(), FirstName = "", LastName = "", Email = "", IsActive = true };
        await CustomerState.CreateAsync(next);
        await LoadAsync();
    }

    public async Task DeleteCustomer(Guid id)
    {
        await CustomerState.DeleteAsync(id);
        await LoadAsync();
    }

    public void EditCustomer(Guid id)
    {
        NavigationManager.NavigateTo($"/customers/edit/{id}");
    }

    public void CreateCustomer()
    {
        NavigationManager.NavigateTo("/customers/edit");
    }

    private bool showErrorDemo;

    private void ShowErrorDemo()
    {
        showErrorDemo = true;
    }

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;
}


