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
            _userListPresenter = _serviceProvider.GetRequiredService<CustomerListPresenter>();
      
      // Initialize presenter
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
        _productListPresenter = _serviceProvider.GetRequiredService<ProductListPresenter>();
            
            // Initialize presenter
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
        var menuStrip = new MenuStrip();
        var managementMenu = new ToolStripMenuItem();
        var userManagementMenuItem = new ToolStripMenuItem();
        var productManagementMenuItem = new ToolStripMenuItem();
        var fileMenu = new ToolStripMenuItem();
        var exitMenuItem = new ToolStripMenuItem();
        
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
        fileMenu.DropDownItems.Add(exitMenuItem);
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(37, 20);
        fileMenu.Text = "File";
    
        // 
        // exitMenuItem
        // 
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.Size = new Size(93, 22);
        exitMenuItem.Text = "Exit";
        exitMenuItem.Click += OnExitClicked;
    
     // 
        // managementMenu
        // 
        managementMenu.DropDownItems.AddRange(new ToolStripItem[] { userManagementMenuItem, productManagementMenuItem });
      managementMenu.Name = "managementMenu";
  managementMenu.Size = new Size(90, 20);
   managementMenu.Text = "Management";
  
        // 
        // userManagementMenuItem
   // 
        userManagementMenuItem.Name = "userManagementMenuItem";
   userManagementMenuItem.Size = new Size(170, 22);
 userManagementMenuItem.Text = "Customer Management";
      userManagementMenuItem.Click += OnCustomerManagementClicked;
        
      // 
        // productManagementMenuItem
        // 
    productManagementMenuItem.Name = "productManagementMenuItem";
   productManagementMenuItem.Size = new Size(170, 22);
        productManagementMenuItem.Text = "Product Management";
     productManagementMenuItem.Click += OnProductManagementClicked;
  
  // 
    // MainForm
        // 
     ClientSize = new Size(1024, 768);
    Controls.Add(menuStrip);
  IsMdiContainer = true;
  MainMenuStrip = menuStrip;
   Text = "CleanCut Desktop Application";
        WindowState = FormWindowState.Maximized;
     
        ResumeLayout(false);
        PerformLayout();
    }
}