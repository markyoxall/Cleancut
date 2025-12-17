using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanCut.BlazorSPA.Pages.Models;
using FluentValidation;
using FluentValidation.Results;
using CleanCut.BlazorSPA.Pages.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CleanCut.BlazorSPA.Pages
{
    public partial class CountryEdit : ComponentBase, IDisposable
    {
        [Parameter]
        public Guid? Id { get; set; }

        protected bool IsEditMode => Id.HasValue;

        private EditContext? _editContext;
        private ValidationMessageStore? _messageStore;
        private CancellationTokenSource? _debounceCts;

        // Demo controls
        protected bool UseDebounce { get; set; } = true;
        protected int ValidationRuns { get; set; }
        protected DateTime? LastValidationTime { get; set; }
        protected bool IsWaitingForDebounce { get; set; }
        protected string Status { get; set; } = "Idle";
        protected int PendingKeystrokes { get; set; }
        // Messages shown when a simulated API call happens
        protected System.Collections.Generic.List<string> ApiCallMessages { get; set; } = new();
        protected int ApiCallCount => ApiCallMessages.Count;

        [Inject]
        protected Pages.State.IApiCallTracker ApiCallTracker { get; set; } = null!;

        protected SimpleCountry Model = new();

        [Inject]
        protected ICountryState CountryState { get; set; } = null!;

        [Inject]
        protected IValidator<SimpleCountry> CountryValidator { get; set; } = null!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            if (Id.HasValue)
            {
                var loaded = await CountryState.GetByIdAsync(Id.Value);
                if (loaded != null)
                {
                    Model = loaded;
                }
            }

            if (Model is null || Model.Id == Guid.Empty)
            {
                Model = new SimpleCountry
                {
                    Id = Guid.NewGuid(),
                    CountryName = string.Empty
                    // set other defaults if needed
                };
            }

            // Initialize EditContext and subscribe for debounced validation
            _editContext = new EditContext(Model);
            _editContext.OnFieldChanged += EditContext_OnFieldChanged;
        }

        private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            // track keystrokes since last API call
            PendingKeystrokes++;

            // If debounce is disabled, validate immediately on every keystroke
            if (!UseDebounce)
            {
                _debounceCts?.Cancel();
                _ = SimulateApiCallAsync();
                return;
            }

            // If enough keystrokes accumulated, trigger an API call now (shows "every 5 keystrokes")
            if (PendingKeystrokes >= 5)
            {
                _debounceCts?.Cancel();
                _ = SimulateApiCallAsync();
                return;
            }

            // Debounce mode: cancel previous and start a short delay before validating
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            IsWaitingForDebounce = true;
            var token = _debounceCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, token);
                    if (!token.IsCancellationRequested)
                    {
                        await SimulateApiCallAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    // debounce was canceled — clear pending flag
                    await InvokeAsync(() =>
                    {
                        IsWaitingForDebounce = false;
                        StateHasChanged();
                    });
                }
            }, token);
        }

        private async Task SimulateApiCallAsync()
        {
            // Run on UI thread and simulate validation + API call
            await InvokeAsync(async () =>
            {
                // run FluentValidation and update ValidationMessageStore
                if (_editContext != null && CountryValidator != null)
                {
                    _messageStore ??= new ValidationMessageStore(_editContext);
                    _messageStore.Clear();
                    ValidationResult result = await CountryValidator.ValidateAsync(Model);
                    foreach (var failure in result.Errors)
                    {
                        _messageStore.Add(new FieldIdentifier(Model, failure.PropertyName), failure.ErrorMessage);
                    }
                }
                _editContext?.NotifyValidationStateChanged();
                Status = "Validating...";
                IsWaitingForDebounce = false;
                StateHasChanged();
                await Task.Delay(150);
                Status = "Calling API...";
                // record a message for this simulated remote call
                var message = $"Calling remote API at {DateTime.UtcNow:HH:mm:ss.fff} (pending keystrokes: {PendingKeystrokes})";
                ApiCallMessages.Add(message);

                // also notify global tracker (non-critical)
                try
                {
                    ApiCallTracker?.Record(message);
                }
                catch
                {
                    // ignore tracker failures in demo
                }

                StateHasChanged();
                await Task.Delay(250);
                ValidationRuns++;
                LastValidationTime = DateTime.UtcNow;
                PendingKeystrokes = 0;
                Status = "Idle";
                StateHasChanged();
            });
        }

        protected async Task OnValidSubmitAsync()
        {
            if (IsEditMode)
            {
                await CountryState.UpdateAsync(Model);
            }
            else
            {
                await CountryState.CreateAsync(Model);
            }

            NavigationManager.NavigateTo("/countrieslist");
        }

        public void Dispose()
        {
            _debounceCts?.Cancel();
            if (_editContext != null) _editContext.OnFieldChanged -= EditContext_OnFieldChanged;
        }
        protected void Cancel()
        {
            NavigationManager.NavigateTo("/countrieslist");
        }

        protected void ValidateNow()
        {
            // immediate validation (bypass debounce) for demo
            if (_editContext != null && CountryValidator != null)
            {
                _messageStore ??= new ValidationMessageStore(_editContext);
                _messageStore.Clear();
                var result = CountryValidator.Validate(Model);
                foreach (var failure in result.Errors)
                {
                    _messageStore.Add(new FieldIdentifier(Model, failure.PropertyName), failure.ErrorMessage);
                }
                _editContext.NotifyValidationStateChanged();
            }
            ValidationRuns++;
            LastValidationTime = DateTime.UtcNow;
        }

        protected async Task SimulateTypingAsync()
        {
            // Simulate typing by updating the bound property multiple times with small delays.
            // This will trigger EditContext.OnFieldChanged via the bound InputText (oninput).
            var baseText = Model.CountryName ?? string.Empty;
            for (int i = 1; i <= 8; i++)
            {
                Model.CountryName = baseText + i.ToString();
                // notify the EditContext about the field change
                _editContext?.NotifyFieldChanged(new FieldIdentifier(Model, nameof(Model.CountryName)));
                await Task.Delay(120);
            }
        }

        // Handle oninput events from the manual input element
        protected void OnInputChanged(ChangeEventArgs e)
        {
            Model.CountryName = e?.Value?.ToString() ?? string.Empty;
            _editContext?.NotifyFieldChanged(new FieldIdentifier(Model, nameof(Model.CountryName)));
        }

        protected string GetFieldCss(string fieldName)
        {
            if (_editContext == null || Model == null)
            {
                return string.Empty;
            }

            var field = new FieldIdentifier(Model, fieldName);
            var hasErrors = _editContext.GetValidationMessages(field).Any();
            return hasErrors ? "is-invalid" : string.Empty;
        }
    }
}
