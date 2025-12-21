using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.Views.Countries;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;
using System.Text.Json;

namespace CleanCut.WinApp;

/// <summary>
/// Main application form with navigation
/// </summary>
public partial class MainForm : BaseForm
{
    private readonly Services.Management.IManagementLoader _managementLoader;
    private readonly ILogger<MainForm> _logger;
    private readonly IConfiguration _configuration;

    // Registry: module id -> factory
    private readonly Dictionary<string, Func<Task<Services.Management.ILoadedManagement>>> _managementFactories = new();
    private readonly Dictionary<string, ManagementModuleConfig> _moduleConfigs = new();

    // Track open management handles by module id (only one per module)
    private readonly Dictionary<string, Services.Management.ILoadedManagement> _openManagements = new();

    public MainForm(Services.Management.IManagementLoader managementLoader, ILogger<MainForm> logger, IConfiguration configuration)
    {
        _managementLoader = managementLoader ?? throw new ArgumentNullException(nameof(managementLoader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        InitializeComponent();
        // Cannot call async method directly in constructor, so use Load event
        this.Load += MainForm_Load;

        // Load modules from configuration (robust type resolution)
        ReloadModulesFromConfiguration();
    }

    private void ReloadModulesFromConfiguration()
    {
        _managementFactories.Clear();
        _moduleConfigs.Clear();

        try
        {
            // Prefer runtime config written in AppContext.BaseDirectory (where ManageModulesForm writes)
            var runtimeConfigPath = Path.Combine(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory(), "appsettings.json");

            string configPathToUse = runtimeConfigPath;

            // If runtime config doesn't exist, fall back to project file so developer still sees modules during debugging
            if (!File.Exists(configPathToUse))
            {
                var projectBase = GetProjectBasePath();
                var projectPath = Path.Combine(projectBase ?? string.Empty, "appsettings.json");
                if (File.Exists(projectPath))
                    configPathToUse = projectPath;
            }

            if (!File.Exists(configPathToUse))
            {
                _logger.LogInformation("No appsettings.json found at {Path}", configPathToUse);
                return;
            }

            using var stream = File.OpenRead(configPathToUse);
            using var doc = JsonDocument.Parse(stream);

            if (!doc.RootElement.TryGetProperty("ManagementModules", out var modules))
            {
                _logger.LogInformation("No ManagementModules section found in {Path}", configPathToUse);
                return;
            }

            var list = new List<ManagementModuleConfig>();
            foreach (var el in modules.EnumerateArray())
            {
                try
                {
                    var id = el.TryGetProperty("Id", out var idEl) ? idEl.GetString() ?? string.Empty : string.Empty;
                    var title = el.TryGetProperty("Title", out var tEl) ? tEl.GetString() ?? string.Empty : string.Empty;
                    var viewType = el.TryGetProperty("ViewType", out var vEl) ? vEl.GetString() : null;
                    var presenterType = el.TryGetProperty("PresenterType", out var pEl) ? pEl.GetString() : null;
                    var enabled = el.TryGetProperty("Enabled", out var enEl) && enEl.GetBoolean();

                    list.Add(new ManagementModuleConfig(id, title, viewType, presenterType, enabled));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping invalid management module entry in appsettings.json");
                }
            }

            foreach (var mod in list.Where(m => m.Enabled))
            {
                if (string.IsNullOrWhiteSpace(mod.Id))
                {
                    _logger.LogWarning("Skipping management module with missing Id: {Title}", mod.Title);
                    continue;
                }

                var viewType = ResolveType(mod.ViewType);
                var presenterType = ResolveType(mod.PresenterType);

                if (viewType == null || presenterType == null)
                {
                    _logger.LogWarning("Could not load types for management module {Id}: view={ViewType}, presenter={PresenterType}", mod.Id, mod.ViewType, mod.PresenterType);
                    continue;
                }

                _moduleConfigs[mod.Id] = mod;
                _managementFactories[mod.Id] = CreateFactory(viewType, presenterType);
                _logger.LogInformation("Registered management module {Id} -> view={View}, presenter={Presenter}", mod.Id, viewType.FullName, presenterType.FullName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load management modules from configuration file");
        }
    }

    private string? GetProjectBasePath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "CleanCut.WinApp.csproj")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    private Type? ResolveType(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        try
        {
            var t = Type.GetType(typeName, throwOnError: false);
            if (t != null)
                return t;
        }
        catch { }

        var trimmed = typeName.Split(',')[0].Trim();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var found = asm.GetType(trimmed, throwOnError: false, ignoreCase: false);
                if (found != null)
                    return found;

                var byFull = asm.GetTypes().FirstOrDefault(x => string.Equals(x.FullName, trimmed, StringComparison.Ordinal));
                if (byFull != null)
                    return byFull;

                var byName = asm.GetTypes().FirstOrDefault(x => string.Equals(x.Name, trimmed, StringComparison.OrdinalIgnoreCase));
                if (byName != null)
                    return byName;
            }
            catch (ReflectionTypeLoadException) { }
            catch { }
        }

        var parts = typeName.Split(',').Select(p => p.Trim()).ToArray();
        if (parts.Length >= 2)
        {
            var asmName = parts[1];
            try
            {
                var asm = Assembly.Load(new AssemblyName(asmName));
                var candidate = asm.GetTypes().FirstOrDefault(t => string.Equals(t.FullName, parts[0], StringComparison.Ordinal) || string.Equals(t.Name, parts[0], StringComparison.OrdinalIgnoreCase));
                if (candidate != null)
                    return candidate;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Assembly load failed for {Assembly}", asmName);
            }
        }

        _logger.LogDebug("ResolveType could not find type for '{TypeName}'", typeName);
        return null;
    }

    private Func<Task<Services.Management.ILoadedManagement>> CreateFactory(Type viewType, Type presenterType)
    {
        MethodInfo? loadAsyncMethod = _managementLoader.GetType().GetMethod("LoadAsync", BindingFlags.Public | BindingFlags.Instance);
        if (loadAsyncMethod == null)
            throw new InvalidOperationException("Management loader does not expose a LoadAsync method");

        MethodInfo generic = loadAsyncMethod.MakeGenericMethod(viewType, presenterType);

        return async () =>
        {
            var taskObj = generic.Invoke(_managementLoader, null) as Task;
            if (taskObj == null)
                throw new InvalidOperationException("Failed to invoke LoadAsync on management loader");

            await taskObj.ConfigureAwait(false);

            var resultProperty = taskObj.GetType().GetProperty("Result");
            if (resultProperty == null)
                throw new InvalidOperationException("Unable to get Result from LoadAsync task");

            var loaded = resultProperty.GetValue(taskObj) as Services.Management.ILoadedManagement;
            if (loaded == null)
                throw new InvalidOperationException("LoadAsync returned null or unexpected type");

            return loaded;
        };
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        // Build menu from current factories
        BuildManagementMenu();
        await InitializeNavigationAsync();
    }

    private void BuildManagementMenu()
    {
        // managementMenu is created in Designer; populate at runtime based on config
        managementMenu.DropDownItems.Clear();

        foreach (var id in _managementFactories.Keys.OrderBy(k => k))
        {
            var config = _moduleConfigs.ContainsKey(id) ? _moduleConfigs[id] : null;
            var title = config?.Title ?? id;

            var menuItem = new ToolStripMenuItem(title) { Tag = id };
            menuItem.Click += OnManagementMenuItemClicked;
            managementMenu.DropDownItems.Add(menuItem);
        }

        managementMenu.Visible = managementMenu.DropDownItems.Count > 0;
    }

    private async void OnManagementMenuItemClicked(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item && item.Tag is string id && _managementFactories.TryGetValue(id, out var factory))
        {
            await LoadManagementAsync(id, factory);
        }
    }

