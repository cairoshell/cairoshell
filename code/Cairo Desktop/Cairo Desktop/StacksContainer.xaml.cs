using System;
using System.Windows;
using System.Windows.Controls;
using CairoDesktop.Configuration;
using CairoDesktop.Common;
using CairoDesktop.SupportingClasses;
using System.Windows.Input;

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
            else if (e.Data.GetDataPresent(typeof(SystemDirectory)))
            {
                SystemDirectory dropData = e.Data.GetData(typeof(SystemDirectory)) as SystemDirectory;

                int initialIndex = StacksManager.StackLocations.IndexOf(dropData);
                StacksManager.StackLocations.Move(initialIndex, StacksManager.StackLocations.Count - 1);
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

        #region Drag and drop reordering

        private Point? startPoint = null;

        private void StackMenu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
        }

        private void StackMenu_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (startPoint != null)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = (Point)startPoint - mousePos;

                if (mousePos.Y <= this.ActualHeight && ((Point)startPoint).Y <= this.ActualHeight && e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    Menu menu = sender as Menu;
                    SystemDirectory selectedDir = menu.DataContext as SystemDirectory;
                    
                    DragDrop.DoDragDrop(menu, selectedDir, DragDropEffects.Move);

                    // reset the stored mouse position
                    startPoint = null;
                }
            }
        }

        private void StackMenu_Drop(object sender, DragEventArgs e)
        {
            Menu dropContainer = sender as Menu;
            SystemDirectory replacedDir = dropContainer.DataContext as SystemDirectory;

            String[] fileNames = e.Data.GetData(DataFormats.FileDrop) as String[];
            if (fileNames != null)
            {
                foreach (String fileName in fileNames)
                {
                    if (StacksManager.AddLocation(fileName))
                    {
                        int dropIndex = StacksManager.StackLocations.IndexOf(replacedDir);
                        StacksManager.StackLocations.Move(StacksManager.StackLocations.Count - 1, dropIndex);
                    }
                }
            }
            else if (e.Data.GetDataPresent(typeof(SystemDirectory)))
            {
                SystemDirectory dropData = e.Data.GetData(typeof(SystemDirectory)) as SystemDirectory;

                int initialIndex = StacksManager.StackLocations.IndexOf(dropData);
                int dropIndex = StacksManager.StackLocations.IndexOf(replacedDir);
                StacksManager.StackLocations.Move(initialIndex, dropIndex);
            }

            e.Handled = true;
        }

        private void StackMenu_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // reset the stored mouse position
            startPoint = null;
        }

        #endregion
    }
}
