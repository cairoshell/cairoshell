using System;

namespace CairoDesktop.Infrastructure.ObjectModel
{
    public class WindowManagerEventArgs : EventArgs
    {
        public bool DisplaysChanged;
        public bool IsFastSetup;
        public WindowManagerEventReason Reason;
    }
}