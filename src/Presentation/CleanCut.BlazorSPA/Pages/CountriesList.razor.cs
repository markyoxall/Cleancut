using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Models;
using CleanCut.BlazorSPA.Pages.State;

namespace CleanCut.BlazorSPA.Pages;

public partial class CountriesList : ComponentBase
{
    protected List<SimpleCountry> Countries { get; private set; } = new();

    [Inject] protected ICountryState CountryState { get; set; } = null!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Countries = await CountryState.GetAllAsync();
    }

    public void EditCountry(Guid id) => NavigationManager.NavigateTo($"/countries/edit/{id}");
    public void CreateCountry() => NavigationManager.NavigateTo("/countries/edit");
    public async Task DeleteCountry(Guid id)
    {
        await CountryState.DeleteAsync(id);
        await LoadAsync();
    }
}
