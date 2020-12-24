using System;
using ManagedShell.AppBar;

namespace CairoDesktop.SupportingClasses
{
    public class WindowManagerEventArgs : EventArgs
    {
        public bool DisplaysChanged;
        public ScreenSetupReason Reason;
    }
}