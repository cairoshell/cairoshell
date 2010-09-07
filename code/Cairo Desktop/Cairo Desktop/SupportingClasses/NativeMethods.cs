using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CairoDesktop.SupportingClasses
{
    public sealed class NativeMethods
    {
        public const int HWND_TOPMOST = -1; // 0xffff 
        public const int HWND_BOTTOMMOST = 1;
        public const int SWP_NOSIZE = 1; // 0x0001  
        public const int SWP_NOMOVE = 2; // 0x0002  
        public const int SWP_NOZORDER = 4; // 0x0004  
        public const int SWP_NOACTIVATE = 16; // 0x0010  
        public const int SWP_SHOWWINDOW = 64; // 0x0040  
        public const int SWP_HIDEWINDOW = 128; // 0x0080  
        public const int SWP_DRAWFRAME = 32; // 0x0020 


        static NativeMethods()
        {
        }

        // Handling the close splash screen event
        [DllImport("kernel32.dll")]
        public static extern Int32 OpenEvent(Int32 DesiredAccess, bool InheritHandle, string Name);

        // OpenEvent DesiredAccess defines
        public const int EVENT_MODIFY_STATE = 0x00000002;

        [DllImport("kernel32.dll")]
        public static extern Int32 SetEvent(Int32 Handle);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(Int32 Handle);

        [DllImport("user32.dll")]
        public static extern Int32 SetShellWindow(IntPtr hWnd);

        [DllImport("USER32.dll")]
        extern public static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
    }
}
