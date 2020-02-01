using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CairoDesktop.AppGrabber;

namespace CairoDesktop
{
	public partial class QuickLaunchButton
	{
        static AppGrabber.AppGrabber appGrabber = AppGrabber.AppGrabber.Instance;

		public QuickLaunchButton()
		{
			this.InitializeComponent();

            switch (Configuration.Settings.TaskbarIconSize)
            {
                case 0:
                    imgIcon.Width = 32;
                    imgIcon.Height = 32;
                    break;
                case 10:
                    imgIcon.Width = 24;
                    imgIcon.Height = 24;
                    break;
                default:
                    imgIcon.Width = 16;
                    imgIcon.Height = 16;
                    break;
            }
        }

        private void LaunchProgram(object sender, RoutedEventArgs e)
        {
            Button item = (Button)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgram(app);
        }

        private void LaunchProgramMenu(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgram(app);
        }

        private void LaunchProgramAdmin(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.LaunchProgramAdmin(app);
        }

        private void programsMenu_Remove(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            appGrabber.RemoveAppConfirm(app);
        }

        private void programsMenu_Properties(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ApplicationInfo app = item.DataContext as ApplicationInfo;

            AppGrabber.AppGrabber.ShowAppProperties(app);
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
                int dropIndex = appGrabber.QuickLaunch.IndexOf(replacedApp);
                appGrabber.InsertByPath(fileNames, dropIndex, AppCategoryType.QuickLaunch);
            }
            else if (e.Data.GetDataPresent(typeof(ApplicationInfo)))
            {
                ApplicationInfo dropData = e.Data.GetData(typeof(ApplicationInfo)) as ApplicationInfo;

                int initialIndex = appGrabber.QuickLaunch.IndexOf(dropData);
                int dropIndex = appGrabber.QuickLaunch.IndexOf(replacedApp);
                appGrabber.QuickLaunch.Move(initialIndex, dropIndex);
                appGrabber.Save();
            }

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
    }
}