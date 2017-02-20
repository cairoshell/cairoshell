using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using CairoDesktop.Configuration;
using CairoDesktop.Common;
using CairoDesktop.SupportingClasses;

namespace CairoDesktop {
    /// <summary>
    /// Interaction logic for StacksContainer.xaml
    /// </summary>
    public partial class StacksContainer : UserControl 
    {
        public StacksContainer() 
        {
            InitializeComponent();
        }

        private void locationDisplay_DragEnter(object sender, DragEventArgs e)
        {
            String[] formats = e.Data.GetFormats(true);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }

            e.Handled = true;
        }

        private void locationDisplay_Drop(object sender, DragEventArgs e)
        {
            String[] fileNames = e.Data.GetData(DataFormats.FileDrop) as String[];
            if (fileNames != null) 
            {
                foreach (String fileName in fileNames)
                {
                    StacksManager.AddLocation(fileName);
                }
            }

            e.Handled = true;
        }

        private void File_Button_Click(object sender, RoutedEventArgs e)
        {
            Button senderButton = sender as Button;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = senderButton.CommandParameter as String;
            try 
            {
                proc.Start();
            } 
            catch 
            {
                // No 'Open' command associated with this filetype in the registry
                Interop.Shell.ShowOpenWithDialog(proc.StartInfo.FileName);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_MenuItem_Click(sender, e);
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            CustomCommands.Icon_ContextMenu_Loaded(sender, e);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            StacksManager.StackLocations.Remove((sender as MenuItem).CommandParameter as SystemDirectory);
        }
        
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(sender.GetType().ToString());
            openDir((sender as MenuItem).CommandParameter.ToString());
        }
        
        /// <summary>
        /// Launches the FileManager specified in the application Settings object to the specified directory.
        /// </summary>
        /// <param name="directoryPath">Directory to open.</param>
        private void openDir(String directoryPath) 
        {
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = Environment.ExpandEnvironmentVariables(Settings.FileManager);
            prc.StartInfo.Arguments = directoryPath;
            prc.Start();
        }
    }
}
