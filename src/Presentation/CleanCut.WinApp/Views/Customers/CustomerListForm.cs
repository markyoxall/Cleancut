using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Presenters;

namespace CleanCut.WinApp.Views.Customers;

/// <summary>
/// Customer List Form implementing MVP pattern
/// </summary>
public partial class CustomerListForm : BaseForm, ICustomerListView
{
    private Button _savePrefsButton = null!;
    private CustomerListPresenter _presenter;
    public event EventHandler? AddCustomerRequested;
    public event EventHandler<Guid>? EditCustomerRequested;
    public event EventHandler<Guid>? DeleteCustomerRequested;
    public event EventHandler? RefreshRequested;

    public CustomerListForm()
    {
        InitializeComponent();
        AddSavePrefsButton();
        SetupEventHandlers();
    }

    // Allow presenter to be set for callbacks
    public void SetPresenter(CustomerListPresenter presenter)
    {
        _presenter = presenter;
    }

    private void SetupEventHandlers()
    {
        _addButton.Click += (s, e) => AddCustomerRequested?.Invoke(this, EventArgs.Empty);
        _editButton.Click += (s, e) => {
            var userId = GetSelectedCustomerId();
            if (userId.HasValue)
                EditCustomerRequested?.Invoke(this, userId.Value);
        };
        _deleteButton.Click += (s, e) => {
            var userId = GetSelectedCustomerId();
            if (userId.HasValue)
                DeleteCustomerRequested?.Invoke(this, userId.Value);
        };
        _refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _gridView.FocusedRowChanged += (s, e) => UpdateButtonStates();
    }

    private void AddSavePrefsButton()
    {
        _savePrefsButton = new Button();
        _savePrefsButton.Text = "Save Preferences";
        _savePrefsButton.Size = new System.Drawing.Size(120, 30);
        _savePrefsButton.Location = new System.Drawing.Point(350, 380);
        _savePrefsButton.UseVisualStyleBackColor = true;
        _savePrefsButton.Click += async (s, e) => await OnSavePrefsClicked();
        Controls.Add(_savePrefsButton);
    }

    private async Task OnSavePrefsClicked()
    {
        // Gather current column order and widths based on VisibleIndex
        var columns = new List<DevExpress.XtraGrid.Columns.GridColumn>();
        for (int i = 0; i < _gridView.Columns.Count; i++)
            columns.Add(_gridView.Columns[i]);
        columns.Sort((a, b) => a.VisibleIndex.CompareTo(b.VisibleIndex));

        var columnOrder = new List<string>();
        var columnWidths = new Dictionary<string, int>();
        foreach (var col in columns)
        {
            if (col.Visible)
            {
                columnOrder.Add(col.FieldName);
                columnWidths[col.FieldName] = (int)col.Width;
            }
        }
        if (_presenter != null)
        {
            await _presenter.SaveGridPreferencesAsync(columnOrder, columnWidths);
            MessageBox.Show("Grid preferences saved! Close and reopen the Customers view to see them applied.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("Presenter not set. Preferences not saved.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void DisplayCustomers(IEnumerable<CustomerInfo> users)
    {
        if (InvokeRequired)
        {
            Invoke(() => DisplayCustomers(users));
            return;
        }

        // Show a dialog with the settings that have been read in before building the grid
        var columnOrder = _presenter?.Preferences?.ColumnOrder;
        var columnWidths = _presenter?.Preferences?.ColumnWidths;
        if (columnOrder != null && columnOrder.Count > 0)
        {
            string msg = "Loaded Preferences:\nOrder: " + string.Join(", ", columnOrder);
            if (columnWidths != null)
            {
                msg += "\nWidths: " + string.Join(", ", columnWidths.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            }
            //MessageBox.Show(msg, "Loaded Grid Preferences", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Always build columns from scratch before binding data, only from preferences if present
        _gridView.OptionsBehavior.AutoPopulateColumns = false;
        _gridView.Columns.Clear();

        if (columnOrder != null && columnOrder.Count > 0)
        {
            for (int i = 0; i < columnOrder.Count; i++)
            {
                var fieldName = columnOrder[i];
                var col = _gridView.Columns.AddField(fieldName);
                col.Visible = true;
                col.VisibleIndex = i;
                if (columnWidths != null && columnWidths.TryGetValue(fieldName, out int width))
                    col.Width = width;
            }
        }
        else
        {
            // If no preferences, add all properties as columns
            foreach (var prop in typeof(CustomerInfo).GetProperties())
            {
                var col = _gridView.Columns.AddField(prop.Name);
                col.Visible = true;
            }
        }

        _gridControl.DataSource = users is List<CustomerInfo> list ? list : new List<CustomerInfo>(users);
        UpdateButtonStates();
    }

    public void ClearCustomers()
    {
        if (InvokeRequired)
        {
            Invoke(ClearCustomers);
            return;
        }
        _gridControl.DataSource = null;
        UpdateButtonStates();
    }

    public Guid? GetSelectedCustomerId()
    {
        if (_gridView.FocusedRowHandle < 0)
            return null;
        var row = _gridView.GetRow(_gridView.FocusedRowHandle) as CustomerInfo;
        return row?.Id;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _gridView.FocusedRowHandle >= 0 && _gridView.GetRow(_gridView.FocusedRowHandle) is CustomerInfo;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    // Apply saved column order and widths
    public void ApplyGridPreferences(List<string>? columnOrder, Dictionary<string, int>? columnWidths)
    {
        if (_gridView.Columns.Count == 0)
            return;

        _gridView.BeginUpdate();
        try
        {
            // Set column order
            if (columnOrder != null)
            {
                int visibleIndex = 0;
                foreach (var fieldName in columnOrder)
                {
                    var col = _gridView.Columns.ColumnByFieldName(fieldName);
                    if (col != null)
                    {
                        col.VisibleIndex = visibleIndex++;
                    }
                }
            }

            // Set column widths
            if (columnWidths != null)
            {
                foreach (var kvp in columnWidths)
                {
                    var col = _gridView.Columns.ColumnByFieldName(kvp.Key);
                    if (col != null)
                    {
                        col.Width = kvp.Value;
                    }
                }
            }
        }
        finally
        {
            _gridView.EndUpdate();
        }
    }
}
