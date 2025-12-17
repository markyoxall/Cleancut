using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp;

/// <summary>
/// Main application form with navigation
/// </summary>
public partial class MainForm : BaseForm
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainForm> _logger;
    private CustomerListPresenter? _userListPresenter;
    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem exitMenuItem;
    private ToolStripMenuItem managementMenu;
    private ToolStripMenuItem userManagementMenuItem;
    private ToolStripMenuItem productManagementMenuItem;
    private ToolStripMenuItem countryManagementToolStripMenuItem;
    private ProductListPresenter? _productListPresenter;

    public MainForm(IServiceProvider serviceProvider, ILogger<MainForm> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeComponent();
        InitializeNavigation();
    }

    private void InitializeNavigation()
    {
        // Load the customer management by default
        LoadCustomerManagement();
    }

    private void LoadCustomerManagement()
    {
        try
        {
            _logger.LogInformation("Loading customer management");

            // Clean up existing presenters
            CleanupPresenters();

            // Create new view and presenter
            var userListView = _serviceProvider.GetRequiredService<ICustomerListView>();
            // Ensure the presenter receives the same view instance that will be shown
            _userListPresenter = Microsoft.Extensions.DependencyInjection.ActivatorUtilities
                .CreateInstance<CustomerListPresenter>(_serviceProvider, userListView);

            // Initialize presenter (will populate the same view instance that will be shown)
            _userListPresenter.Initialize();

            // Show the form
            if (userListView is Form form)
            {
                form.MdiParent = this;
                form.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer management");
            ShowError($"Failed to load customer management: {ex.Message}");
        }
    }

    private void LoadProductManagement()
    {
        try
        {
            _logger.LogInformation("Loading product management");

            // Clean up existing presenters
            CleanupPresenters();

            // Create new view and presenter
            var productListView = _serviceProvider.GetRequiredService<IProductListView>();
            // Ensure the presenter receives the same view instance that will be shown
            _productListPresenter = Microsoft.Extensions.DependencyInjection.ActivatorUtilities
                .CreateInstance<ProductListPresenter>(_serviceProvider, productListView);

            // Initialize presenter (will populate the same view instance that will be shown)
            _productListPresenter.Initialize();

            // Show the form
            if (productListView is Form form)
            {
                form.MdiParent = this;
                form.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product management");
            ShowError($"Failed to load product management: {ex.Message}");
        }
    }

    private void CleanupPresenters()
    {
        // ? Close and dispose any existing MDI child windows
        foreach (Form childForm in MdiChildren)
        {
            childForm.Close();
            childForm.Dispose(); // Explicitly dispose
        }

        // ? Clean up presenters
        _userListPresenter?.Cleanup();
        _userListPresenter = null;

        _productListPresenter?.Cleanup();
        _productListPresenter = null;
    }

    private void OnCustomerManagementClicked(object? sender, EventArgs e)
    {
        LoadCustomerManagement();
    }

    private void OnProductManagementClicked(object? sender, EventArgs e)
    {
        LoadProductManagement();
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        CleanupPresenters();
        base.OnFormClosed(e);
    }

    private void InitializeComponent()
    {
        menuStrip = new MenuStrip();
        fileMenu = new ToolStripMenuItem();
        exitMenuItem = new ToolStripMenuItem();
        managementMenu = new ToolStripMenuItem();
        userManagementMenuItem = new ToolStripMenuItem();
        productManagementMenuItem = new ToolStripMenuItem();
        countryManagementToolStripMenuItem = new ToolStripMenuItem();
        menuStrip.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, managementMenu });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1024, 24);
        menuStrip.TabIndex = 0;
        menuStrip.Text = "menuStrip";
        // 
        // fileMenu
        // 
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { exitMenuItem });
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(37, 20);
        fileMenu.Text = "File";
        // 
        // exitMenuItem
        // 
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.Size = new Size(92, 22);
        exitMenuItem.Text = "Exit";
        exitMenuItem.Click += OnExitClicked;
        // 
        // managementMenu
        // 
        managementMenu.DropDownItems.AddRange(new ToolStripItem[] { userManagementMenuItem, productManagementMenuItem, countryManagementToolStripMenuItem });
        managementMenu.Name = "managementMenu";
        managementMenu.Size = new Size(43, 20);
        managementMenu.Text = "man";
        // 
        // userManagementMenuItem
        // 
        userManagementMenuItem.Name = "userManagementMenuItem";
        userManagementMenuItem.Size = new Size(200, 22);
        userManagementMenuItem.Text = "Customer Management";
        userManagementMenuItem.Click += OnCustomerManagementClicked;
        // 
        // productManagementMenuItem
        // 
        productManagementMenuItem.Name = "productManagementMenuItem";
        productManagementMenuItem.Size = new Size(200, 22);
        productManagementMenuItem.Text = "Product Management";
        productManagementMenuItem.Click += OnProductManagementClicked;
        // 
        // countryManagementToolStripMenuItem
        // 
        countryManagementToolStripMenuItem.Name = "countryManagementToolStripMenuItem";
        countryManagementToolStripMenuItem.Size = new Size(200, 22);
        countryManagementToolStripMenuItem.Text = "Country Management";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        ClientSize = new Size(1024, 768);
        Controls.Add(menuStrip);
        IsMdiContainer = true;
        MainMenuStrip = menuStrip;
        Name = "MainForm";
        Text = "CleanCut Desktop Application";
        WindowState = FormWindowState.Maximized;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
