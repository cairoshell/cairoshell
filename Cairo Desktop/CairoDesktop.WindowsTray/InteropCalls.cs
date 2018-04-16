namespace CairoDesktop.WindowsTray
{
    using System;
    using System.Runtime.InteropServices;


    internal class InteropCalls
    {
        [DllImport("CairoDesktop.WindowsTrayHooks.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetSystrayCallback(Delegate theDelegate);

        [DllImport("CairoDesktop.WindowsTrayHooks.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetIconDataCallback(Delegate theDelegate);

        [DllImport("CairoDesktop.WindowsTrayHooks.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr InitializeSystray(int width, float scale);

        [DllImport("CairoDesktop.WindowsTrayHooks.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Run();

        [DllImport("CairoDesktop.WindowsTrayHooks.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ShutdownSystray();
    }
}
