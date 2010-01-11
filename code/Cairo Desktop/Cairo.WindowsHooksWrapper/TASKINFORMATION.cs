namespace Cairo.WindowsHooksWrapper
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TASKINFORMATION
    {
        public string WindowName;
        public IntPtr WindowHandle;
        public IntPtr NewWindowHandle;
        public IntPtr WindowIcon;
        public WindowActions WindowAction; // Enum
        public IntPtr SystemMenu;
    }
}