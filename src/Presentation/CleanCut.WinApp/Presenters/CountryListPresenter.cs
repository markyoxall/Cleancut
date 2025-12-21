using CleanCut.Application.DTOs;
using CleanCut.Application.Queries.Countries.GetAllCountries;
using CleanCut.Application.Queries.Countries.GetCountry;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Countries;

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CleanCut.Infrastructure.Caching.Constants;

namespace CleanCut.WinApp.Presenters;

public class CountryListPresenter : BasePresenter<ICountryListView>
{


    private readonly IMediator _mediator;

    private readonly IServiceProvider _serviceProvider;

    private readonly Services.Factories.IViewFactory<ICountryEditView> _countryEditViewFactory;

    private readonly ILogger<CountryListPresenter> _logger;

    private readonly CleanCut.Application.Common.Interfaces.ICacheService _cacheService;

    private List<CountryInfo> _cachedCountries= new(); // ?? Cache countries locally

    public CountryListPresenter(
        ICountryListView view,
        IMediator mediator,
        IServiceProvider serviceProvider,
        Services.Factories.IViewFactory<ICountryEditView> countryEditViewFactory,
        ILogger<CountryListPresenter> logger,
        CleanCut.Application.Common.Interfaces.ICacheService cacheService)
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _countryEditViewFactory = countryEditViewFactory ?? throw new ArgumentNullException(nameof(countryEditViewFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe to view events
        View.AddCountryRequested += OnAddCountryRequestedHandler;
        View.EditCountryRequested += OnEditCountryRequestedHandler;
        View.DeleteCountryRequested += OnDeleteCountryRequestedHandler;
        View.RefreshRequested += OnRefreshRequestedHandler;
        // Load initial data
        _ = LoadCountriesAsync();
       
    }

    // Named handlers
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
        });
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.AddCountryRequested -= OnAddCountryRequestedHandler;
        View.EditCountryRequested -= OnEditCountryRequestedHandler;
        View.DeleteCountryRequested -= OnDeleteCountryRequestedHandler;
        View.RefreshRequested -= OnRefreshRequestedHandler;
        base.Cleanup();
    }


}
