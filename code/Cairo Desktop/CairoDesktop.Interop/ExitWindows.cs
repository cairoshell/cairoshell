namespace CairoDesktop.Interop
{
    using System;
    using System.Runtime.InteropServices;
    /// <summary>
    /// Flags representing the ExitWindows reason codes.
    /// </summary>
    [Flags]
    public enum ExitWindows : uint
    {
        /// <summary>
        /// Log the user off.
        /// </summary>
        Logoff = 0x00,

        /// <summary>
        /// Shutdown the machine.
        /// </summary>
        Shutdown = 0x08,

        /// <summary>
        /// Reboots the machine.
        /// </summary>
        Reboot = 0x02,

        /// <summary>
        /// Forces the machine to perform the operation if the apps are hung.
        /// Use this in conjunction with one of the lower flags.
        /// </summary>
        ForceIfHung = 0x10
    }
}
