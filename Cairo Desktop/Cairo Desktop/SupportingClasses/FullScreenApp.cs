using System;
using System.Windows.Forms;
using CairoDesktop.Interop;

namespace CairoDesktop.SupportingClasses
{
    public class FullScreenApp
    {
        public IntPtr hWnd;
        public Screen screen;
        public NativeMethods.Rect rect;
    }
}