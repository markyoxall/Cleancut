namespace CleanCut.BlazorSPA.Pages.Services;

public sealed class DebugSettings
{
    /// <summary>
    /// When true the ErrorBoundary is active.
    /// Set to false to let exceptions bubble (useful when debugging to see full stack traces).
    /// </summary>
    public bool UseErrorBoundary { get; set; } = true;
}
