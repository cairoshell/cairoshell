﻿using CairoDesktop.Common;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.ShellFolders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CairoDesktop.MenuBar
{
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
            StacksManager.Instance.RemoveLocation((sender as MenuItem).CommandParameter as ShellFolder);
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
                // ensure the quick preview uses the scroller (so sorting is applied) by wiring the scroller early
                try
                {
                    if (sender is ICommandSource cmdsrc && cmdsrc.CommandParameter is ShellFolder)
                    {
                        // the template creates an empty MenuItem as the submenu placeholder; replace its Header with the scroller
                        if (cmdsrc is MenuItem menuItem && menuItem.Items.Count > 0 && menuItem.Items[0] is MenuItem subMenuItem)
                        {
                            if (!(subMenuItem.Header is StacksScroller))
                            {
                                if (menuItem.DataContext is ShellFolder sf)
                                {
                                    sf.Refresh(); // ensure contents are up-to-date
                                    StacksScroller scroller = new StacksScroller(sf, this, menuItem, MenuBar._commandService);
                                    subMenuItem.Header = scroller;
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void NameLabel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Open folders with middle button clicks.
            if (e.ChangedButton == MouseButton.Middle)
            {
                // Some checks just in case.
                ICommandSource cmdsrc = sender as ICommandSource;
                if (cmdsrc?.CommandParameter is ShellFolder cmdparam)
                {
                    OpenDir(cmdparam.Path, !Settings.Instance.FoldersOpenDesktopOverlay);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Launches the FileManager specified in the application Settings object to the specified directory.
        /// </summary>
        /// <param name="directoryPath">Directory to open.</param>
        /// <param name="alwaysOpenWithShell">If true, user preferences will not be honored and the shell will always be used to open the directory.</param>
        internal void OpenDir(string directoryPath, bool alwaysOpenWithShell)
        {
            if (!MenuBar._commandService.InvokeCommand(alwaysOpenWithShell ? "OpenLocationInWindow" : "OpenLocation", ("Path", directoryPath)))
            {
                CairoMessage.Show(Common.Localization.DisplayString.sError_FileNotFoundInfo, Common.Localization.DisplayString.sError_OhNo, MessageBoxButton.OK, CairoMessageImage.Error);
            }
        }

        #region Drag and drop

        private Point? startPoint;
        private bool ctxOpen;
        private bool inMove;

        private void locationDisplay_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else if (e.Data.GetDataPresent(typeof(ShellFolder)))
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
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) && !e.Data.GetDataPresent(typeof(ShellFolder)))
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
            else if (e.Data.GetDataPresent(typeof(ShellFolder)))
            {
                ShellFolder dropData = e.Data.GetData(typeof(ShellFolder)) as ShellFolder;

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

                if (mousePos.Y <= ActualHeight && ((Point)startPoint).Y <= ActualHeight && e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    Menu menu = sender as Menu;
                    ShellFolder selectedDir = menu.DataContext as ShellFolder;

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
            ShellFolder replacedDir = dropContainer.DataContext as ShellFolder;

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
            else if (e.Data.GetDataPresent(typeof(ShellFolder)))
            {
                ShellFolder dropData = e.Data.GetData(typeof(ShellFolder)) as ShellFolder;

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
            // TODO: When this becomes an extension, use injected IDesktopManager
            var desktopManager = MenuBar?._host.Services.GetService<IDesktopManager>();

            ContextMenu menu = (sender as ContextMenu);
            foreach (Control item in menu.Items)
            {
                if (item.Name == "miOpenOnDesktop")
                {
                    item.Visibility = (!Settings.Instance.EnableDynamicDesktop || desktopManager?.IsEnabled != true) ?
                        Visibility.Collapsed : Visibility.Visible;
                    return;
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
                    if (!(subMenuItem.Header is StacksScroller))
                    {
                        try
                        {
                            if (menuItem.DataContext is ShellFolder sf)
                            {
                                sf.Refresh(); // ensure contents are up-to-date
                                StacksScroller scroller = new StacksScroller(sf, this, menuItem, MenuBar._commandService);
                                subMenuItem.Header = scroller;
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
