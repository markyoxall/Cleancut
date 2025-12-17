using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Components;

public partial class CustomerRow : ComponentBase
{
    [Parameter]
    public SimpleCustomer Customer { get; set; } = null!;

    [Parameter]
    public EventCallback<Guid> EditRequested { get; set; }

    [Parameter]
    public EventCallback<Guid> DeleteRequested { get; set; }

    private bool ShowConfirm { get; set; }

    // Lifecycle examples: show when component initialized / parameters set in console for learning
    protected override void OnInitialized()
    {
        Console.WriteLine($"CustomerRow OnInitialized: {Customer.Id}");
    }

    protected override void OnParametersSet()
    {
        Console.WriteLine($"CustomerRow OnParametersSet: {Customer.Id}");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            Console.WriteLine($"CustomerRow first render: {Customer.Id}");
    }

    private Task OnEditClicked()
    {
        return EditRequested.InvokeAsync(Customer.Id);
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
            await DeleteRequested.InvokeAsync(Customer.Id);
        }
    }
}
