using CairoDesktop.Common;
using CairoDesktop.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CairoDesktop
{
    internal class SystemHotKeys
    {
        internal static void RegisterSystemHotkeys()
        {
            new HotKey(Key.R, KeyModifier.Win | KeyModifier.NoRepeat, OnWinRCommand);
            new HotKey(Key.D, KeyModifier.Win | KeyModifier.NoRepeat, OnWinDCommand);

        }

        private static void OnWinDCommand(HotKey obj)
        {
            if (Startup.DesktopWindow != null)
                Startup.DesktopWindow.IsOverlayOpen = !Startup.DesktopWindow.IsOverlayOpen;
        }
        
        private static void OnWinRCommand(HotKey cmd)
        {
            Shell.ShowRunDialog();
        }
    }
}
