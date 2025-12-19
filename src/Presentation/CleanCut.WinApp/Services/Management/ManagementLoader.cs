using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Services.Management;

public class ManagementLoader : IManagementLoader
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ManagementLoader> _logger;

    public ManagementLoader(IServiceScopeFactory scopeFactory, ILogger<ManagementLoader> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public LoadedManagement<TView, TPresenter> Load<TView, TPresenter>()
        where TView : class, CleanCut.WinApp.MVP.IView
        where TPresenter : CleanCut.WinApp.MVP.BasePresenter<TView>
    {
        _logger.LogInformation("Loading management {Management}", typeof(TPresenter).Name);

        var scope = _scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        var view = provider.GetRequiredService<TView>();

        var presenter = ActivatorUtilities.CreateInstance<TPresenter>(provider, view);

        presenter.Initialize();

        // Do not show the form here — caller (e.g. MainForm) is responsible for setting
        // MdiParent and showing the view so tests can exercise loader without UI side-effects.

        // Resolve a logger for the LoadedManagement if available; fall back to a generic logger.
        var loadedLogger = provider.GetService(typeof(ILogger<LoadedManagement<TView, TPresenter>>)) as ILogger
                           ?? _logger as ILogger
                           ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        return new LoadedManagement<TView, TPresenter>(presenter, view, scope, loadedLogger);
    }

    public async Task<LoadedManagement<TView, TPresenter>> LoadAsync<TView, TPresenter>()
        where TView : class, CleanCut.WinApp.MVP.IView
        where TPresenter : CleanCut.WinApp.MVP.BasePresenter<TView>
    {
        _logger.LogInformation("Loading management (async) {Management}", typeof(TPresenter).Name);

        var scope = _scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;

        // Try to resolve view asynchronously if possible
        var view = provider.GetRequiredService<TView>();

        // If presenter has async initialization, call it here (not shown in current codebase)
        var presenter = ActivatorUtilities.CreateInstance<TPresenter>(provider, view);

        // If presenter has async initialize, await it here (not shown in current codebase)
        presenter.Initialize();

        var loadedLogger = provider.GetService(typeof(ILogger<LoadedManagement<TView, TPresenter>>)) as ILogger
                           ?? _logger as ILogger
                           ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        // Simulate async for now (since DI is sync)
        await Task.Yield();
        return new LoadedManagement<TView, TPresenter>(presenter, view, scope, loadedLogger);
    }
}
