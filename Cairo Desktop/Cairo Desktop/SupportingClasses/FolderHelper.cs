using System;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;

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

            var args = Environment.ExpandEnvironmentVariables(path);
            var filename = Environment.ExpandEnvironmentVariables(Settings.Instance.FileManager);

            return Shell.StartProcess(filename, $@"""{args}""");
        }
    }
}