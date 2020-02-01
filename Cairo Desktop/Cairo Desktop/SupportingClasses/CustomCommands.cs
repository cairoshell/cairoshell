using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Globalization;
using System.Collections.Specialized;
using CairoDesktop.Common;
using CairoDesktop.SupportingClasses;

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

        public static void Icon_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            string tag = item.Tag as string;
            string verb = tag.Substring(0, tag.IndexOf('|'));
            string fileName = tag.Substring(tag.IndexOf('|') + 1);

            PerformAction(verb, fileName, ((Button)((ContextMenu)item.Parent).PlacementTarget));
            
            if (Startup.DesktopWindow != null)
                Startup.DesktopWindow.IsOverlayOpen = false;
        }

        public static void PerformAction(string verb, string fileName)
        {
            if (verb == "open")
            {
                if (Startup.DesktopWindow != null)
                    Startup.DesktopWindow.IsOverlayOpen = false;

                Interop.Shell.StartProcess(fileName);

                return;
            }
            else if (verb == "openwith")
            {
                if (Startup.DesktopWindow != null)
                    Startup.DesktopWindow.IsOverlayOpen = false;

                Interop.Shell.ShowOpenWithDialog(fileName);

                return;
            }
            else if (verb == "delete")
            {
                string displayName = Interop.Shell.GetDisplayName(fileName);
                bool? deleteChoice = CairoMessage.ShowOkCancel(String.Format(Localization.DisplayString.sDesktop_DeleteInfo, displayName), Localization.DisplayString.sDesktop_DeleteTitle, "Resources/cairoIcon.png", Localization.DisplayString.sInterface_Delete, Localization.DisplayString.sInterface_Cancel);
                if (deleteChoice.HasValue && deleteChoice.Value)
                {
                    Interop.Shell.SendToRecycleBin(fileName);
                }
                return;
            }
            else if (verb == "properties")
            {
                Interop.Shell.ShowFileProperties(fileName);
                Startup.DesktopWindow.IsOverlayOpen = false;

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
                StacksManager.AddLocation(fileName);
                return;
            }
            else if (verb == "removeStack")
            {
                StacksManager.StackLocations.Remove(new SystemDirectory(fileName, _dispatcher));
                return;
            }
            else if (verb == "openWithShell")
            {
                FolderHelper.OpenWithShell(fileName);
            }
            else
            {
                Interop.Shell.StartProcess(fileName, "", verb);
            }
        }

        public static void PerformAction(string verb, string fileName, Button btn)
        {
            if (verb == "rename")
            {
                DockPanel parent = btn.Content as DockPanel;
                TextBox rename = parent.FindName("txtRename") as TextBox;
                Border label = parent.FindName("bdrFilename") as Border;

                rename.Visibility = Visibility.Visible;
                label.Visibility = Visibility.Collapsed;
                rename.Focus();
                rename.SelectAll();
                return;
            }
            else
            {
                PerformAction(verb, fileName);
            }
        }

        public static void Icon_ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;
            string filePath = menu.Tag as string;

            menu.Items.Clear();
            
            menu.Items.Add(new MenuItem { Header = Localization.DisplayString.sInterface_Open, Tag = "open|" + filePath });

            if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
                menu.Items.Add(new MenuItem { Header = Localization.DisplayString.sInterface_OpenWith, Tag = "openwith|" + filePath });
            else
                menu.Items.Add(new MenuItem { Header = Localization.DisplayString.sInterface_AddToStacks, Tag = "addStack|" + filePath });

            foreach (string verb in ((menu.PlacementTarget as Button).DataContext as SystemFile).Verbs)
            {
                if (verb.ToLower() != "open")
                    menu.Items.Add(new MenuItem { Header = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(verb), Tag = verb + "|" + filePath });
            }

            menu.Items.Add(new Separator());

            menu.Items.Add(new MenuItem { Header = Localization.DisplayString.sInterface_Copy, Tag = "copy|" + filePath });

            menu.Items.Add(new Separator());

            menu.Items.Add(new MenuItem { Header = Localization.DisplayString.sInterface_Delete, Tag = "delete|" + filePath });
            menu.Items.Add(new MenuItem { Header = Localization.DisplayString.sInterface_Rename, Tag = "rename|" + filePath });

            menu.Items.Add(new Separator());

            menu.Items.Add(new MenuItem { Header = Localization.DisplayString.sInterface_Properties, Tag = "properties|" + filePath });
        }
    }
}
