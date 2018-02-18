using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;

namespace CairoDesktop.SupportingClasses
{
    class FolderHelper
    {
        public static bool OpenLocation(string path)
        {
            if (Settings.FoldersOpenDesktopOverlay && Startup.DesktopWindow != null && !path.StartsWith("::{"))
            {
                try
                {
                    Startup.DesktopWindow.Navigate(path);
                    Startup.DesktopWindow.IsOverlayOpen = true;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else 
            {
                if (Startup.DesktopWindow != null)
                    Startup.DesktopWindow.IsOverlayOpen = false;

                return Shell.StartProcess(Environment.ExpandEnvironmentVariables(Settings.FileManager), path);
            }
        }
    }
}
