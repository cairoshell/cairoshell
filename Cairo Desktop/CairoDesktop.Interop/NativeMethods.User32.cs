using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CairoDesktop.Interop
{
    public partial class NativeMethods
    {
        const string User32_DllName = "user32";

        [DllImport(User32_DllName)]
        public static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        public delegate bool CallBackPtr(IntPtr hwnd, int lParam);

        [DllImport(User32_DllName)]
        public static extern int EnumWindows(CallBackPtr callPtr, int lPar);

        [DllImport(User32_DllName, CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder sb, int Length);
    }
}
