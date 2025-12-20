using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp;

public partial class ManageModulesForm
{
    private DataGridView grid = null!;
    private Button addButton = null!;
    private Button removeButton = null!;
    private Button saveButton = null!;
    private Button cancelButton = null!;

    private void InitializeComponent()
    {
        this.grid = new DataGridView();
        this.addButton = new Button();
        this.removeButton = new Button();
        this.saveButton = new Button();
        this.cancelButton = new Button();

        this.SuspendLayout();

        // grid
        this.grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.grid.Location = new Point(12, 12);
        this.grid.Size = new Size(760, 380);
        this.grid.AutoGenerateColumns = true;

        // addButton
        this.addButton.Text = "Add";
        this.addButton.Location = new Point(12, 400);
        this.addButton.Size = new Size(75, 25);
        this.addButton.Click += OnAddClicked;

        // removeButton
        this.removeButton.Text = "Remove";
        this.removeButton.Location = new Point(100, 400);
        this.removeButton.Size = new Size(75, 25);
        this.removeButton.Click += OnRemoveClicked;

        // saveButton
        this.saveButton.Text = "Save";
        this.saveButton.Location = new Point(600, 400);
        this.saveButton.Size = new Size(75, 25);
        this.saveButton.Click += OnSaveClicked;

        // cancelButton
        this.cancelButton.Text = "Cancel";
        this.cancelButton.Location = new Point(688, 400);
        this.cancelButton.Size = new Size(75, 25);
        this.cancelButton.Click += OnCancelClicked;

        // Form
        this.ClientSize = new Size(784, 441);
        this.Controls.Add(this.grid);
        this.Controls.Add(this.addButton);
        this.Controls.Add(this.removeButton);
        this.Controls.Add(this.saveButton);
        this.Controls.Add(this.cancelButton);
        this.Text = "Manage Modules";

        this.ResumeLayout(false);
    }
}
