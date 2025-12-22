using CleanCut.Application.DTOs;
using CleanCut.Application.Queries.Countries.GetAllCountries;
using CleanCut.Application.Queries.Countries.GetCountry;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Countries;
using CleanCut.WinApp.Services.Management;

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CleanCut.Infrastructure.Caching.Constants;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for the Country List view. Responsible for loading country data and coordinating
/// UI interactions. Also manages persisting and restoring the DevExpress grid layout via
/// the <see cref="ILayoutPersistenceService"/> so persistence is testable and implementation-agnostic.
/// </summary>
public class CountryListPresenter : BasePresenter<ICountryListView>
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly Services.Factories.IViewFactory<ICountryEditView> _countryEditViewFactory;
    private readonly ILogger<CountryListPresenter> _logger;
    private readonly CleanCut.Application.Common.Interfaces.ICacheService _cacheService;
    private readonly ILayoutPersistenceService _layoutPersistenceService;

    private List<CountryInfo> _cachedCountries = new();

    // layout loaded from persistence to be applied after data binding
    private string? _loadedLayoutJson;

    public CountryListPresenter(
        ICountryListView view,
        IMediator mediator,
        IServiceProvider serviceProvider,
        Services.Factories.IViewFactory<ICountryEditView> countryEditViewFactory,
        ILogger<CountryListPresenter> logger,
        CleanCut.Application.Common.Interfaces.ICacheService cacheService,
        ILayoutPersistenceService layoutPersistenceService)
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _countryEditViewFactory = countryEditViewFactory ?? throw new ArgumentNullException(nameof(countryEditViewFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _layoutPersistenceService = layoutPersistenceService ?? throw new ArgumentNullException(nameof(layoutPersistenceService));
    }

    /// <summary>
    /// Initialize the presenter, subscribe to view events and load layout for this module.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        // Subscribe to view events
        View.AddCountryRequested += OnAddCountryRequestedHandler;
        View.EditCountryRequested += OnEditCountryRequestedHandler;
        View.DeleteCountryRequested += OnDeleteCountryRequestedHandler;
        View.RefreshRequested += OnRefreshRequestedHandler;

        // Subscribe to layout events
        View.SaveLayoutRequested += OnSaveLayoutRequestedHandler;
        View.LoadLayoutRequested += OnLoadLayoutRequestedHandler;

        // Load layout for this presenter/module
        _ = LoadLayoutAsync();

        // Load initial data
        _ = LoadCountriesAsync();
    }

    private void OnSaveLayoutRequestedHandler(object? sender, EventArgs e) => _ = OnSaveLayoutRequestedAsync();
    private void OnLoadLayoutRequestedHandler(object? sender, EventArgs e) => _ = OnLoadLayoutRequestedAsync();

    private async Task LoadLayoutAsync()
    {
        const string moduleName = nameof(CountryListPresenter);
        try
        {
            _loadedLayoutJson = await _layoutPersistenceService.LoadLayoutAsync(moduleName, AppUserContext.CurrentUserName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load layout for {Module}", moduleName);
        }
    }

    private async Task OnSaveLayoutRequestedAsync()
    {
        try
        {
            var layoutJson = View.GetLayoutJson();
            if (string.IsNullOrEmpty(layoutJson))
            {
                View.ShowInfo("No layout available to save.");
                return;
            }

            await _layoutPersistenceService.SaveLayoutAsync(nameof(CountryListPresenter), layoutJson, AppUserContext.CurrentUserName);
            View.ShowSuccess("Layout saved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save layout");
            View.ShowError($"Failed to save layout: {ex.Message}");
        }
    }

    private async Task OnLoadLayoutRequestedAsync()
    {
        try
        {
            await LoadLayoutAsync();
            if (!string.IsNullOrEmpty(_loadedLayoutJson))
            {
                View.ApplyLayoutFromJson(_loadedLayoutJson);
                View.ShowSuccess("Layout applied.");
            }
            else
            {
                View.ShowInfo("No saved layout found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load/apply layout");
            View.ShowError($"Failed to load layout: {ex.Message}");
        }
    }

    // Named handlers for add/edit/delete/refresh follow (unchanged)
    private void OnAddCountryRequestedHandler(object? sender, EventArgs e) => _ = OnAddCountryRequested(sender, e);
    private void OnEditCountryRequestedHandler(object? sender, Guid id) => _ = OnEditCountryRequested(sender, id);
    private void OnDeleteCountryRequestedHandler(object? sender, Guid id) => _ = OnDeleteCountryRequestedAsync(sender, id);
    private void OnRefreshRequestedHandler(object? sender, EventArgs e) => _ = OnRefreshRequestedAsync(sender, e);

    private async Task OnAddCountryRequested(object? sender, EventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Add country requested");

            var editForm = _countryEditViewFactory.Create();
            if (editForm is Form form)
            {
                form.Text = "Add New Country";
            }

            editForm.ClearForm();

            CountryEditPresenter? presenter = null;
            try
            {
                presenter = ActivatorUtilities.CreateInstance<CountryEditPresenter>(_serviceProvider, editForm);
                presenter.Initialize();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create CountryEditPresenter");
                View.ShowError($"Failed to open Country edit dialog: {ex.Message}");
                return;
            }

            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Invalidate country caches after create
                await _cacheService.RemoveByPatternAsync(CacheKeys.CountryPattern());

                await LoadCountriesAsync();
                View.ShowSuccess("Country created successfully.");
            }

            presenter.Cleanup();
            (presenter as IDisposable)?.Dispose();
        });
    }

    private async Task OnEditCountryRequested(object? sender, Guid countryId)
    {
        try
        {
            _logger.LogInformation("Edit country requested for country {CountryId}", countryId);

            var country = _cachedCountries.FirstOrDefault(u => u.Id == countryId);
            if (country == null)
            {
                _logger.LogWarning("Country {CountryId} not found in cache, fetching from database", countryId);
                country = await _mediator.Send(new GetCountryQuery(countryId));

                if (country == null)
                {
                    View.ShowError("Country not found.");
                    return;
                }
            }

            var editForm = _countryEditViewFactory.Create();
            if (editForm is Form form)
            {
                form.Text = "Edit Country";
            }

            CountryEditPresenter? presenter = null;
            try
            {
                presenter = ActivatorUtilities.CreateInstance<CountryEditPresenter>(_serviceProvider, editForm);
                presenter.SetEditMode(country);
                presenter.Initialize();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create CountryEditPresenter for edit");
                View.ShowError($"Failed to open Country edit dialog: {ex.Message}");
                return;
            }

            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Invalidate country caches after update
                await _cacheService.RemoveByPatternAsync(CacheKeys.CountryPattern());

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadCountriesAsync();
                        if (View is Control control)
                        {
                            control.Invoke(() => View.ShowSuccess("Country updated successfully."));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing countrys after edit");
                    }
                });
            }

            presenter.Cleanup();
            (presenter as IDisposable)?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in edit country operation");
            View.ShowError($"Failed to open country edit dialog: {ex.Message}");
        }
    }

    private async Task OnDeleteCountryRequestedAsync(object? sender, Guid countryId)
    {
        try
        {
            _logger.LogInformation("Delete country requested for country {CountryId}", countryId);

            var country = _cachedCountries.FirstOrDefault(u => u.Id == countryId);
            if (country == null)
            {
                View.ShowError("Country not found.");
                return;
            }

            var confirmMessage = $"Are you sure you want to delete country '{country.Id} {country.Name}'?";
            if (!View.ShowConfirmation(confirmMessage))
                return;

            await ExecuteAsync(async () =>
            {
                try
                {
                    View.ShowInfo($"Delete functionality for country '{country.Id} {country.Name}' would be implemented here.");

                    // Invalidate caches on delete
                    await _cacheService.RemoveByPatternAsync(CacheKeys.CountryPattern());

                    await LoadCountriesAsync();
                    View.ShowSuccess("Country deleted successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting country {CountryId}", countryId);
                    View.ShowError($"Failed to delete country: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in delete country operation");
            View.ShowError($"Failed to delete country: {ex.Message}");
        }
    }

    private async Task OnRefreshRequestedAsync(object? sender, EventArgs e)
    {
        await LoadCountriesAsync();
    }

    private async Task LoadCountriesAsync()
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading countrys");

            var cacheKey = CacheKeys.AllCountries();
            var countries = await _cacheService.GetAsync<List<CountryInfo>>(cacheKey);
            if (countries == null)
            {
                countries = await _mediator.Send(new GetAllCountriesQuery());
                var list = countries.ToList();
                await _cacheService.SetAsync(cacheKey, list, CacheTimeouts.Countries);
                _cachedCountries = list;
                _logger.LogInformation("Loaded countrys from database and cached");
            }
            else
            {
                _cachedCountries = countries.ToList();
                _logger.LogInformation("Loaded countrys from cache");
            }

            View.DisplayCountries(_cachedCountries);

            _logger.LogInformation("Loaded {CountryCount} countrys", _cachedCountries.Count());

            // Apply loaded layout if present
            if (!string.IsNullOrEmpty(_loadedLayoutJson))
            {
                try
                {
                    View.ApplyLayoutFromJson(_loadedLayoutJson);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to apply loaded layout");
                }
            }
        });
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.AddCountryRequested -= OnAddCountryRequestedHandler;
        View.EditCountryRequested -= OnEditCountryRequestedHandler;
        View.DeleteCountryRequested -= OnDeleteCountryRequestedHandler;
        View.RefreshRequested -= OnRefreshRequestedHandler;

        // Unsubscribe layout events
        View.SaveLayoutRequested -= OnSaveLayoutRequestedHandler;
        View.LoadLayoutRequested -= OnLoadLayoutRequestedHandler;

        base.Cleanup();
    }


}
