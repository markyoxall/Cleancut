using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Threading;

namespace CleanCut.BlazorSPA.Pages.Services;

public class JsInteropService : IJsInteropService
{
    private readonly IJSRuntime _js;

    public JsInteropService(IJSRuntime js)
    {
        _js = js;
    }

    public ValueTask<bool> ConfirmAsync(string message)
    {
        return _js.InvokeAsync<bool>("confirm", message);
    }

    public ValueTask AlertAsync(string message)
    {
        return _js.InvokeVoidAsync("alert", message);
    }
}
