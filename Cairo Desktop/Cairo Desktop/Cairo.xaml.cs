using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Localization;
using CairoDesktop.SupportingClasses;
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
            Button senderButton = sender as Button;
            if (senderButton != null && senderButton.CommandParameter != null)
            {
                string commandString = senderButton.CommandParameter as String;
                if (!string.IsNullOrWhiteSpace(commandString))
                {
                    // Determine if [SHIFT] key is held. Bypass Directory Processing, which will use the Shell to open the item.
                    if (!KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                    {
                        // get the file attributes for file or directory
                        FileAttributes attr = File.GetAttributes(commandString);
                        bool isDirectory = (attr & FileAttributes.Directory) == FileAttributes.Directory;

                        // if directory, perform special handling
                        if (isDirectory
                            && Settings.EnableDynamicDesktop
                            && Window.GetWindow(senderButton)?.Name == "CairoDesktopWindow"
                            && Startup.DesktopWindow != null)
                        {
                            Startup.DesktopWindow.Navigate(commandString);
                            return;
                        }
                        else if (isDirectory)
                        {
                            FolderHelper.OpenLocation(commandString);
                            return;
                        }
                    }

                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.FileName = commandString;

                    if (Startup.DesktopWindow != null)
                        Startup.DesktopWindow.IsOverlayOpen = false;

                    try
                    {
                        proc.Start();
                        return;
                    }
                    catch
                    {
                        // No 'Open' command associated with this filetype in the registry
                        Interop.Shell.ShowOpenWithDialog(proc.StartInfo.FileName);
                        return;
                    }
                }
            }

            CairoMessage.Show(DisplayString.sError_FileNotFoundInfo, DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
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
