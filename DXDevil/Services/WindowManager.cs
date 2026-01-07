using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DXDevil.Services
{
    /// <summary>
    /// Manages single-instance application-wide forms. Keeps strong references to open forms
    /// and ensures only one instance per form type is shown. Thread-safe.
    /// </summary>
    public sealed class WindowManager
    {
        private readonly Dictionary<Type, Form> _openForms = new();
        private readonly object _sync = new();

        private static readonly Lazy<WindowManager> _instance = new(() => new WindowManager());
        public static WindowManager Instance => _instance.Value;

        private WindowManager() { }

        /// <summary>
        /// Show a single-instance form of type T. If an instance already exists it will be activated.
        /// Otherwise the <paramref name="create"/> factory is used to create, show and track a new instance.
        /// </summary>
        public T ShowSingle<T>(Func<T> create, Form? mdiParent = null) where T : Form
        {
            if (create == null) throw new ArgumentNullException(nameof(create));

            var key = typeof(T);
            lock (_sync)
            {
                if (_openForms.TryGetValue(key, out var existing) && !existing.IsDisposed)
                {
                    existing.WindowState = FormWindowState.Normal;
                    existing.BringToFront();
                    existing.Activate();
                    return (T)existing;
                }

                // create a new instance and track it
                var form = create();
                if (mdiParent != null)
                    form.MdiParent = mdiParent;

                // When the form closes remove it from the registry
                form.FormClosed += (s, e) =>
                {
                    lock (_sync)
                    {
                        _openForms.Remove(key);
                    }
                };

                _openForms[key] = form;
                form.Show();
                return form;
            }
        }
    }
}
