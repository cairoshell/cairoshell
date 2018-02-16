using CairoDesktop.Common;
using CairoDesktop.Configuration;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop
{
    partial class Cairo : ResourceDictionary
    {

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                Button senderButton = sender as Button;
                if (senderButton != null && senderButton.CommandParameter != null)
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.FileName = senderButton.CommandParameter as String;
                    try
                    {
                        // get the file attributes for file or directory
                        FileAttributes attr = File.GetAttributes(senderButton.CommandParameter as String);

                        //detect whether its a directory or file
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory && Settings.EnableDynamicDesktop && Window.GetWindow(senderButton) != null && Window.GetWindow(senderButton).Name == "CairoDesktopWindow")
                        {
                            Startup.DesktopWindow.Navigate(senderButton.CommandParameter as String);
                        }
                        else
                        {
                            proc.Start();

                            Startup.DesktopWindow.IsOverlayOpen = false;
                        }

                        return;
                    }
                    catch
                    {
                        // No 'Open' command associated with this filetype in the registry
                        Interop.Shell.ShowOpenWithDialog(proc.StartInfo.FileName);

                        Startup.DesktopWindow.IsOverlayOpen = false;

                        return;
                    }
                }
            }

            CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
        }

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
            {
                if (peer is Border)
                {
                    peer.Visibility = Visibility.Visible;
                }
            }
            box.Visibility = Visibility.Collapsed;
        }

        private void txtRename_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void btnAppGrabber_Click(object sender, RoutedEventArgs e)
        {
            Startup.MenuBarWindow.ProgramsMenu.IsSubmenuOpen = false;
            Startup.MenuBarWindow.appGrabber.ShowDialog();
        }

        private void btnUninstallApps_Click(object sender, RoutedEventArgs e)
        {
            if (!Interop.Shell.StartProcess("appwiz.cpl"))
            {
                CairoMessage.Show(Localization.DisplayString.sError_CantOpenAppWiz, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
