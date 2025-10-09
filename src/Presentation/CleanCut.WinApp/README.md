# CleanCut.WinApp - Windows Application Presentation Layer

## Purpose in Clean Architecture

The **Windows Application Layer** provides a **desktop user interface** for your application using Windows Forms or WPF. It offers rich client functionality, offline capabilities, and direct integration with Windows features. This layer communicates with the Application Layer to execute business use cases while providing a native Windows experience.

## Key Principles

### 1. **Rich Desktop Experience**
- Native Windows look and feel
- Keyboard shortcuts and accessibility features
- Integration with Windows features (notifications, file system, etc.)
- Offline and online capabilities

### 2. **MVVM/MVP Pattern**
- Separation of concerns between UI and logic
- Testable presentation logic
- Data binding for responsive UI updates

### 3. **Performance Optimization**
- Efficient data loading and caching
- Background processing for long-running operations
- Responsive UI with async/await patterns

### 4. **User-Centric Design**
- Intuitive navigation and workflows
- Contextual menus and toolbars
- Proper error handling and user feedback

## Folder Structure

```
CleanCut.WinApp/
??? Forms/               # Windows Forms
?   ??? MainForm.cs
?   ??? CustomerForm.cs
?   ??? OrderForm.cs
?   ??? ProductForm.cs
?   ??? LoginForm.cs
??? UserControls/        # Reusable user controls
?   ??? CustomerListControl.cs
?   ??? OrderDetailsControl.cs
?   ??? ProductGridControl.cs
?   ??? SearchControl.cs
??? Services/            # UI-specific services
?   ??? FormFactory.cs
?   ??? DialogService.cs
?   ??? NotificationService.cs
?   ??? NavigationService.cs
??? ViewModels/          # Presentation logic (MVVM pattern)
?   ??? MainViewModel.cs
?   ??? CustomerViewModel.cs
?   ??? OrderViewModel.cs
?   ??? ProductViewModel.cs
??? Helpers/             # UI helper classes
?   ??? ControlExtensions.cs
?   ??? DataGridViewHelper.cs
?   ??? ValidationHelper.cs
?   ??? FormHelper.cs
??? Models/              # UI-specific models
?   ??? CustomerDisplayModel.cs
?   ??? OrderDisplayModel.cs
?   ??? ProductDisplayModel.cs
??? Resources/           # Resources and images
?   ??? Icons/
?   ??? Images/
?   ??? Strings.resx
??? Configuration/       # App configuration
    ??? AppSettings.cs
    ??? WindowsFormsConfig.cs
```

## What Goes Here

### Forms
- Main application windows
- Dialog boxes and modal forms
- Data entry and editing forms
- Reports and dashboards

### User Controls
- Reusable UI components
- Custom controls for specific functionality
- Composite controls that combine multiple elements

### ViewModels
- Presentation logic and state management
- Data binding properties
- Command handling
- Input validation

### Services
- UI-specific services (dialogs, notifications)
- Form factories and navigation
- Background task management

## Example Patterns

