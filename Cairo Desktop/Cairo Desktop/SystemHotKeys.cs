using CairoDesktop.Common;
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

        #region WinR Command
        private static void OnWinRCommand(HotKey cmd)
        {
            string path;
            string dialogtitle;
            string dialogtext;

            dialogtitle = "Start Application";
            dialogtext = "Enter the name of the program file you wish to run.  You may also enter in URLs and items found in your path and applications list.";

            SHRunFileDialog(IntPtr.Zero, IntPtr.Zero,
            string.Empty,
            dialogtitle,
            dialogtext,
            RunFileDialogFlags.CalcDirectory
            );
        }


        [DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "#61", SetLastError = true)]
        private static extern bool SHRunFileDialog(IntPtr hwndOwner, IntPtr hIcon, string lpszPath, string lpszDialogTitle, string lpszDialogTextBody, RunFileDialogFlags uflags);

        [Flags()]
        public enum RunFileDialogFlags : uint
        {
            /// <summary>
            /// Don't use any of the flags (only works alone)
            /// </summary>
            None = 0x0000,
            /// <summary>
            /// Removes the browse button
            /// </summary>
            NoBrowse = 0x0001,
            /// <summary>
            /// No default item selected
            /// </summary>
            NoDefault = 0x0002,
            /// <summary>
            /// Calculates the working directory from the file name
            /// </summary>
            CalcDirectory = 0x0004,
            // CalcDirectory = 0x0003,
            /// <summary>
            /// Removes the edit box label
            /// </summary>
            NoLabel = 0x0008,
            /// <summary>
            /// Removes the seperate memory space checkbox (Windows NT only)
            /// </summary>
            NoSeperateMemory = 0x0020
        }
        #endregion

    }
}
