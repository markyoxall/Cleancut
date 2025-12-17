using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using CleanCut.BlazorSPA.Pages.Models;
using CleanCut.BlazorSPA.Pages.State;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace CleanCut.BlazorSPA.Pages;

public partial class CustomerEdit : ComponentBase, IDisposable
{
    [Parameter]
    public Guid? Id { get; set; }

    protected SimpleCustomer Model { get; private set; } = new();

    protected bool IsEditMode => Id.HasValue;

    [Inject]
    protected ICustomerState CustomerState { get; set; } = null!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    private EditContext? _editContext;
    private CancellationTokenSource? _debounceCts;

    protected override async Task OnInitializedAsync()
    {
        if (IsEditMode && Id.HasValue)
        {
            var loaded = await CustomerState.GetByIdAsync(Id.Value);
            if (loaded != null)
            {
                Model = loaded;
            }
        }

        if (Model == null || Model.Id == Guid.Empty)
        {
            Model = new SimpleCustomer
            {
                Id = System.Guid.NewGuid(),
                MemberSince = DateTime.UtcNow,
                FavoriteColor = "#000000",
                Country = string.Empty,
                Tags = new List<string>()
            };
        }

        InitializeEditContext();
    }

    private void InitializeEditContext()
    {
        _editContext = new EditContext(Model!);
        _editContext.OnFieldChanged += EditContext_OnFieldChanged;
    }

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        // Debounce validation: wait 500ms after last change
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        var token = _debounceCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500, token);
                if (!token.IsCancellationRequested)
                {
                    await InvokeAsync(() =>
                    {
                        _editContext?.Validate();
                        StateHasChanged();
                        // Debounced validation completed
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }, token);
    }

  

    protected async Task OnValidSubmitAsync()
    {
        if (IsEditMode)
        {
            await CustomerState.UpdateAsync(Model);
        }
        else
        {
            await CustomerState.CreateAsync(Model);
        }

        NavigationManager.NavigateTo("/customerslist");
    }

    protected async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        var file = e.File;
        Model.UploadedFileName = file?.Name;
        Model.UploadedFileSize = file?.Size;
        await Task.CompletedTask;
        // Persist file selection to draft as well (no-op until draft storage is wired)
    }



    // Helper to bind website as string
    protected string? WebsiteString
    {
        get => Model.Website?.ToString();
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Model.Website = null;
            }
            else
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var u))
                    Model.Website = u;
                else
                    Model.Website = null;
            }
        }
    }

    // Helper to bind preferred contact time as hh:mm string
    protected string? PreferredContactTimeString
    {
        get => Model.PreferredContactTime?.ToString(@"hh\:mm");
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Model.PreferredContactTime = null;
            }
            else
            {
                if (TimeSpan.TryParse(value, out var ts))
                    Model.PreferredContactTime = ts;
                else
                    Model.PreferredContactTime = null;
            }
        }
    }

    protected string GetFieldCss(string fieldName)
    {
        if (_editContext is null)
            return string.Empty;

        var fi = new FieldIdentifier(Model!, fieldName);
        var hasErrors = _editContext.GetValidationMessages(fi).Any();
        return hasErrors ? "is-invalid" : string.Empty;
    }

    // Helper for tags CSV binding
    protected string TagsCsv
    {
        get => Model.Tags is null ? string.Empty : string.Join(",", Model.Tags);
        set
        {
            Model.Tags = string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : value.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
        }
    }


    public void Dispose()
    {
        _debounceCts?.Cancel();
        if (_editContext != null)
            _editContext.OnFieldChanged -= EditContext_OnFieldChanged;
    }
}
