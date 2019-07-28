using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
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
            if (senderButton != null && senderButton.DataContext != null)
            {
                SystemFile file = senderButton.DataContext as SystemFile;
                if (!string.IsNullOrWhiteSpace(file.FullName))
                {
                    // Determine if [SHIFT] key is held. Bypass Directory Processing, which will use the Shell to open the item.
                    if (!KeyboardUtilities.IsKeyDown(System.Windows.Forms.Keys.ShiftKey))
                    {
                        // if directory, perform special handling
                        if (file.IsDirectory
                            && Settings.EnableDynamicDesktop
                            && Window.GetWindow(senderButton)?.Name == "CairoDesktopWindow"
                            && Startup.DesktopWindow != null)
                        {
                            Startup.DesktopWindow.Navigate(file.FullName);
                            return;
                        }
                        else if (file.IsDirectory)
                        {
                            FolderHelper.OpenLocation(file.FullName);
                            return;
                        }
                    }

                    if (Startup.DesktopWindow != null)
                        Startup.DesktopWindow.IsOverlayOpen = false;

                    Shell.ExecuteProcess(file.FullName);
                    return;
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

        public static void ShowShutdownConfirmation()
        {
            ShowActionConfirmation(DisplayString.sShutDown_Info
                , DisplayString.sShutDown_Title
                , "Resources/shutdownIcon.png"
                , DisplayString.sShutDown_ShutDown
                , DisplayString.sInterface_Cancel
                , NativeMethods.Shutdown);
        }

        public static void ShowRebootConfirmation()
        {
            ShowActionConfirmation(DisplayString.sRestart_Info
                , DisplayString.sRestart_Title
                , "Resources/restartIcon.png"
                , DisplayString.sRestart_Restart
                , DisplayString.sInterface_Cancel
                , NativeMethods.Reboot);
        }

        public static void ShowLogOffConfirmation()
        {
            ShowActionConfirmation(DisplayString.sLogoff_Info
                , DisplayString.sLogoff_Title
                , "Resources/logoffIcon.png"
                , DisplayString.sLogoff_Logoff
                , DisplayString.sInterface_Cancel
                , NativeMethods.Logoff);
        }

        private static void ShowActionConfirmation(string message, string title, string imageSource, string okButtonText, string cancelButtonText, Action systemAction)
        {
            bool? actionChoice = CairoMessage.ShowOkCancel(message, title, imageSource, okButtonText, cancelButtonText);
            if (actionChoice.HasValue && actionChoice.Value)
                systemAction();
        }
    }
}