### Main Form with Navigation
```csharp
public partial class MainForm : Form
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MainForm> _logger;

    public MainForm(IMediator mediator, IDialogService dialogService, 
                   INotificationService notificationService, ILogger<MainForm> logger)
    {
        InitializeComponent();
        _mediator = mediator;
        _dialogService = dialogService;
        _notificationService = notificationService;
        _logger = logger;
        
        InitializeMainForm();
    }

    private void InitializeMainForm()
    {
        // Setup menu and toolbar
        SetupMenus();
        SetupToolbar();
        SetupStatusBar();
        
        // Load initial data
        LoadDashboard();
    }

    private void SetupMenus()
    {
        // File menu
        var fileMenu = new ToolStripMenuItem("&File");
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("&New Customer", null, NewCustomer_Click, Keys.Control | Keys.N));
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("&New Order", null, NewOrder_Click, Keys.Control | Keys.O));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, Exit_Click, Keys.Alt | Keys.F4));

        // View menu
        var viewMenu = new ToolStripMenuItem("&View");
        viewMenu.DropDownItems.Add(new ToolStripMenuItem("&Customers", null, ShowCustomers_Click, Keys.F2));
        viewMenu.DropDownItems.Add(new ToolStripMenuItem("&Orders", null, ShowOrders_Click, Keys.F3));
        viewMenu.DropDownItems.Add(new ToolStripMenuItem("&Products", null, ShowProducts_Click, Keys.F4));

        // Add to main menu
        menuStrip.Items.Add(fileMenu);
        menuStrip.Items.Add(viewMenu);
    }

    private async void NewCustomer_Click(object sender, EventArgs e)
    {
        try
        {
            using var customerForm = new CustomerForm(_mediator, _dialogService);
            customerForm.Text = "New Customer";
            
            if (customerForm.ShowDialog(this) == DialogResult.OK)
            {
                _notificationService.ShowSuccess("Customer created successfully!");
                await RefreshCurrentView();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new customer");
            _dialogService.ShowError("An error occurred while creating the customer.", "Error");
        }
    }

    private async void ShowCustomers_Click(object sender, EventArgs e)
    {
        await ShowPanel<CustomerListControl>();
    }

    private async Task ShowPanel<T>() where T : UserControl, new()
    {
        try
        {
            // Clear current content
            mainPanel.Controls.Clear();
            
            // Create and configure new control
            var control = new T { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(control);
            
            // Initialize control if it has async initialization
            if (control is IAsyncInitializable asyncControl)
            {
                ShowLoadingIndicator();
                await asyncControl.InitializeAsync();
                HideLoadingIndicator();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing panel {PanelType}", typeof(T).Name);
            _dialogService.ShowError("An error occurred while loading the requested view.", "Error");
        }
    }

    private async void LoadDashboard()
    {
        try
        {
            ShowLoadingIndicator();
            
            // Load dashboard data
            var dashboardData = await _mediator.Send(new GetDashboardDataQuery());
            
            // Update dashboard controls
            UpdateDashboardStats(dashboardData);
            
            HideLoadingIndicator();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            HideLoadingIndicator();
            _dialogService.ShowError("Failed to load dashboard data.", "Error");
        }
    }

    private void ShowLoadingIndicator()
    {
        statusLabel.Text = "Loading...";
        progressBar.Visible = true;
        progressBar.Style = ProgressBarStyle.Marquee;
    }

    private void HideLoadingIndicator()
    {
        statusLabel.Text = "Ready";
        progressBar.Visible = false;
    }
}
```

