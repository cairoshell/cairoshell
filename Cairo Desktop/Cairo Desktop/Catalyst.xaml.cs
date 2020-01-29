using CairoDesktop.Common;
using CairoDesktop.Localization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System;

namespace CairoDesktop
{
    partial class Cairo : ResourceDictionary
    {

        private void miVerb_Click(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_MenuItem_Click(sender, e);
        }

        private void ctxFile_Loaded(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_ContextMenu_Loaded(sender, e);
        }

        private void txtRename_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            string orig = ((Button)((DockPanel)box.Parent).Parent).CommandParameter as string;

            if (!SystemFile.RenameFile(orig, box.Text))
                box.Text = Path.GetFileName(orig);

            foreach (UIElement peer in (box.Parent as DockPanel).Children)
                if (peer is Border)
                    peer.Visibility = Visibility.Visible;

            box.Visibility = Visibility.Collapsed;
        }

        private void txtRename_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                (sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

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
