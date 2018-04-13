using System;
using System.Runtime.InteropServices;

namespace CairoDesktop.Interop
{
    public class WinSparkle
    {
        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_init();

        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_cleanup();

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_set_appcast_url(String url);

        [DllImport("WinSparkle.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_check_update_with_ui();

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void win_sparkle_set_automatic_check_for_updates(int state);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int win_sparkle_get_automatic_check_for_updates();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int win_sparkle_can_shutdown_callback_t();
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void win_sparkle_shutdown_request_callback_t();

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern long win_sparkle_set_can_shutdown_callback(win_sparkle_can_shutdown_callback_t callback);

        [DllImport("WinSparkle.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern long win_sparkle_set_shutdown_request_callback(win_sparkle_shutdown_request_callback_t callback);
    }
}
