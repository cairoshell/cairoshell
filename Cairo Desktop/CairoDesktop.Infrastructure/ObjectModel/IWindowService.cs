using System;
using System.Collections.Generic;
using ManagedShell.AppBar;

namespace CairoDesktop.Infrastructure.ObjectModel
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
