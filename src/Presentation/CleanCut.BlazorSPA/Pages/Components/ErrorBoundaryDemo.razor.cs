using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CleanCut.BlazorSPA.Pages.Components;

public partial class ErrorBoundaryDemo : ErrorBoundary
{
    private void Throw()
    {
        throw new InvalidOperationException("This is a test exception for ErrorBoundary demo.");
    }
}
