using System.Windows.Threading;
using System.Windows.Input;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using CairoDesktop.Common;
using CairoDesktop.SupportingClasses;
using CairoDesktop.Interop;

namespace CairoDesktop
{
    public static class CustomCommands
    {
        private static readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        static CustomCommands()
        {            
        }

        public static RoutedUICommand OpenSearchResult { get; } = new RoutedUICommand("OpenSearchResult", "OpenSearchResult", typeof(CustomCommands));

        public static void PerformAction(string verb, string fileName)
        {
            switch (verb)
            {
                case "open":
                    DesktopManager.Instance.IsOverlayOpen = false;

                    Shell.StartProcess(fileName);

                    return;
                case "openwith":
                    DesktopManager.Instance.IsOverlayOpen = false;

                    Shell.ShowOpenWithDialog(fileName);

                    return;
                case "delete":
                    string displayName = Shell.GetDisplayName(fileName);
                    bool? deleteChoice = CairoMessage.ShowOkCancel(string.Format(Localization.DisplayString.sDesktop_DeleteInfo, displayName), Localization.DisplayString.sDesktop_DeleteTitle, "Resources/cairoIcon.png", Localization.DisplayString.sInterface_Delete, Localization.DisplayString.sInterface_Cancel);
                    if (deleteChoice.HasValue && deleteChoice.Value)
                    {
                        Shell.SendToRecycleBin(fileName);
                    }
                    return;
                case "properties":
                    Shell.ShowFileProperties(fileName);
                    DesktopManager.Instance.IsOverlayOpen = false;

                    return;
                case "copy":
                    StringCollection scPath = new StringCollection();
                    scPath.Add(fileName);
                    System.Windows.Forms.Clipboard.SetFileDropList(scPath);
                    return;
                case "addStack":
                    StacksManager.Instance.AddLocation(fileName);
                    return;
                case "removeStack":
                    StacksManager.Instance.StackLocations.Remove(new SystemDirectory(fileName, _dispatcher));
                    return;
                case "openWithShell":
                    FolderHelper.OpenWithShell(fileName);
                    break;
                case "personalize" when Shell.IsCairoRunningAsShell:
                    CairoSettingsWindow.Instance.Show();
                    CairoSettingsWindow.Instance.Activate();
                    CairoSettingsWindow.Instance.TabDesktop.IsSelected = true;
                    break;
                case "personalize":
                    Shell.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,2");
                    break;
                case "displaySettings":
                    Shell.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,3");
                    break;
                default:
                    Shell.StartProcess(fileName, "", verb);
                    break;
            }
        }

        public static void PerformDirectoryAction(string action, SystemDirectory directory, Window window)
        {
            switch (action)
            {
                case "new":
                    window.Activate();

                    // watch for new file to be created so we can perform an action
                    directory.FileCreated += ShellNew_FileCreated;
                    break;
                case "paste":
                    directory.PasteFromClipboard();
                    break;
            }
        }

        private static void ShellNew_FileCreated(object sender, FileSystemEventArgs e)
        {
            // file was created due to usage of the shell new menu
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (sender is SystemFile file)
                {
                    // remove this handler
                    file.ParentDirectory.FileCreated -= ShellNew_FileCreated;

                    // TODO: New shortcut wizard
                    /*if (Path.GetExtension(file.FullName) == ".lnk")
                    {
                        // new shortcut
                        CairoMessage.Show("Yay!", "New Shortcut");
                    }
                    else*/
                    {
                        file.RequestInteractiveRename(file);
                    }
                }
            });
        }
    }
}
