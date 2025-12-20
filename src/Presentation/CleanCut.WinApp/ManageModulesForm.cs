using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp;

public partial class ManageModulesForm : BaseForm
{
    private readonly string _configPath;
    private List<ManageEntry> _entries = new();

    public ManageModulesForm()
    {
        InitializeComponent();
        _configPath = Path.Combine(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory(), "appsettings.json");
        LoadEntries();
    }

    private void LoadEntries()
    {
        try
        {
            using var stream = File.OpenRead(_configPath);
            using var doc = JsonDocument.Parse(stream);
            if (!doc.RootElement.TryGetProperty("ManagementModules", out var modules))
                return;

            _entries = modules.EnumerateArray().Select(e => new ManageEntry
            {
                Id = e.GetProperty("Id").GetString() ?? string.Empty,
                Title = e.GetProperty("Title").GetString() ?? string.Empty,
                ViewType = e.GetProperty("ViewType").GetString() ?? string.Empty,
                PresenterType = e.GetProperty("PresenterType").GetString() ?? string.Empty,
                Enabled = e.GetProperty("Enabled").GetBoolean()
            }).ToList();

            grid.DataSource = null;
            grid.DataSource = _entries;
        }
        catch (Exception ex)
        {
            ShowError("Failed to load modules configuration: " + ex.Message);
        }
    }

    private void SaveEntries()
    {
        try
        {
            string jsonText = File.ReadAllText(_configPath);
            var root = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonText) ?? new Dictionary<string, object>();

            var options = new JsonSerializerOptions { WriteIndented = true };

            // Build new ManagementModules array
            var modules = _entries.Select(e => new
            {
                Id = e.Id,
                Title = e.Title,
                ViewType = e.ViewType,
                PresenterType = e.PresenterType,
                Enabled = e.Enabled
            }).ToList();

            root["ManagementModules"] = modules;

            var outJson = JsonSerializer.Serialize(root, options);
            File.WriteAllText(_configPath, outJson);
        }
        catch (Exception ex)
        {
            ShowError("Failed to save modules configuration: " + ex.Message);
        }
    }

    private void OnAddClicked(object? sender, EventArgs e)
    {
        var entry = new ManageEntry { Id = Guid.NewGuid().ToString("n") };
        _entries.Add(entry);
        grid.DataSource = null;
        grid.DataSource = _entries;
    }

    private void OnRemoveClicked(object? sender, EventArgs e)
    {
        if (grid.CurrentRow?.DataBoundItem is ManageEntry entry)
        {
            _entries.Remove(entry);
            grid.DataSource = null;
            grid.DataSource = _entries;
        }
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        SaveEntries();
        Close();
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close();
    }

    private class ManageEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ViewType { get; set; } = string.Empty;
        public string PresenterType { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }
}
