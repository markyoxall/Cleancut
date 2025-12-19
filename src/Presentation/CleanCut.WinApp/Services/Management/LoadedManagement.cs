using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Services.Management;

public interface ILoadedManagement : IDisposable
{
    object Presenter { get; }
    CleanCut.WinApp.MVP.IView View { get; }
    Microsoft.Extensions.DependencyInjection.IServiceScope Scope { get; }
    void Cleanup();
}

/// <summary>
/// Generic view over a loaded management handle exposing a strongly-typed presenter.
/// </summary>
/// <typeparam name="TPresenter">The presenter concrete type.</typeparam>
public interface ILoadedManagement<TPresenter> : ILoadedManagement
    where TPresenter : class
{
    /// <summary>
    /// Strongly-typed presenter instance created for this management handle.
    /// </summary>
    TPresenter PresenterTyped { get; }
}

public sealed class LoadedManagement<TView, TPresenter> : ILoadedManagement<TPresenter>, IDisposable
    where TView : class, CleanCut.WinApp.MVP.IView
    where TPresenter : CleanCut.WinApp.MVP.BasePresenter<TView>
{
    private readonly ILogger _logger;

    public LoadedManagement(TPresenter presenter, TView view, Microsoft.Extensions.DependencyInjection.IServiceScope scope, ILogger logger)
    {
        PresenterTyped = presenter ?? throw new ArgumentNullException(nameof(presenter));
        View = view ?? throw new ArgumentNullException(nameof(view));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TPresenter PresenterTyped { get; }
    public TView View { get; }
    public Microsoft.Extensions.DependencyInjection.IServiceScope Scope { get; }

    object ILoadedManagement.Presenter => PresenterTyped!;
    CleanCut.WinApp.MVP.IView ILoadedManagement.View => View!;
    Microsoft.Extensions.DependencyInjection.IServiceScope ILoadedManagement.Scope => Scope!;

    TPresenter ILoadedManagement<TPresenter>.PresenterTyped => PresenterTyped!;

    public void Dispose()
    {
        try
        {
            PresenterTyped?.Cleanup();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during presenter Cleanup");
        }

        try
        {
            (PresenterTyped as IDisposable)?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing presenter");
        }

        try
        {
            Scope?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing service scope");
        }
    }

    public void Cleanup()
    {
        try
        {
            PresenterTyped?.Cleanup();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during presenter Cleanup");
        }
    }
}
