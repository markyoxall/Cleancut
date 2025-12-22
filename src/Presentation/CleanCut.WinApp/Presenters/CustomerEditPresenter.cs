using CleanCut.Application.Commands.Customers.CreateCustomer;
using CleanCut.Application.Commands.Customers.UpdateCustomer;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Customers;
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for Customer Edit View implementing MVP pattern
/// </summary>
public class CustomerEditPresenter : BasePresenter<ICustomerEditView>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerEditPresenter> _logger;
    private readonly Services.ICommandFactory _commandFactory;
    private readonly IValidator<CustomerEditViewModel>? _validator;
    private CustomerInfo? _existingCustomer;
    private bool _isEditMode;

    public CustomerEditPresenter(ICustomerEditView view, IMediator mediator, IMapper mapper, Services.ICommandFactory commandFactory, ILogger<CustomerEditPresenter> logger, IValidator<CustomerEditViewModel>? validator = null)
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator;
    }

    public void SetEditMode(CustomerInfo user)
    {
        _existingCustomer = user;
        _isEditMode = true;

        var editModel = _mapper.Map<CustomerEditViewModel>(user);
        View.SetCustomerData(editModel);
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
            // Clear any previous per-field errors
            try { View.ClearValidationErrors(); } catch { }

            // Prefer FluentValidation if available
            var userData = View.GetCustomerData();
            if (_validator != null)
            {
                var validationResult = await _validator.ValidateAsync(userData);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(f => f.PropertyName)
                        .ToDictionary(g => g.Key, g => string.Join("; ", g.Select(x => x.ErrorMessage)));

                    View.ShowValidationErrors(errors);
                    View.ShowError("Please fix validation errors before saving.");
                    return;
                }
            }
            else
            {
                // Fallback to existing ValidateForm behavior
                var validationErrors = View.ValidateForm();
                if (validationErrors.Any())
                {
                    var errorMessage = string.Join("\n", validationErrors.Values);
                    View.ShowError($"Please fix the following errors:\n{errorMessage}");
                    return;
                }
            }

            var dto = _mapper.Map<CustomerInfo>(userData);

            try
            {
                if (_isEditMode && _existingCustomer != null)
                {
                    // Update existing user
                    _logger.LogInformation("Updating user {CustomerId}", _existingCustomer.Id);
                    var updateCommand = _commandFactory.UpdateCustomerCommand(_existingCustomer.Id, userData);

                    await _mediator.Send(updateCommand);
                    _logger.LogInformation("Customer updated successfully");
                }
                else
                {
                    // Create new user
                    _logger.LogInformation("Creating new user");
                    var createCommand = _commandFactory.CreateCustomerCommand(userData);

                    await _mediator.Send(createCommand);
                    _logger.LogInformation("Customer created successfully");
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
        _logger.LogInformation("Customer edit cancelled");

        if (View is Form form)
        {
            form.DialogResult = DialogResult.Cancel;
            form.Close();
        }
    }
}