### Customer Form with Validation
```csharp
public partial class CustomerForm : Form
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private CustomerDto _customer;
    private bool _isEditing;

    public CustomerForm(IMediator mediator, IDialogService dialogService, CustomerDto customer = null)
    {
        InitializeComponent();
        _mediator = mediator;
        _dialogService = dialogService;
        _customer = customer;
        _isEditing = customer != null;
        
        InitializeForm();
    }

    private void InitializeForm()
    {
        // Setup validation
        SetupValidation();
        
        // Load data if editing
        if (_isEditing)
        {
            LoadCustomerData();
        }
        
        // Setup event handlers
        saveButton.Click += SaveButton_Click;
        cancelButton.Click += CancelButton_Click;
        
        // Focus first control
        nameTextBox.Focus();
    }

    private void SetupValidation()
    {
        // Name validation
        nameTextBox.Validating += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                errorProvider.SetError(nameTextBox, "Customer name is required");
                e.Cancel = true;
            }
            else if (nameTextBox.Text.Length > 100)
            {
                errorProvider.SetError(nameTextBox, "Name cannot exceed 100 characters");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(nameTextBox, "");
            }
        };

        // Email validation
        emailTextBox.Validating += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(emailTextBox.Text))
            {
                errorProvider.SetError(emailTextBox, "Email address is required");
                e.Cancel = true;
            }
            else if (!IsValidEmail(emailTextBox.Text))
            {
                errorProvider.SetError(emailTextBox, "Invalid email address format");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(emailTextBox, "");
            }
        };

        // Phone validation
        phoneTextBox.Validating += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(phoneTextBox.Text) && !IsValidPhone(phoneTextBox.Text))
            {
                errorProvider.SetError(phoneTextBox, "Invalid phone number format");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(phoneTextBox, "");
            }
        };
    }

    private void LoadCustomerData()
    {
        if (_customer == null) return;

        nameTextBox.Text = _customer.Name;
        emailTextBox.Text = _customer.Email;
        phoneTextBox.Text = _customer.Phone;
        isVipCheckBox.Checked = _customer.IsVip;
        
        // Load address data
        if (_customer.Address != null)
        {
            streetTextBox.Text = _customer.Address.Street;
            cityTextBox.Text = _customer.Address.City;
            stateTextBox.Text = _customer.Address.State;
            postalCodeTextBox.Text = _customer.Address.PostalCode;
            countryTextBox.Text = _customer.Address.Country;
        }
    }

    private async void SaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Validate all controls
            if (!ValidateChildren())
            {
                _dialogService.ShowWarning("Please correct the validation errors before saving.", "Validation Error");
                return;
            }

            // Disable form during save
            EnableControls(false);
            
            if (_isEditing)
            {
                await UpdateCustomer();
            }
            else
            {
                await CreateCustomer();
            }
            
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            EnableControls(true);
            _dialogService.ShowError($"An error occurred while saving: {ex.Message}", "Error");
        }
    }

    private async Task CreateCustomer()
    {
        var command = new CreateCustomerCommand
        {
            Name = nameTextBox.Text.Trim(),
            Email = emailTextBox.Text.Trim(),
            Phone = phoneTextBox.Text.Trim(),
            IsVip = isVipCheckBox.Checked,
            Address = new AddressDto
            {
                Street = streetTextBox.Text.Trim(),
                City = cityTextBox.Text.Trim(),
                State = stateTextBox.Text.Trim(),
                PostalCode = postalCodeTextBox.Text.Trim(),
                Country = countryTextBox.Text.Trim()
            }
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            throw new ApplicationException(string.Join(", ", result.Errors));
        }
    }

    private async Task UpdateCustomer()
    {
        var command = new UpdateCustomerCommand
        {
            CustomerId = _customer.Id,
            Name = nameTextBox.Text.Trim(),
            Email = emailTextBox.Text.Trim(),
            Phone = phoneTextBox.Text.Trim(),
            IsVip = isVipCheckBox.Checked,
            Address = new AddressDto
            {
                Street = streetTextBox.Text.Trim(),
                City = cityTextBox.Text.Trim(),
                State = stateTextBox.Text.Trim(),
                PostalCode = postalCodeTextBox.Text.Trim(),
                Country = countryTextBox.Text.Trim()
            }
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            throw new ApplicationException(string.Join(", ", result.Errors));
        }
    }

    private void EnableControls(bool enabled)
    {
        nameTextBox.Enabled = enabled;
        emailTextBox.Enabled = enabled;
        phoneTextBox.Enabled = enabled;
        isVipCheckBox.Enabled = enabled;
        streetTextBox.Enabled = enabled;
        cityTextBox.Enabled = enabled;
        stateTextBox.Enabled = enabled;
        postalCodeTextBox.Enabled = enabled;
        countryTextBox.Enabled = enabled;
        saveButton.Enabled = enabled;
        cancelButton.Enabled = enabled;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhone(string phone)
    {
        // Simple phone validation - customize as needed
        return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[\d\s\-\(\)\+\.]+$");
    }
}
```

