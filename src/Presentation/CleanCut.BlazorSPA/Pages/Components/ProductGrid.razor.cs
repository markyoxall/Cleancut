using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Components;

public partial class ProductGrid
{
    [Parameter]
    public IEnumerable<SimpleProduct>? Products { get; set; }
}
