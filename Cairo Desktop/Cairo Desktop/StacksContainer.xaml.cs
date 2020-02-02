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

        private void locationDisplay_DragOver(object sender, DragEventArgs e)
        {
            String[] formats = e.Data.GetFormats(true);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else if (!e.Data.GetDataPresent(typeof(SystemDirectory)))
            {
                e.Effects = DragDropEffects.None;
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

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            StacksManager.StackLocations.Remove((sender as MenuItem).CommandParameter as SystemDirectory);
        }
        
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            openDir((sender as ICommandSource).CommandParameter.ToString(), true);
        }

        private void OpenDesktop_Click(object sender, RoutedEventArgs e)
        {
            openDir((sender as ICommandSource).CommandParameter.ToString(), false);
        }

        private void NameLabel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Open folders with middle button clicks.
            if (e.ChangedButton == MouseButton.Middle)
            {
                // Some checks just in case.
                ICommandSource cmdsrc = sender as ICommandSource;
                if (cmdsrc?.CommandParameter is SystemDirectory cmdparam)
                {
                    openDir(cmdparam.FullName, false); 
                    e.Handled = true;
                }
            }
        }
        
        /// <summary>
        /// Launches the FileManager specified in the application Settings object to the specified directory.
        /// </summary>
        /// <param name="directoryPath">Directory to open.</param>
        private void openDir(string directoryPath, bool openWithShell) 
        {
            if ((!openWithShell && !FolderHelper.OpenLocation(directoryPath)) || (openWithShell && !FolderHelper.OpenWithShell(directoryPath)))
            {
                CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Drag and drop reordering

        private Point? startPoint = null;
        private bool ctxOpen = false;
        private bool inMove = false;

        private void StackMenu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!ctxOpen)
            {
                // Store the mouse position
                startPoint = e.GetPosition(this);
            }
        }

        private void StackMenu_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!inMove && startPoint != null && !ctxOpen)
            {
                inMove = true;

                Point mousePos = e.GetPosition(this);
                Vector diff = (Point)startPoint - mousePos;

                if (mousePos.Y <= this.ActualHeight && ((Point)startPoint).Y <= this.ActualHeight && e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    Menu menu = sender as Menu;
                    SystemDirectory selectedDir = menu.DataContext as SystemDirectory;

                    try
                    {
                        DragDrop.DoDragDrop(menu, selectedDir, DragDropEffects.Move);
                    }
                    catch { }

                    // reset the stored mouse position
                    startPoint = null;

                    e.Handled = true;
                }

                inMove = false;
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

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            startPoint = null;
            ctxOpen = true;

            if (!Settings.EnableDynamicDesktop || !Settings.EnableDesktop)
            {
                ContextMenu menu = (sender as ContextMenu);
                foreach (Control item in menu.Items)
                {
                    if (item.Name == "miOpenOnDesktop")
                    {
                        item.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
            }
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            ctxOpen = false;
        }

        #endregion

        private void Scroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = System.Windows.Media.VisualTreeHelper.GetChild(sender as ListView, 0) as ScrollViewer;

            if (scrollViewer == null)
                return;

            if (e.Delta < 0)
                scrollViewer.LineRight();
            else
                scrollViewer.LineLeft();

            e.Handled = true;
        }
    }
}