### Customer List User Control
```csharp
public partial class CustomerListControl : UserControl, IAsyncInitializable
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private BindingList<CustomerDisplayModel> _customers;
    private Timer _searchTimer;

    public CustomerListControl()
    {
        InitializeComponent();
        InitializeControl();
    }

    // Constructor injection through DI container
    public CustomerListControl(IMediator mediator, IDialogService dialogService, INotificationService notificationService) : this()
    {
        _mediator = mediator;
        _dialogService = dialogService;
        _notificationService = notificationService;
    }

    private void InitializeControl()
    {
        // Setup search timer for delayed search
        _searchTimer = new Timer { Interval = 500 };
        _searchTimer.Tick += SearchTimer_Tick;
        
        // Setup event handlers
        searchTextBox.TextChanged += SearchTextBox_TextChanged;
        refreshButton.Click += RefreshButton_Click;
        addButton.Click += AddButton_Click;
        editButton.Click += EditButton_Click;
        deleteButton.Click += DeleteButton_Click;
        
        // Setup data grid
        SetupDataGrid();
    }

    public async Task InitializeAsync()
    {
        await LoadCustomers();
    }

    private void SetupDataGrid()
    {
        customersDataGridView.AutoGenerateColumns = false;
        customersDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        customersDataGridView.MultiSelect = false;
        
        // Add columns
        customersDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "Customer Name",
            DataPropertyName = nameof(CustomerDisplayModel.Name),
            Width = 150
        });
        
        customersDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Email",
            HeaderText = "Email Address",
            DataPropertyName = nameof(CustomerDisplayModel.Email),
            Width = 200
        });
        
        customersDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Phone",
            HeaderText = "Phone Number",
            DataPropertyName = nameof(CustomerDisplayModel.Phone),
            Width = 120
        });
        
        customersDataGridView.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CreatedAt",
            HeaderText = "Registration Date",
            DataPropertyName = nameof(CustomerDisplayModel.CreatedAtFormatted),
            Width = 120
        });
        
        customersDataGridView.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "IsVip",
            HeaderText = "VIP",
            DataPropertyName = nameof(CustomerDisplayModel.IsVip),
            Width = 60
        });
        
        // Event handlers
        customersDataGridView.SelectionChanged += CustomersDataGridView_SelectionChanged;
        customersDataGridView.CellDoubleClick += CustomersDataGridView_CellDoubleClick;
    }

    private async Task LoadCustomers(string searchTerm = null)
    {
        try
        {
            ShowLoadingIndicator(true);
            
            var query = new SearchCustomersQuery
            {
                SearchTerm = searchTerm,
                PageNumber = 1,
                PageSize = 1000 // Load all for desktop app
            };
            
            var result = await _mediator.Send(query);
            
            _customers = new BindingList<CustomerDisplayModel>(
                result.Data.Select(c => new CustomerDisplayModel(c)).ToList());
            
            customersDataGridView.DataSource = _customers;
            
            UpdateStatusLabel();
            UpdateButtonStates();
            
            ShowLoadingIndicator(false);
        }
        catch (Exception ex)
        {
            ShowLoadingIndicator(false);
            _dialogService.ShowError($"Failed to load customers: {ex.Message}", "Error");
        }
    }

    private void SearchTextBox_TextChanged(object sender, EventArgs e)
    {
        _searchTimer.Stop();
        _searchTimer.Start();
    }

    private async void SearchTimer_Tick(object sender, EventArgs e)
    {
        _searchTimer.Stop();
        await LoadCustomers(searchTextBox.Text);
    }

    private async void RefreshButton_Click(object sender, EventArgs e)
    {
        await LoadCustomers(searchTextBox.Text);
    }

    private async void AddButton_Click(object sender, EventArgs e)
    {
        using var customerForm = new CustomerForm(_mediator, _dialogService);
        customerForm.Text = "New Customer";
        
        if (customerForm.ShowDialog(this) == DialogResult.OK)
        {
            _notificationService.ShowSuccess("Customer created successfully!");
            await LoadCustomers(searchTextBox.Text);
        }
    }

    private async void EditButton_Click(object sender, EventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer == null) return;

        try
        {
            // Get full customer data
            var query = new GetCustomerQuery { CustomerId = selectedCustomer.Id };
            var customer = await _mediator.Send(query);
            
            using var customerForm = new CustomerForm(_mediator, _dialogService, customer);
            customerForm.Text = $"Edit Customer - {customer.Name}";
            
            if (customerForm.ShowDialog(this) == DialogResult.OK)
            {
                _notificationService.ShowSuccess("Customer updated successfully!");
                await LoadCustomers(searchTextBox.Text);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load customer data: {ex.Message}", "Error");
        }
    }

    private async void DeleteButton_Click(object sender, EventArgs e)
    {
        var selectedCustomer = GetSelectedCustomer();
        if (selectedCustomer == null) return;

        var result = _dialogService.ShowQuestion(
            $"Are you sure you want to delete customer '{selectedCustomer.Name}'?\n\nThis action cannot be undone.",
            "Confirm Delete");

        if (result == DialogResult.Yes)
        {
            try
            {
                var command = new DeleteCustomerCommand { CustomerId = selectedCustomer.Id };
                var deleteResult = await _mediator.Send(command);
                
                if (deleteResult.IsSuccess)
                {
                    _notificationService.ShowSuccess("Customer deleted successfully!");
                    await LoadCustomers(searchTextBox.Text);
                }
                else
                {
                    _dialogService.ShowError(string.Join(", ", deleteResult.Errors), "Delete Failed");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to delete customer: {ex.Message}", "Error");
            }
        }
    }

    private void CustomersDataGridView_SelectionChanged(object sender, EventArgs e)
    {
        UpdateButtonStates();
    }

    private async void CustomersDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            await EditButton_Click(sender, e);
        }
    }

    private CustomerDisplayModel GetSelectedCustomer()
    {
        if (customersDataGridView.SelectedRows.Count > 0)
        {
            return customersDataGridView.SelectedRows[0].DataBoundItem as CustomerDisplayModel;
        }
        return null;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = GetSelectedCustomer() != null;
        editButton.Enabled = hasSelection;
        deleteButton.Enabled = hasSelection;
    }

    private void UpdateStatusLabel()
    {
        var count = _customers?.Count ?? 0;
        statusLabel.Text = $"{count} customer(s) loaded";
    }

    private void ShowLoadingIndicator(bool show)
    {
        loadingLabel.Visible = show;
        customersDataGridView.Enabled = !show;
        refreshButton.Enabled = !show;
        addButton.Enabled = !show;
    }
}

// Display model for customer list
public class CustomerDisplayModel
{
    public Guid Id { get; }
    public string Name { get; }
    public string Email { get; }
    public string Phone { get; }
    public DateTime CreatedAt { get; }
    public string CreatedAtFormatted => CreatedAt.ToString("MM/dd/yyyy");
    public bool IsVip { get; }

    public CustomerDisplayModel(CustomerDto customer)
    {
        Id = customer.Id;
        Name = customer.Name;
        Email = customer.Email;
        Phone = customer.Phone;
        CreatedAt = customer.CreatedAt;
        IsVip = customer.IsVip;
    }
}
```

