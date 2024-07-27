using System;
using ManagedShell.AppBar;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public class WindowManagerEventArgs : EventArgs
    {
        public bool DisplaysChanged;
        public bool IsFastSetup;
        public ScreenSetupReason Reason;
    }
}