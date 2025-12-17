using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Components;

public partial class ProductRow : ComponentBase
{
    [Parameter]
    public SimpleProduct Product { get; set; } = null!;

    [Parameter]
    public EventCallback<Guid> EditRequested { get; set; }

    [Parameter]
    public EventCallback<Guid> DeleteRequested { get; set; }

    private bool ShowConfirm { get; set; }

    protected override void OnInitialized()
    {
        Console.WriteLine($"ProductRow OnInitialized: {Product.Id}");
    }

    protected override void OnParametersSet()
    {
        Console.WriteLine($"ProductRow OnParametersSet: {Product.Id}");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            Console.WriteLine($"ProductRow first render: {Product.Id}");
    }

    private Task OnEditClicked()
    {
        return EditRequested.InvokeAsync(Product.Id);
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
            await DeleteRequested.InvokeAsync(Product.Id);
        }
    }
}
