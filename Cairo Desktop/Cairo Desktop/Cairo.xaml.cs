using CairoDesktop.Common;
using CairoDesktop.Localization;
using System.Windows;

namespace CairoDesktop
{
    partial class Cairo : ResourceDictionary
    {
        private void btnAppGrabber_Click(object sender, RoutedEventArgs e)
        {
            Startup.MenuBarWindow.ProgramsMenu.IsSubmenuOpen = false;
            Startup.MenuBarWindow.appGrabber.ShowDialog();
        }

        private void btnUninstallApps_Click(object sender, RoutedEventArgs e)
        {
            if (!Interop.Shell.StartProcess("appwiz.cpl"))
                CairoMessage.Show(DisplayString.sError_CantOpenAppWiz, DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