### Dialog Service Implementation
```csharp
public interface IDialogService
{
    void ShowInformation(string message, string title = "Information");
    void ShowWarning(string message, string title = "Warning");
    void ShowError(string message, string title = "Error");
    DialogResult ShowQuestion(string message, string title = "Question");
    string ShowInputDialog(string prompt, string title = "Input", string defaultValue = "");
    string ShowSaveFileDialog(string filter = "All Files|*.*", string defaultExt = "");
    string ShowOpenFileDialog(string filter = "All Files|*.*");
}

public class DialogService : IDialogService
{
    public void ShowInformation(string message, string title = "Information")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ShowWarning(string message, string title = "Warning")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void ShowError(string message, string title = "Error")
    {
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public DialogResult ShowQuestion(string message, string title = "Question")
    {
        return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    }

    public string ShowInputDialog(string prompt, string title = "Input", string defaultValue = "")
    {
        using var inputForm = new InputDialog(prompt, title, defaultValue);
        return inputForm.ShowDialog() == DialogResult.OK ? inputForm.InputValue : null;
    }

    public string ShowSaveFileDialog(string filter = "All Files|*.*", string defaultExt = "")
    {
        using var dialog = new SaveFileDialog
        {
            Filter = filter,
            DefaultExt = defaultExt,
            AddExtension = true
        };
        
        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
    }

    public string ShowOpenFileDialog(string filter = "All Files|*.*")
    {
        using var dialog = new OpenFileDialog
        {
            Filter = filter,
            Multiselect = false
        };
        
        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
    }
}
```

