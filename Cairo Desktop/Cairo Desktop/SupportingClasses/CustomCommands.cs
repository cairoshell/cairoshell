using System.Windows.Threading;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.SupportingClasses;
using ManagedShell.Common.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CairoDesktop
{
    public static class CustomCommands
    {
        private static readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        static CustomCommands()
        {
        }

        public static void PerformAction(string verb, string fileName)
        {
             var desktopManager = CairoApplication.Current.Host.Services.GetService<DesktopManager>();
             var settingsUiService = CairoApplication.Current.Host.Services.GetService<ISettingsUIService>();

            switch (verb)
            {
                case Actions.Open:
                    desktopManager.IsOverlayOpen = false;
                    ShellHelper.StartProcess(fileName);
                    return;
                case Actions.OpenWith:
                    desktopManager.IsOverlayOpen = false;
                    ShellHelper.ShowOpenWithDialog(fileName);
                    return;
                case Actions.Delete:
                    string displayName = ShellHelper.GetDisplayName(fileName);
                    CairoMessage.ShowOkCancel(string.Format(Localization.DisplayString.sDesktop_DeleteInfo, displayName), 
                        Localization.DisplayString.sDesktop_DeleteTitle, CairoMessageImage.Warning, 
                        Localization.DisplayString.sInterface_Delete, Localization.DisplayString.sInterface_Cancel,
                        result =>
                        {
                            if (result == true)
                            {
                                ShellHelper.SendToRecycleBin(fileName);
                            }
                        });
                    return;
                case Actions.Properties:
                    ShellHelper.ShowFileProperties(fileName);
                    desktopManager.IsOverlayOpen = false;
                    return;
                case Actions.Copy:
                    StringCollection scPath = new StringCollection();
                    scPath.Add(fileName);
                    System.Windows.Forms.Clipboard.SetFileDropList(scPath);
                    return;
                case DirectoryActions.AddStack:
                    StacksManager.Instance.AddLocation(fileName);
                    return;
                case DirectoryActions.RemoveStack:
                    StacksManager.Instance.StackLocations.Remove(new SystemDirectory(fileName, _dispatcher));
                    return;
                case Actions.OpenWithShell:
                    FolderHelper.OpenWithShell(fileName);
                    break;
                case Actions.Personalize when EnvironmentHelper.IsAppRunningAsShell:
                    settingsUiService?.Show("desktop");
                    break;
                case Actions.Personalize:
                    ShellHelper.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,2");
                    break;
                case Actions.DisplaySettings:
                    ShellHelper.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,3");
                    break;
                default:
                    ShellHelper.StartProcess(fileName, "", verb);
                    break;
            }
        }

        public static void PerformDirectoryAction(string action, SystemDirectory directory, Window window)
        {
            switch (action)
            {
                case DirectoryActions.New:
                    window.Activate();
                    // watch for new file to be created so we can perform an action
                    directory.FileCreated += ShellNew_FileCreated;
                    break;
                case DirectoryActions.Paste:
                    directory.PasteFromClipboard();
                    break;
            }
        }

        private static void ShellNew_FileCreated(object sender, FileSystemEventArgs e)
        {
            // file was created due to usage of the shell new menu
            CairoApplication.Current.Dispatcher.Invoke(() =>
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

        public sealed class Actions
        {
            private Actions() { }

            public const string Open = "open";

            public const string OpenWith = "openwith";

            public const string Delete = "delete";

            public const string Properties = "properties";

            public const string Copy = "copy";

            public const string Cut = "cut";

            public const string Link = "link";

            public const string OpenWithShell = "openWithShell";

            public const string Personalize = "personalize";

            public const string DisplaySettings = "displaySettings";

            public const string Rename = "rename";
        }

        public sealed class DirectoryActions
        {
            private DirectoryActions() { }

            public const string AddStack = "addStack";

            public const string RemoveStack = "removeStack";

            public const string OpenFolder = "openFolder";

            public const string New = "new";

            public const string Paste = "paste";
        }
    }
}
