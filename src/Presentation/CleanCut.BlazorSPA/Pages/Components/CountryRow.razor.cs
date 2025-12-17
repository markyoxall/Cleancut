using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Components;

public partial class CountryRow : ComponentBase
{
    [Parameter]
    public SimpleCountry Country { get; set; } = null!;

    [Parameter]
    public EventCallback<Guid> EditRequested { get; set; }

    [Parameter]
    public EventCallback<Guid> DeleteRequested { get; set; }

    private bool ShowConfirm { get; set; }

    // Lifecycle examples: show when component initialized / parameters set in console for learning
    protected override void OnInitialized()
    {
        Console.WriteLine($"CountryRow OnInitialized: {Country.Id}");
    }

    protected override void OnParametersSet()
    {
        Console.WriteLine($"CountryRow OnParametersSet: {Country.Id}");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            Console.WriteLine($"CountryRow first render: {Country.Id}");
    }

    private Task OnEditClicked()
    {
        return EditRequested.InvokeAsync(Country.Id);
    }

    private void ShowDeleteConfirm()
    {
        ShowConfirm = true;
    }

    private async Task OnConfirmClose(bool confirmed)
    {
        ShowConfirm = false;
        if (confirmed)
        {
            await DeleteRequested.InvokeAsync(Country.Id);
        }
    }
}
