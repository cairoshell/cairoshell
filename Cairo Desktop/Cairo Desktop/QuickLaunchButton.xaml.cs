using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CairoDesktop.AppGrabber;
using CairoDesktop.Configuration;
using ManagedShell.Common.Helpers;

namespace CairoDesktop
{
	public partial class QuickLaunchButton
	{
        public static DependencyProperty ParentTaskbarProperty = DependencyProperty.Register("ParentTaskbar", typeof(Taskbar), typeof(QuickLaunchButton));
        public Taskbar ParentTaskbar
        {
            get { return (Taskbar)GetValue(ParentTaskbarProperty); }
            set { SetValue(ParentTaskbarProperty, value); }
        }

        public QuickLaunchButton()
		{
			InitializeComponent();

            setIconSize();

            // register for settings changes
            Settings.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName))
            {
                switch (e.PropertyName)
                {
                    case "TaskbarIconSize":
                        setIconSize();
                        break;
                }
            }
        }

        private void setIconSize()
        {
            imgIcon.Width = IconHelper.GetSize(Settings.Instance.TaskbarIconSize);
            imgIcon.Height = IconHelper.GetSize(Settings.Instance.TaskbarIconSize);
        }

        private void LaunchProgram(object sender, RoutedEventArgs e)
        {
            Button item = (Button)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            ParentTaskbar._appGrabber.LaunchProgram(app);
        }

        private void LaunchProgramMenu(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            ParentTaskbar._appGrabber.LaunchProgram(app);
        }

        private void LaunchProgramAdmin(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            ParentTaskbar._appGrabber.LaunchProgramAdmin(app);
        }

        private void programsMenu_Remove(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            ParentTaskbar._appGrabber.RemoveAppConfirm(app, null);
        }

        private void programsMenu_Rename(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            ParentTaskbar._appGrabber.RenameAppDialog(app, null);
        }

        private void programsMenu_Properties(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            ParentTaskbar._appGrabber.ShowAppProperties(app);
        }

        #region Drag and drop reordering

        private Point? startPoint = null;
        private bool inDrag = false;

        // receive drop functions
        private void btn_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else if (!e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void btn_Drop(object sender, DragEventArgs e)
        {
            Button dropContainer = sender as Button;
            ApplicationInfo replacedApp = dropContainer.DataContext as ApplicationInfo;

            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (fileNames != null)
            {
                int dropIndex = ParentTaskbar._appGrabber.QuickLaunch.IndexOf(replacedApp);
                ParentTaskbar._appGrabber.InsertByPath(fileNames, dropIndex, AppCategoryType.QuickLaunch);
            }
            else if (e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                ApplicationInfo dropData = e.Data.GetData(typeof(ApplicationInfo)) as ApplicationInfo;

                int initialIndex = ParentTaskbar._appGrabber.QuickLaunch.IndexOf(dropData);
                int dropIndex = ParentTaskbar._appGrabber.QuickLaunch.IndexOf(replacedApp);
                ParentTaskbar._appGrabber.QuickLaunch.Move(initialIndex, dropIndex);
                ParentTaskbar._appGrabber.Save();
            }

            setParentAutoHide(true);

            e.Handled = true;
        }

        // send drag functions
        private void btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(this);
        }

        private void btn_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!inDrag && startPoint != null)
            {
                inDrag = true;

                Point mousePos = e.GetPosition(this);
                Vector diff = (Point)startPoint - mousePos;

                if (mousePos.Y <= this.ActualHeight && ((Point)startPoint).Y <= this.ActualHeight && e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    Button button = sender as Button;
                    ApplicationInfo selectedApp = button.DataContext as ApplicationInfo;

                    try
                    {
                        DragDrop.DoDragDrop(button, selectedApp, DragDropEffects.Move);

                        setParentAutoHide(true);
                    }
                    catch { }

                    // reset the stored mouse position
                    startPoint = null;
                }
                else if (e.LeftButton != MouseButtonState.Pressed)
                {
                    // reset the stored mouse position
                    startPoint = null;
                }

                inDrag = false;
            }

            e.Handled = true;
        }

        #endregion

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            setParentAutoHide(true);
        }

        private void setParentAutoHide(bool enabled)
        {
            if (ParentTaskbar != null) ParentTaskbar.CanAutoHide = enabled;
        }
    }
}