using CleanCut.BlazorSPA.Pages.Models;
using CleanCut.BlazorSPA.Pages.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace CleanCut.BlazorSPA.Pages;

public partial class ProductEdit : ComponentBase, IDisposable
{
    [Parameter]
    public Guid? Id { get; set; }

    protected SimpleProduct Model { get; private set; } = new();

    protected bool IsEditMode => Id.HasValue;

    [Inject]
    protected IProductState ProductState { get; set; } = null!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;
  
 

    private EditContext? _editContext;
    private CancellationTokenSource? _debounceCts;

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            var loaded = await ProductState.GetByIdAsync(Id.Value);
            if (loaded != null)
            {
                Model = loaded;
            }
        }

        if (Model is null || Model.Id == Guid.Empty)
        {
            Model = new SimpleProduct
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsAvailable = true,
                IsTaxable = true,
                Category = ProductCategory.Grocery,
                Tags = new List<string>()
            };
        }

        // Initialize EditContext and subscribe for debounced validation
        _editContext = new EditContext(Model);
        _editContext.OnFieldChanged += EditContext_OnFieldChanged;
    }

    private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
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
                    });
                }
            }
            catch (OperationCanceledException) { }
        }, token);
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


 


    protected async Task OnValidSubmitAsync()
    {
        if (IsEditMode)
        {
            Model.UpdatedAt = DateTime.UtcNow;
            await ProductState.UpdateAsync(Model);
        }
        else
        {
            await ProductState.CreateAsync(Model);
        }

        NavigationManager.NavigateTo("/productslist");
    }

    // helper to mark invalid fields
    protected string GetFieldCss(string fieldName)
    {
        if (_editContext == null) return string.Empty;
        var fi = new FieldIdentifier(Model, fieldName);
        return _editContext.GetValidationMessages(fi).Any() ? "is-invalid" : string.Empty;
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        if (_editContext != null) _editContext.OnFieldChanged -= EditContext_OnFieldChanged;
    }
}
