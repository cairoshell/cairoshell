using CairoDesktop.SupportingClasses;
using System;
using System.Collections.Generic;
using ManagedShell.AppBar;

// TODO: Window services should move to the Infrastructure project

namespace CairoDesktop.Services
{
    public interface IWindowService : IDisposable
    {
        List<CairoAppBarWindow> Windows { get; }

        void Register();

        void HandleScreenAdded(AppBarScreen screen);

        void RefreshWindows(WindowManagerEventArgs args);

        void HandleScreenRemoved(string screenDeviceName);
    }
}
