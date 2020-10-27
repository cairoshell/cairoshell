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
        public MenuBar MenuBar;

        public static DependencyProperty PopupWidthProperty = DependencyProperty.Register("PopupWidth", typeof(double), typeof(StacksContainer), new PropertyMetadata(new double()));
        public double PopupWidth
        {
            get { return (double)GetValue(PopupWidthProperty); }
            set { SetValue(PopupWidthProperty, value); }
        }

        public StacksContainer() 
        {
            InitializeComponent();

            DataContext = StacksManager.Instance;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            StacksManager.Instance.RemoveLocation((sender as MenuItem).CommandParameter as SystemDirectory);
        }
        
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenDir((sender as ICommandSource).CommandParameter.ToString(), true);
        }

        private void OpenDesktop_Click(object sender, RoutedEventArgs e)
        {
            OpenDir((sender as ICommandSource).CommandParameter.ToString(), false);
        }

        private void NameLabel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // set popup width in case the display changed
            if (e.ChangedButton == MouseButton.Left && MenuBar != null)
            {
                PopupWidth = MenuBar.Width;
            }
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
                    OpenDir(cmdparam.FullName, false); 
                    e.Handled = true;
                }
            }
        }
        
        /// <summary>
        /// Launches the FileManager specified in the application Settings object to the specified directory.
        /// </summary>
        /// <param name="directoryPath">Directory to open.</param>
        internal void OpenDir(string directoryPath, bool openWithShell) 
        {
            if ((!openWithShell && !FolderHelper.OpenLocation(directoryPath)) || (openWithShell && !FolderHelper.OpenWithShell(directoryPath)))
            {
                CairoMessage.Show(Localization.DisplayString.sError_FileNotFoundInfo, Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
            }
        }

        #region Drag and drop

        private Point? startPoint = null;
        private bool ctxOpen = false;
        private bool inMove = false;

        private void locationDisplay_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else if (e.Data.GetDataPresent(typeof(SystemDirectory)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void locationDisplay_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) && !e.Data.GetDataPresent(typeof(SystemDirectory)))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // check to see if there at least one path we can add
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
                bool canAdd = false;
                if (fileNames != null)
                {
                    foreach (string fileName in fileNames)
                    {
                        if (StacksManager.Instance.CanAdd(fileName))
                        {
                            canAdd = true;
                            break;
                        }
                    }
                }

                if (!canAdd)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
        }

        private void locationDisplay_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNames != null)
            {
                foreach (string fileName in fileNames)
                {
                    StacksManager.Instance.AddLocation(fileName);
                }
            }
            else if (e.Data.GetDataPresent(typeof(SystemDirectory)))
            {
                SystemDirectory dropData = e.Data.GetData(typeof(SystemDirectory)) as SystemDirectory;

                int initialIndex = StacksManager.Instance.StackLocations.IndexOf(dropData);
                StacksManager.Instance.StackLocations.Move(initialIndex, StacksManager.Instance.StackLocations.Count - 1);
            }

            e.Handled = true;
        }

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

            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNames != null)
            {
                foreach (string fileName in fileNames)
                {
                    if (StacksManager.Instance.AddLocation(fileName))
                    {
                        int dropIndex = StacksManager.Instance.StackLocations.IndexOf(replacedDir);
                        StacksManager.Instance.StackLocations.Move(StacksManager.Instance.StackLocations.Count - 1, dropIndex);
                    }
                }
            }
            else if (e.Data.GetDataPresent(typeof(SystemDirectory)))
            {
                SystemDirectory dropData = e.Data.GetData(typeof(SystemDirectory)) as SystemDirectory;

                int initialIndex = StacksManager.Instance.StackLocations.IndexOf(dropData);
                int dropIndex = StacksManager.Instance.StackLocations.IndexOf(replacedDir);
                StacksManager.Instance.StackLocations.Move(initialIndex, dropIndex);
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

            if (!Settings.Instance.EnableDynamicDesktop || !DesktopManager.IsEnabled)
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

        private void MenuItem_OnSubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.Items.Count > 0 && menuItem.Items[0] is MenuItem subMenuItem)
                {
                    StacksScroller scroller = new StacksScroller()
                    {
                        DataContext = menuItem.DataContext,
                        ParentContainer = this
                    };
                    subMenuItem.Header = scroller;
                }
            }
        }
    }
}
