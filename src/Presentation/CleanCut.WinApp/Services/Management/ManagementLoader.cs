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

        string moduleName = typeof(TPresenter).Name;

        UserPreferences? preferences = null;
        try
        {
            // Resolve the preferences service from the newly created scope so any DB-backed
            // implementation (scoped) is resolved within the correct scope and does not become
            // a captive dependency of this singleton loader.
            var preferencesService = provider.GetService<IUserPreferencesService>();
            if (preferencesService != null)
            {
                preferences = await preferencesService.LoadPreferencesAsync(moduleName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load preferences for {Module}", moduleName);
        }

        var view = provider.GetRequiredService<TView>();

        object? presenter;
        var ctorWithPrefs = typeof(TPresenter).GetConstructor(new[] { typeof(TView), typeof(UserPreferences) });
        if (ctorWithPrefs != null)
        {
            presenter = ActivatorUtilities.CreateInstance(provider, typeof(TPresenter), view, preferences);
        }
        else
        {
            presenter = ActivatorUtilities.CreateInstance<TPresenter>(provider, view);
            // If presenter exposes Preferences property, set it
            if (presenter is CleanCut.WinApp.Presenters.CustomerListPresenter clp)
            {
                clp.Preferences = preferences;
            }
        }

        ((CleanCut.WinApp.MVP.BasePresenter<TView>)presenter).Initialize();

        var loadedLogger = provider.GetService(typeof(ILogger<LoadedManagement<TView, TPresenter>>)) as ILogger
                           ?? _logger as ILogger
                           ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        return new LoadedManagement<TView, TPresenter>(
            (TPresenter)presenter, view, scope, loadedLogger);
    }
}
