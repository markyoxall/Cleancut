using CleanCut.Application.Commands.Customers.CreateCustomer;
using CleanCut.Application.Commands.Customers.UpdateCustomer;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Customers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for Customer Edit View implementing MVP pattern
/// </summary>
public class CustomerEditPresenter : BasePresenter<ICustomerEditView>
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private CustomerInfo? _existingCustomer;
    private bool _isEditMode;

    public CustomerEditPresenter(ICustomerEditView view, IMediator mediator, ILogger logger) 
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SetEditMode(CustomerInfo user)
    {
        _existingCustomer = user;
        _isEditMode = true;
        
        var editModel = new CustomerEditModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive
        };
        
        View.SetCustomerData(editModel);
    }

    public override void Initialize()
    {
        base.Initialize();
        
        // Subscribe to view events
        View.SaveRequested += OnSaveRequested;
        View.CancelRequested += OnCancelRequested;
        
        if (!_isEditMode)
        {
            View.ClearForm();
        }
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.SaveRequested -= OnSaveRequested;
        View.CancelRequested -= OnCancelRequested;
        
        base.Cleanup();
    }

    private async void OnSaveRequested(object? sender, EventArgs e)
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

            var userData = View.GetCustomerData();

            try
            {
                if (_isEditMode && _existingCustomer != null)
                {
                    // Update existing user
                    _logger.LogInformation("Updating user {CustomerId}", _existingCustomer.Id);
                    
                    var updateCommand = new UpdateCustomerCommand(
                        _existingCustomer.Id,
                        userData.FirstName,
                        userData.LastName,
                        userData.Email
                    );
                    
                    await _mediator.Send(updateCommand);
                    _logger.LogInformation("Customer updated successfully");
                }
                else
                {
                    // Create new user
                    _logger.LogInformation("Creating new user");
                    
                    var createCommand = new CreateCustomerCommand(
                        userData.FirstName,
                        userData.LastName,
                        userData.Email
                    );
                    
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