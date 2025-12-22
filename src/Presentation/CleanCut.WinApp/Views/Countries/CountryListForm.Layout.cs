using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CleanCut.WinApp.Views.Countries
{
    public partial class CountryListForm
    {
        /// <summary>
        /// Wire layout button handlers when the form is first shown.
        /// This avoids modifying the existing designer-generated constructor.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            try
            {
                if (_saveLayoutButton != null)
                    _saveLayoutButton.Click += async (s, ev) => await OnSaveLayoutClicked();
            }
            catch { }

            try
            {
                if (_loadLayoutButton != null)
                    _loadLayoutButton.Click += async (s, ev) => await OnLoadLayoutClicked();
            }
            catch { }
        }

        private Task OnSaveLayoutClicked()
        {
            try
            {
                // Raise event for presenter to handle persistence
                SaveLayoutRequested?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                // ignore UI-side event issues
            }
            return Task.CompletedTask;
        }

        private Task OnLoadLayoutClicked()
        {
            try
            {
                // Raise event for presenter to handle loading and applying
                LoadLayoutRequested?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                // ignore UI-side event issues
            }
            return Task.CompletedTask;
        }
    }
}
