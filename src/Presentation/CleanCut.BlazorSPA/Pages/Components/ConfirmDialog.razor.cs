using Microsoft.AspNetCore.Components;
using CleanCut.BlazorSPA.Pages.Services;
using System.Threading.Tasks;

namespace CleanCut.BlazorSPA.Pages.Components;

public partial class ConfirmDialog
{
    [Inject]
    private IJsInteropService Js { get; set; } = null!;

    [Parameter]
    public string? Message { get; set; }

    public async Task<bool> ConfirmAsync(string message)
    {
        return await Js.ConfirmAsync(message);
    }
}
