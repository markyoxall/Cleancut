using System;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DXDevil.Services;
namespace DXDevil;

public partial class MainMenuForm : RibbonForm
{
    private readonly string? _defaultConn; private Form1? _form1Instance; private XtraForm1? _xtraFormInstance;
    public MainMenuForm(string? defaultConn)
    {
        _defaultConn = defaultConn;
        InitializeComponent();
    }

    private void OpenForm1_ItemClick(object? sender, ItemClickEventArgs e)
    {
        if (_form1Instance != null && !_form1Instance.IsDisposed)
        {
            _form1Instance.WindowState = FormWindowState.Normal;
            _form1Instance.BringToFront();
            _form1Instance.Activate();
            return;
        }

        _form1Instance = new Form1(new DXDevil.Data.SqlOrderRepository(_defaultConn)) { MdiParent = this };
        _form1Instance.FormClosed += (s, e) => _form1Instance = null;
        _form1Instance.Show();
    }

    private void OpenXtra_ItemClick(object? sender, ItemClickEventArgs e)
    {
        if (_xtraFormInstance != null && !_xtraFormInstance.IsDisposed)
        {
            _xtraFormInstance.WindowState = FormWindowState.Normal;
            _xtraFormInstance.BringToFront();
            _xtraFormInstance.Activate();
            return;
        }

        _xtraFormInstance = new XtraForm1() { MdiParent = this };
        _xtraFormInstance.FormClosed += (s, e) => _xtraFormInstance = null;
        _xtraFormInstance.Show();
    }

    private void OpenComplex_ItemClick(object? sender, ItemClickEventArgs e)
    {
        WindowManager.Instance.ShowSingle(() => new ComplexResponsiveForm(), this);
    }
}
