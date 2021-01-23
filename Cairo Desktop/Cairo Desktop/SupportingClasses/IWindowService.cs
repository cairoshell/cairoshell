using System;
using System.Collections.Generic;
using System.Windows.Forms;

// TODO: Window services should move to the Infrastructure project

namespace CairoDesktop.SupportingClasses
{
    public interface IWindowService : IDisposable
    {
        List<CairoAppBarWindow> Windows { get; }

        void Register();

        void HandleScreenAdded(Screen screen);

        void RefreshWindows(WindowManagerEventArgs args);

        void HandleScreenRemoved(string screenDeviceName);
    }
}