    private async Task InitializeNavigationAsync()
    {
        var first = _managementFactories.Keys.FirstOrDefault();
        if (!string.IsNullOrEmpty(first) && _managementFactories.TryGetValue(first, out var factory))
        {
            await LoadManagementAsync(first, factory);
        }
    }

    private async Task LoadManagementAsync(string id, Func<Task<Services.Management.ILoadedManagement>> factory)
    {
        try
        {
            // Check if a form for this module is already open
            if (_openManagements.TryGetValue(id, out var existing) && existing.View is Form existingForm && !existingForm.IsDisposed)
            {
                existingForm.BringToFront();
                existingForm.WindowState = FormWindowState.Normal;
                existingForm.Activate();
                return;
            }

            var loaded = await factory();
            if (loaded == null)
                throw new InvalidOperationException("Factory returned null for management load");

            // Track this management handle by module id
            _openManagements[id] = loaded;

            if (loaded.View is Form form)
            {
                form.MdiParent = this;
                // When the form closes, clean up its resources and remove from tracking
                form.FormClosed += (s, e) =>
                {
                    try
                    {
                        loaded.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error disposing management handle for module {Id}", id);
                    }
                    _openManagements.Remove(id);
                };
                form.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading management module {Id}", id);
            ShowError($"Failed to load management module {id}: {ex.Message}");
        }
    }

    private void OnManageModulesClicked(object? sender, EventArgs e)
    {
        var dlg = new ManageModulesForm();
        dlg.ShowDialog();

        // After dialog closes, reload configuration and rebuild factories so changes take effect
        ReloadModulesFromConfiguration();
        BuildManagementMenu();
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        Close();
    }
}
