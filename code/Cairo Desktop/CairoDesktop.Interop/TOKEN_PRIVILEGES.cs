namespace CairoDesktop.Interop
{
    using System;
    using System.Runtime.InteropServices;
    /// <summary>
    /// Structure for the token privileges request.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TOKEN_PRIVILEGES
    {
        /// <summary>
        /// The number of privileges.
        /// </summary>
        public uint PrivilegeCount;

        /// <summary>
        /// The local UID for the request.
        /// </summary>
        public long Luid;

        /// <summary>
        /// Attributes for the request.
        /// </summary>
        public uint Attributes;
    }
}
