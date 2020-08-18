using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;

namespace CairoDesktop.SupportingClasses
{
    class FolderHelper
    {
        public static bool OpenLocation(string path)
        {
            if (Settings.Instance.EnableDynamicDesktop && Settings.Instance.FoldersOpenDesktopOverlay && DesktopManager.IsEnabled && !path.StartsWith("::{"))
            {
                try
                {
                    DesktopManager.Instance.NavigationManager.NavigateTo(path);
                    DesktopManager.Instance.IsOverlayOpen = true;
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
            DesktopManager.Instance.IsOverlayOpen = false;

            path = Environment.ExpandEnvironmentVariables(path);

            return Shell.StartProcess(Environment.ExpandEnvironmentVariables(Settings.Instance.FileManager), "\"" + path + "\"");
        }
    }
}