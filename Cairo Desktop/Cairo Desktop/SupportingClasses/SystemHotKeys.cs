using CairoDesktop.Common;
using CairoDesktop.Interop;
using System.Windows.Input;

namespace CairoDesktop.SupportingClasses
{
    internal class SystemHotKeys
    {
        internal static void RegisterSystemHotkeys()
        {
            new HotKey(Key.R, KeyModifier.Win | KeyModifier.NoRepeat, OnWinRCommand);
            new HotKey(Key.D, KeyModifier.Win | KeyModifier.NoRepeat, OnWinDCommand);
            new HotKey(Key.OemComma, KeyModifier.Win | KeyModifier.NoRepeat, OnWinDCommand);
            new HotKey(Key.E, KeyModifier.Win | KeyModifier.NoRepeat, OnWinECommand);
            new HotKey(Key.I, KeyModifier.Win | KeyModifier.NoRepeat, OnWinICommand);
            new HotKey(Key.Pause, KeyModifier.Win | KeyModifier.NoRepeat, OnWinPauseCommand);
        }

        private static void OnWinDCommand(HotKey obj)
        {
            if (WindowManager.Instance.DesktopWindow != null)
                WindowManager.Instance.DesktopWindow.IsOverlayOpen = !WindowManager.Instance.DesktopWindow.IsOverlayOpen;
        }
        
        private static void OnWinRCommand(HotKey cmd)
        {
            Shell.ShowRunDialog();
        }

        private static void OnWinECommand(HotKey cmd)
        {
            FolderHelper.OpenLocation("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
        }

        private static void OnWinICommand(HotKey cmd)
        {
            Shell.StartProcess("control.exe");
        }

        private static void OnWinPauseCommand(HotKey cmd)
        {
            Shell.StartProcess("system.cpl");
        }

        // TODO: Add window management related hotkeys
    }
}