## Key Technologies & Packages

### Required NuGet Packages
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

### Project References
- **CleanCut.Application** (for commands, queries, and DTOs)
- **CleanCut.Infrastructure.Shared** (for shared services)

## Configuration & Startup

### Program.cs for Windows Forms
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Setup dependency injection
            var host = CreateHostBuilder().Build();

            // Get main form from DI container
            var mainForm = host.Services.GetRequiredService<MainForm>();
            
            Application.Run(mainForm);
        }

        static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add application services
                    services.AddApplicationServices();
                    services.AddInfrastructureServices(context.Configuration);
                    
                    // Add Windows Forms specific services
                    services.AddTransient<IDialogService, DialogService>();
                    services.AddTransient<INotificationService, NotificationService>();
                    services.AddTransient<IFormFactory, FormFactory>();
                    
                    // Add forms
                    services.AddTransient<MainForm>();
                    services.AddTransient<CustomerForm>();
                    services.AddTransient<OrderForm>();
                    services.AddTransient<ProductForm>();
                    
                    // Add user controls
                    services.AddTransient<CustomerListControl>();
                    services.AddTransient<OrderListControl>();
                    services.AddTransient<ProductListControl>();
                });
    }
}
```

## Testing Strategy

### Unit Tests
- Test ViewModels and presentation logic
- Mock application services and dependencies
- Test user input validation

### Integration Tests
- Test form interactions with real services
- Verify data binding and UI updates
- Test navigation and workflow scenarios

### UI Automation Tests
- Use tools like White or FlaUI for automated UI testing
- Test critical user workflows
- Verify accessibility features

## Common Patterns

### Background Task Execution
```csharp
private async void LoadDataButton_Click(object sender, EventArgs e)
{
    try
    {
        loadingIndicator.Visible = true;
        loadDataButton.Enabled = false;
        
        // Use Task.Run for CPU-intensive work, direct await for I/O
        var data = await _mediator.Send(new GetLargeDataSetQuery());
        
        // Update UI on main thread
        dataGridView.DataSource = data;
    }
    catch (Exception ex)
    {
        _dialogService.ShowError($"Failed to load data: {ex.Message}", "Error");
    }
    finally
    {
        loadingIndicator.Visible = false;
        loadDataButton.Enabled = true;
    }
}
```

### Form Factory Pattern
```csharp
public interface IFormFactory
{
    T CreateForm<T>() where T : Form;
    UserControl CreateUserControl<T>() where T : UserControl;
}

public class FormFactory : IFormFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public FormFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public T CreateForm<T>() where T : Form
    {
        return _serviceProvider.GetRequiredService<T>();
    }
    
    public UserControl CreateUserControl<T>() where T : UserControl
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
```

## Common Mistakes to Avoid

? **UI Thread Blocking** - Don't perform long operations on UI thread
? **Memory Leaks** - Properly dispose forms and controls
? **Poor Error Handling** - Don't let exceptions crash the application
? **Direct Database Access** - Always go through Application Layer
? **Inconsistent UI** - Maintain consistent look and behavior

? **Async Operations** - Use async/await for I/O operations
? **Proper Disposal** - Implement IDisposable correctly
? **User Feedback** - Provide loading indicators and progress bars
? **Validation** - Validate user input before processing
? **Accessibility** - Support keyboard navigation and screen readers

This layer provides rich desktop functionality - make it responsive, intuitive, and robust!