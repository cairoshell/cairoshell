using System;
using System.Runtime.InteropServices;

namespace CairoDesktop.Interop
{
    public partial class NativeMethods
    {
        public const int DWM_TNP_VISIBLE = 0x8,
            DWM_TNP_OPACITY = 0x4,
            DWM_TNP_RECTDESTINATION = 0x1;

        const string DwmApi_DllName = "dwmapi";

        [DllImport(DwmApi_DllName)]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport(DwmApi_DllName)]
        public static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport(DwmApi_DllName)]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport(DwmApi_DllName)]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);

        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }
    }
}
