using Microsoft.AspNetCore.Components;

namespace CleanCut.BlazorWebApp.Components.Base;

public abstract class DisposableComponentBase : ComponentBase, IDisposable
{
 private bool _disposed = false;
    protected CancellationTokenSource DisposalCancellationTokenSource { get; } = new();

    protected override void OnInitialized()
    {
    if (_disposed)
      return;

    base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        if (_disposed)
            return;

        await base.OnInitializedAsync();
    }

 protected override void OnParametersSet()
    {
        if (_disposed)
         return;

        base.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_disposed)
      return;

        await base.OnParametersSetAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
      if (_disposed)
            return;

        base.OnAfterRender(firstRender);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposed)
    return;

        await base.OnAfterRenderAsync(firstRender);
    }

    protected void SafeStateHasChanged()
    {
        if (_disposed)
            return;

     try
        {
            InvokeAsync(StateHasChanged);
        }
        catch (ObjectDisposedException)
        {
         // Component already disposed, ignore
   }
        catch (InvalidOperationException)
    {
    // Component not ready for state changes, ignore
  }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
  {
     DisposalCancellationTokenSource?.Cancel();
            DisposalCancellationTokenSource?.Dispose();
            _disposed = true;
 }
    }

    public void Dispose()
    {
Dispose(true);
        GC.SuppressFinalize(this);
    }
}