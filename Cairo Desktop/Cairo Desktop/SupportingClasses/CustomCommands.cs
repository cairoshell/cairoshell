using System.Windows.Threading;
using System.Windows.Input;
using System.Collections.Specialized;
using CairoDesktop.Common;
using CairoDesktop.SupportingClasses;
using CairoDesktop.Interop;

namespace CairoDesktop
{
    public static class CustomCommands
    {
        private static Dispatcher _dispatcher;
        private static RoutedUICommand openSearchResultCommand = new RoutedUICommand("OpenSearchResult", "OpenSearchResult", typeof(CustomCommands));


        static CustomCommands()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public static RoutedUICommand OpenSearchResult
        {
            get
            {
                return openSearchResultCommand;
            }
        }

        public static void PerformAction(string verb, string fileName)
        {
            if (verb == "open")
            {
                if (WindowManager.Instance.DesktopWindow != null)
                    WindowManager.Instance.DesktopWindow.IsOverlayOpen = false;

                Shell.StartProcess(fileName);

                return;
            }
            else if (verb == "openwith")
            {
                if (WindowManager.Instance.DesktopWindow != null)
                    WindowManager.Instance.DesktopWindow.IsOverlayOpen = false;

                Shell.ShowOpenWithDialog(fileName);

                return;
            }
            else if (verb == "delete")
            {
                string displayName = Shell.GetDisplayName(fileName);
                bool? deleteChoice = CairoMessage.ShowOkCancel(string.Format(Localization.DisplayString.sDesktop_DeleteInfo, displayName), Localization.DisplayString.sDesktop_DeleteTitle, "Resources/cairoIcon.png", Localization.DisplayString.sInterface_Delete, Localization.DisplayString.sInterface_Cancel);
                if (deleteChoice.HasValue && deleteChoice.Value)
                {
                    Shell.SendToRecycleBin(fileName);
                }
                return;
            }
            else if (verb == "properties")
            {
                Shell.ShowFileProperties(fileName);
                WindowManager.Instance.DesktopWindow.IsOverlayOpen = false;

                return;
            }
            else if (verb == "copy")
            {
                StringCollection scPath = new StringCollection();
                scPath.Add(fileName);
                System.Windows.Forms.Clipboard.SetFileDropList(scPath);
                return;
            }
            else if (verb == "addStack")
            {
                StacksManager.Instance.AddLocation(fileName);
                return;
            }
            else if (verb == "removeStack")
            {
                StacksManager.Instance.StackLocations.Remove(new SystemDirectory(fileName, _dispatcher));
                return;
            }
            else if (verb == "openWithShell")
            {
                FolderHelper.OpenWithShell(fileName);
            }
            else if (verb == "personalize")
            {
                if (Shell.IsCairoRunningAsShell)
                {
                    CairoSettingsWindow.Instance.Show();
                    CairoSettingsWindow.Instance.Activate();
                    CairoSettingsWindow.Instance.TabDesktop.IsSelected = true;
                }
                else
                {
                    Shell.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,2");
                }
            }
            else if (verb == "displaySettings")
            {
                Shell.StartProcess("Rundll32.exe", "shell32.dll,Control_RunDLL desk.cpl,,3");
            }
            else
            {
                Shell.StartProcess(fileName, "", verb);
            }
        }
    }
}
