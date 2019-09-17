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
        private bool ctxOpen = false;
        private bool inMove = false;

        private void btn_Drop(object sender, DragEventArgs e)
        {
            Button dropContainer = sender as Button;
            ApplicationInfo replacedApp = dropContainer.DataContext as ApplicationInfo;

            String[] fileNames = e.Data.GetData(DataFormats.FileDrop) as String[];
            if (fileNames != null)
            {
                foreach (String fileName in fileNames)
                {
                    appGrabber.AddByPath(fileNames, AppCategoryType.QuickLaunch);
                    int dropIndex = appGrabber.QuickLaunch.IndexOf(replacedApp);

                    ApplicationInfo addedApp = appGrabber.QuickLaunch[appGrabber.QuickLaunch.Count - 1];
                    appGrabber.QuickLaunch.Move(appGrabber.QuickLaunch.Count - 1, dropIndex);
                    appGrabber.Save();
                }
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

        private void btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!ctxOpen)
            {
                // Store the mouse position
                startPoint = e.GetPosition(null);
            }
        }

        private void btn_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!inMove && startPoint != null && !ctxOpen)
            {
                inMove = true;

                Point mousePos = e.GetPosition(null);
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

                inMove = false;
            }

            e.Handled = true;
        }

        private void btn_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // reset the stored mouse position
            startPoint = null;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            startPoint = null;
            ctxOpen = true;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            ctxOpen = false;
        }

        private void btn_DragEnter(object sender, DragEventArgs e)
        {
            String[] formats = e.Data.GetFormats(true);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }

            e.Handled = true;
        }

        #endregion
    }
}