using System;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using Microsoft.Extensions.DependencyInjection;

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
                    var desktopManager = CairoApplication.Current.Host.Services.GetService<DesktopManager>();

                    desktopManager.NavigationManager.NavigateTo(path);
                    desktopManager.IsOverlayOpen = true;

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
            var desktopManager = CairoApplication.Current.Host.Services.GetService<DesktopManager>();

            desktopManager.IsOverlayOpen = false;

            var args = Environment.ExpandEnvironmentVariables(path);
            var filename = Environment.ExpandEnvironmentVariables(Settings.Instance.FileManager);

            return Shell.StartProcess(filename, $@"""{args}""");
        }
    }
}