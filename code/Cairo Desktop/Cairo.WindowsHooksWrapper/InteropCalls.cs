namespace Cairo.WindowsHooksWrapper
{
    using System;
    using System.Runtime.InteropServices;


    internal class InteropCalls
    {
        [DllImport("Cairo.WindowsHooks.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern void SetSystrayCallback(Delegate theDelegate);

        [DllImport("Cairo.WindowsHooks.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern void InitializeSystray();

        [DllImport("Cairo.WindowsHooks.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern void Run();

        [DllImport("Cairo.WindowsHooks.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern void ShutdownSystray();
    }
}
