using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Components;

public partial class ProductEditForm
{
    [Parameter]
    public SimpleProduct Product { get; set; } = new();

    [Parameter]
    public EventCallback<SimpleProduct> OnValidSubmit { get; set; }
}
