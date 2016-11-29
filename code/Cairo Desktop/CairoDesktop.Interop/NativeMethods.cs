using System.Windows.Input;
namespace CairoDesktop.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Container class for Win32 Native methods used within the desktop application (e.g. shutdown, sleep, et al).
    /// </summary>
    public class NativeMethods
    {
        private const uint TOKENADJUSTPRIVILEGES = 0x00000020;
        private const uint TOKENQUERY = 0x00000008;

        /// <summary>
        /// Calls the shutdown method on the Win32 API.
        /// </summary>
        public static void Shutdown()
        {
            AdjustTokenPrivilegesForShutdown();
            ExitWindowsEx((uint)(ExitWindows.Shutdown | ExitWindows.ForceIfHung), 0x0);
        }

        /// <summary>
        /// Calls the reboot method on the Win32 API.
        /// </summary>
        public static void Reboot()
        {
            AdjustTokenPrivilegesForShutdown();
            ExitWindowsEx((uint)(ExitWindows.Reboot | ExitWindows.ForceIfHung), 0x0);
        }

        /// <summary>
        /// Calls the logoff method on the Win32 API.
        /// </summary>
        public static void Logoff()
        {
            ExitWindowsEx((uint)ExitWindows.Logoff, 0x0);
        }

        /// <summary>
        /// Calls the Sleep method on the Win32 Power Profile API.
        /// </summary>
        public static void Sleep()
        {
            SetSuspendState(false, false, false);
        }

        public static void PostWindowsMessage(IntPtr hWnd, uint callback, uint uid, uint messageId)
        {
            PostMessage(hWnd, callback, uid, messageId);
        }

        public static IntPtr FindWindow(string className)
        {
            return FindWindow(className, string.Empty);
        }

        public static RECT GetWindowRectangle(IntPtr windowHandle)
        {
            RECT ret = new RECT();
            GetWindowRect(windowHandle, out ret);
            
            return ret;
        }

        public const int HWND_TOPMOST = -1; // 0xffff 
        public const int HWND_BOTTOMMOST = 1;
        public const int SWP_NOSIZE = 1; // 0x0001  
        public const int SWP_NOMOVE = 2; // 0x0002  
        public const int SWP_NOZORDER = 4; // 0x0004  
        public const int SWP_NOACTIVATE = 16; // 0x0010  
        public const int SWP_SHOWWINDOW = 64; // 0x0040  
        public const int SWP_HIDEWINDOW = 128; // 0x0080  
        public const int SWP_DRAWFRAME = 32; // 0x0020 
        
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

        #region Private Methods        
        /// <summary>
        /// Adjusts the current process's token privileges to allow it to shut down or reboot the machine.
        /// Throws an ApplicationException if an error is encountered.
        /// </summary>
        private static void AdjustTokenPrivilegesForShutdown()
        {
            IntPtr procHandle = System.Diagnostics.Process.GetCurrentProcess().Handle;
            IntPtr tokenHandle = IntPtr.Zero;

            bool tokenOpenResult = OpenProcessToken(procHandle, TOKENADJUSTPRIVILEGES | TOKENQUERY, out tokenHandle);
            if (!tokenOpenResult)
            {
                throw new ApplicationException("Error attempting to open process token to raise level for shutdown.\nWin32 Error Code: " + Marshal.GetLastWin32Error());
            }

            long pluid = new long();
            bool privLookupResult = LookupPrivilegeValue(null, "SeShutdownPrivilege", ref pluid);
            if (!privLookupResult)
            {
                throw new ApplicationException("Error attempting to lookup value for shutdown privilege.\n Win32 Error Code: " + Marshal.GetLastWin32Error());
            }

            TOKEN_PRIVILEGES newPriv = new TOKEN_PRIVILEGES();
            newPriv.Luid = pluid;
            newPriv.PrivilegeCount = 1;
            newPriv.Attributes = 0x00000002;

            bool tokenPrivResult = AdjustTokenPrivileges(tokenHandle, false, ref newPriv, 0, IntPtr.Zero, IntPtr.Zero);
            if (!tokenPrivResult)
            {
                throw new ApplicationException("Error attempting to adjust the token privileges to allow shutdown.\n Win32 Error Code: " + Marshal.GetLastWin32Error());
            }
        }

        #region P/Invokes
        [DllImport("user32.dll")]
        private static extern bool ExitWindowsEx(uint flags, uint reason);

        // There is a method for this in System.Windows.Forms, however it calls the same p/invoke and I would prefer not to reference that lib
        [DllImport("powrprof.dll")]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
        
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, bool disableAllPrivileges, ref TOKEN_PRIVILEGES newState, uint bufferLength, IntPtr previousState, IntPtr returnLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint callback, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError=true)]
        private static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        #endregion
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}
