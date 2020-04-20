using System;
using System.Runtime.InteropServices;

namespace CairoDesktop.Interop
{
    public partial class Shell
    {
        static NativeMethods.SYSTEM_POWER_CAPABILITIES spc;
        static bool hasFetchedCapabilities = false;

        /// <summary>
        /// Adjusts the current process's token privileges to allow it to shut down or reboot the machine.
        /// Throws an ApplicationException if an error is encountered.
        /// </summary>
        private static void AdjustTokenPrivilegesForShutdown()
        {
            IntPtr procHandle = System.Diagnostics.Process.GetCurrentProcess().Handle;
            IntPtr tokenHandle = IntPtr.Zero;

            bool tokenOpenResult = NativeMethods.OpenProcessToken(procHandle, NativeMethods.TOKENADJUSTPRIVILEGES | NativeMethods.TOKENQUERY, out tokenHandle);
            if (!tokenOpenResult)
            {
                throw new ApplicationException("Error attempting to open process token to raise level for shutdown.\nWin32 Error Code: " + Marshal.GetLastWin32Error());
            }

            long pluid = new long();
            bool privLookupResult = NativeMethods.LookupPrivilegeValue(null, "SeShutdownPrivilege", ref pluid);
            if (!privLookupResult)
            {
                throw new ApplicationException("Error attempting to lookup value for shutdown privilege.\n Win32 Error Code: " + Marshal.GetLastWin32Error());
            }

            NativeMethods.TOKEN_PRIVILEGES newPriv = new NativeMethods.TOKEN_PRIVILEGES();
            newPriv.Luid = pluid;
            newPriv.PrivilegeCount = 1;
            newPriv.Attributes = 0x00000002;

            bool tokenPrivResult = NativeMethods.AdjustTokenPrivileges(tokenHandle, false, ref newPriv, 0, IntPtr.Zero, IntPtr.Zero);
            if (!tokenPrivResult)
            {
                throw new ApplicationException("Error attempting to adjust the token privileges to allow shutdown.\n Win32 Error Code: " + Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Calls the shutdown method on the Win32 API.
        /// </summary>
        public static void Shutdown()
        {
            AdjustTokenPrivilegesForShutdown();
            NativeMethods.ExitWindowsEx((uint)(NativeMethods.ExitWindows.Shutdown | NativeMethods.ExitWindows.ForceIfHung), 0x0);
        }

        /// <summary>
        /// Calls the reboot method on the Win32 API.
        /// </summary>
        public static void Reboot()
        {
            AdjustTokenPrivilegesForShutdown();
            NativeMethods.ExitWindowsEx((uint)(NativeMethods.ExitWindows.Reboot | NativeMethods.ExitWindows.ForceIfHung), 0x0);
        }

        /// <summary>
        /// Calls the logoff method on the Win32 API.
        /// </summary>
        public static void Logoff()
        {
            NativeMethods.ExitWindowsEx((uint)NativeMethods.ExitWindows.Logoff, 0x0);
        }

        /// <summary>
        /// Calls the Sleep method on the Win32 Power Profile API.
        /// </summary>
        public static void Sleep()
        {
            NativeMethods.SetSuspendState(false, false, false);
        }

        /// <summary>
        /// Calls the Hibernate method on the Win32 Power Profile API.
        /// </summary>
        public static void Hibernate()
        {
            NativeMethods.SetSuspendState(true, false, false);
        }

        /// <summary>
        /// Calls the LockWorkStation method on the User32 API.
        /// </summary>
        public static void Lock()
        {
            NativeMethods.LockWorkStation();
        }

        private static void fetchCapabilities()
        {
            if (!hasFetchedCapabilities)
            {
                NativeMethods.GetPwrCapabilities(out spc);
                hasFetchedCapabilities = true;
            }
        }

        /// <summary>
        /// Returns true if the system supports hibernation.
        /// </summary>
        public static bool CanHibernate()
        {
            fetchCapabilities();

            return spc.HiberFilePresent && spc.SystemS4;
        }

        /// <summary>
        /// Returns true if the system supports sleep.
        /// </summary>
        public static bool CanSleep()
        {
            fetchCapabilities();

            return spc.SystemS3 || spc.SystemS2 || spc.SystemS1;
        }
    }
}
