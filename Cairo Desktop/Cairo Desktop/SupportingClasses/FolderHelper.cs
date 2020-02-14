using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;

namespace CairoDesktop.SupportingClasses
{
    class FolderHelper
    {
        public static bool OpenLocation(string path)
        {
            if (Settings.Instance.EnableDynamicDesktop && Settings.Instance.FoldersOpenDesktopOverlay && Startup.DesktopWindow != null && !path.StartsWith("::{"))
            {
                try
                {
                    Startup.DesktopWindow.NavigationManager.NavigateTo(path);
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
                return OpenWithShell(path);
            }
        }

        public static bool OpenWithShell(string path)
        {
            if (Startup.DesktopWindow != null)
                Startup.DesktopWindow.IsOverlayOpen = false;

            return Shell.StartProcess(Environment.ExpandEnvironmentVariables(Settings.Instance.FileManager), path);
        }
    }
}