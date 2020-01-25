using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DarkUI.Win32
{
    internal sealed class Native
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);
    }
}
