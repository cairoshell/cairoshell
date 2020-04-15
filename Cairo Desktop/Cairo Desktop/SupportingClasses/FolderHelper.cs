using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;

namespace CairoDesktop.SupportingClasses
{
    class FolderHelper
    {
        public static bool OpenLocation(string path)
        {
            if (Settings.Instance.EnableDynamicDesktop && Settings.Instance.FoldersOpenDesktopOverlay && WindowManager.Instance.DesktopWindow != null && !path.StartsWith("::{"))
            {
                try
                {
                    WindowManager.Instance.DesktopWindow.NavigationManager.NavigateTo(path);
                    WindowManager.Instance.DesktopWindow.IsOverlayOpen = true;
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
            if (WindowManager.Instance.DesktopWindow != null)
                WindowManager.Instance.DesktopWindow.IsOverlayOpen = false;

            path = Environment.ExpandEnvironmentVariables(path);

            return Shell.StartProcess(Environment.ExpandEnvironmentVariables(Settings.Instance.FileManager), "\"" + path + "\"");
        }
    }
}