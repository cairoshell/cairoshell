using System;
using System.Runtime.InteropServices;

namespace CairoDesktop.Interop
{
    public partial class NativeMethods
    {
        const string AdvApi32_DllName = "advapi32.dll";
        public const uint TOKENADJUSTPRIVILEGES = 0x00000020;
        public const uint TOKENQUERY = 0x00000008;

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

        [Flags]
        public enum REG_NOTIFY_CHANGE : uint
        {
            /// <summary>
            /// Notify the caller if a subkey is added or deleted
            /// </summary>
            NAME = 0x1,
            /// <summary>
            /// Notify the caller of changes to the attributes of the key,
            /// such as the security descriptor information
            /// </summary>
            ATTRIBUTES = 0x2,
            /// <summary>
            /// Notify the caller of changes to a value of the key. This can
            /// include adding or deleting a value, or changing an existing value
            /// </summary>
            LAST_SET = 0x4,
            /// <summary>
            /// Notify the caller of changes to the security descriptor of the key
            /// </summary>
            SECURITY = 0x8
        }

        [DllImport(AdvApi32_DllName, SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport(AdvApi32_DllName, SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, bool disableAllPrivileges, ref TOKEN_PRIVILEGES newState, uint bufferLength, IntPtr previousState, IntPtr returnLength);

        [DllImport(AdvApi32_DllName, SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport(AdvApi32_DllName)]
        public static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool watchSubtree, REG_NOTIFY_CHANGE notifyFilter, IntPtr hEvent, bool asynchronous);
    }
}
