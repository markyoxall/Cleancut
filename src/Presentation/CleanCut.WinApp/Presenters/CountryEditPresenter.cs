using AutoMapper;
using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Countries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Presenters;

public class CountryEditPresenter : BasePresenter<ICountryEditView>
{

    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<CountryEditPresenter> _logger;
    private readonly Services.ICommandFactory _commandFactory;
    private CountryInfo? _existingCountry;
    private bool _isEditMode;

    public CountryEditPresenter(ICountryEditView view, IMediator mediator, IMapper mapper, Services.ICommandFactory commandFactory, ILogger<CountryEditPresenter> logger)
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SetEditMode(CountryInfo country)
    {
        _existingCountry = country;
        _isEditMode = true;

        var editModel = _mapper.Map<CountryEditViewModel>(country);
        View.SetCountryData(editModel);
    }


    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to view events (use wrapper handlers to avoid async void)
        View.SaveRequested += OnSaveRequestedHandler;
        View.CancelRequested += OnCancelRequested;

        if (!_isEditMode)
        {
            View.ClearForm();
        }
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.SaveRequested -= OnSaveRequestedHandler;
        View.CancelRequested -= OnCancelRequested;

        base.Cleanup();
    }
    private void OnSaveRequestedHandler(object? sender, EventArgs e) => _ = OnSaveRequested(sender, e);

    private async Task OnSaveRequested(object? sender, EventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            // Validate form
            var validationErrors = View.ValidateForm();
            if (validationErrors.Any())
            {
                var errorMessage = string.Join("\n", validationErrors.Values);
                View.ShowError($"Please fix the following errors:\n{errorMessage}");
                return;
            }

            var userData = View.GetCountryData();
            var dto = _mapper.Map<CountryInfo>(userData);

            try
            {
                if (_isEditMode && _existingCountry != null)
                {
                    // Update existing user
                    _logger.LogInformation("Updating user {countryId}", _existingCountry.Id);
                    var updateCommand = _commandFactory.UpdateCountryCommand(_existingCountry.Id, userData);

                    await _mediator.Send(updateCommand);
                    _logger.LogInformation("Country updated successfully");
                }
                else
                {
                    // Create new user
                    _logger.LogInformation("Creating new user");
                    var createCommand = _commandFactory.CreateCountryCommand(userData);

                    await _mediator.Send(createCommand);
                    _logger.LogInformation("Country created successfully");
                }

                // Close the dialog with OK result
                if (View is Form form)
                {
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user");
                View.ShowError($"Failed to save user: {ex.Message}");
            }
        });
    }

    private void OnCancelRequested(object? sender, EventArgs e)
    {
        _logger.LogInformation("Country edit cancelled");

        if (View is Form form)
        {
            form.DialogResult = DialogResult.Cancel;
            form.Close();
        }
    }

}
