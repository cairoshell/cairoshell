using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Globalization;
using System.Collections.Specialized;

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

            if (verb == "open")
            {
                Interop.Shell.StartProcess(fileName);
                return;
            }
            else if (verb == "openwith")
            {
                Interop.Shell.ShowOpenWithDialog(fileName);
                return;
            }
            else if (verb == "delete")
            {
                bool? deleteChoice = CairoMessage.ShowOkCancel("\"" + Path.GetFileName(fileName) + "\" will be sent to the Recycle Bin.", "Are you sure you want to delete this?", "Resources/cairoIcon.png", "Delete", "Cancel");
                if (deleteChoice.HasValue && deleteChoice.Value)
                {
                    Interop.Shell.SendToRecycleBin(fileName);
                }
                return;
            }
            else if (verb == "properties")
            {
                Interop.Shell.ShowFileProperties(fileName);
                return;
            }
            else if (verb == "copy")
            {
                StringCollection scPath = new StringCollection();
                scPath.Add(fileName);
                System.Windows.Forms.Clipboard.SetFileDropList(scPath);
                return;
            }

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Verb = verb;
            try
            {
                proc.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Error running the {0} verb on {1}. ({2})", item.Header, fileName, ex.Message));
            }
        }

        public static void Icon_ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = sender as ContextMenu;

            if (menu.Items.Count < 1)
            {
                string filePath = menu.Tag as string;

                menu.Items.Add(new MenuItem { Header = "Open", Tag = "open|" + filePath });

                if ((File.GetAttributes(filePath) & FileAttributes.Directory) != FileAttributes.Directory)
                    menu.Items.Add(new MenuItem { Header = "Open With...", Tag = "openwith|" + filePath });

                foreach (string verb in ((menu.PlacementTarget as Button).DataContext as SystemFile).Verbs)
                {
                    if (verb.ToLower() != "open")
                        menu.Items.Add(new MenuItem { Header = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(verb), Tag = verb + "|" + filePath });
                }

                menu.Items.Add(new Separator());

                menu.Items.Add(new MenuItem { Header = "Copy", Tag = "copy|" + filePath });

                menu.Items.Add(new Separator());

                menu.Items.Add(new MenuItem { Header = "Delete", Tag = "delete|" + filePath });
                menu.Items.Add(new MenuItem { Header = "Properties", Tag = "properties|" + filePath });
            }
        }
    }
}
