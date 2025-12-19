using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Services.Management;

/// <summary>
/// Loader responsible for creating and initializing management presenters and views.
/// </summary>
public interface IManagementLoader
{
    /// <summary>
    /// Create and initialize a presenter and its view inside a new service scope.
    /// Caller receives a handle containing the presenter, scope and view.
    /// </summary>
    LoadedManagement<TView, TPresenter> Load<TView, TPresenter>()
        where TView : class, IView
        where TPresenter : BasePresenter<TView>;

    /// <summary>
    /// Asynchronously create and initialize a presenter and its view inside a new service scope.
    /// </summary>
    Task<LoadedManagement<TView, TPresenter>> LoadAsync<TView, TPresenter>()
        where TView : class, IView
        where TPresenter : BasePresenter<TView>;
}
