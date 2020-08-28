using CairoDesktop.Common;
using CairoDesktop.Interop;
using System.Windows.Input;

namespace CairoDesktop.SupportingClasses
{
    internal class SystemHotKeys
    {
        internal static void RegisterSystemHotkeys()
        {
            new HotKey(Key.R, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinRCommand);
            new HotKey(Key.D, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinDCommand);
            new HotKey(Key.OemComma, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinDCommand);
            new HotKey(Key.E, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinECommand);
            new HotKey(Key.I, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinICommand);
            new HotKey(Key.Pause, HotKeyModifier.Win | HotKeyModifier.NoRepeat, OnWinPauseCommand);
        }

        private static void OnWinDCommand(HotKey obj)
        {
            DesktopManager.Instance.ToggleOverlay();
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

        // TODO: Add window management related HotKeys
        // Win + [up]
        // Win + [down]
        // Win + [left]
        // Win + [right]
    }
}