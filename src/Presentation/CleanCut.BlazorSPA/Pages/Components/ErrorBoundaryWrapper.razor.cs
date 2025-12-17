using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CleanCut.BlazorSPA.Pages.Services;

namespace CleanCut.BlazorSPA.Pages.Components;

// Simplified wrapper: plain component to keep app simple.
public partial class ErrorBoundaryWrapper : ErrorBoundary
{
    protected override Task OnErrorAsync(Exception exception)
    {
        Console.Error.WriteLine($"ErrorBoundary captured: {exception.Message}");
        return Task.CompletedTask;
    }

}
