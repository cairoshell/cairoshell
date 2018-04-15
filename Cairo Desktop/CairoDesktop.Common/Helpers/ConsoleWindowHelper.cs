using System.Runtime.InteropServices;

namespace CairoDesktop.Common.Helpers
{
    public class ConsoleWindowHelper
    {
        public static int Create()
        {
            if (AllocConsole())
                return 0;
            else
                return Marshal.GetLastWin32Error();
        }

        public static int Destroy()
        {
            if (FreeConsole())
                return 0;
            else
                return Marshal.GetLastWin32Error();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"),
        System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"),
        System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();
    }
}