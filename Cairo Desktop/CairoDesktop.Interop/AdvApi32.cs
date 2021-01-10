using System;
using System.Runtime.InteropServices;

namespace CairoDesktop.Interop
{
    public class AdvApi32
    {
        const string AdvApi32_DllName = "advapi32.dll";

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

        [DllImport(AdvApi32_DllName)]
        public static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool watchSubtree, REG_NOTIFY_CHANGE notifyFilter, IntPtr hEvent, bool asynchronous);
    }
}
