using System;

namespace CairoDesktop.SupportingClasses
{
    public class WindowManagerEventArgs : EventArgs
    {
        public bool DisplaysChanged;
        public ScreenSetupReason Reason;
    }
}