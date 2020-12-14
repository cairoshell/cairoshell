using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

// TODO: This should be moved to the Application project, but AppBarWindow needs TLC first

namespace CairoDesktop.SupportingClasses
{
    public interface IWindowService : IDisposable
    {
        List<AppBarWindow> Windows { get; }

        void Initialize(WindowManager windowManager);

        void HandleScreenAdded(Screen screen);

        void RefreshWindows(WindowManagerEventArgs args);

        void HandleScreenRemoved(string screenDeviceName);
    }
}
