using CairoDesktop.AppGrabber;
using CairoDesktop.Interop;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CairoDesktop
{
    public partial class TaskButton
    {
        private ApplicationInfo _quickLaunchAppInfo;
        public ApplicationInfo QuickLaunchAppInfo
        {
            get
            {
                if (_quickLaunchAppInfo == null)
                {
                    var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
                    if (Window != null)
                    {
                        foreach (ApplicationInfo ai in AppGrabber.AppGrabber.Instance.QuickLaunch)
                        {
                            if (ai.Target == Window.WinFileName || (Window.WinFileName.ToLower().Contains("applicationframehost.exe") && ai.Target == Window.AppUserModelID))
                            {
                                _quickLaunchAppInfo = ai;
                                break;
                            }
                            else if (Window.Title.ToLower().Contains(ai.Name.ToLower()))
                            {
                                _quickLaunchAppInfo = ai;
                            }
                        }
                    }
                }

                return _quickLaunchAppInfo;
            }
            set
            {
                _quickLaunchAppInfo = value;
            }
        }


        public static readonly DependencyProperty TextWidthProperty = DependencyProperty.Register("TextWidth", typeof(double), typeof(TaskButton), new PropertyMetadata(new double()));
        public double TextWidth
        {
            get { return (double)GetValue(TextWidthProperty); }
            set { SetValue(TextWidthProperty, value); }
        }


        public TaskButton()
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

        private void btnClick(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                if (Window.State == WindowsTasks.ApplicationWindow.WindowState.Active)
                {
                    Window.Minimize();
                    Window.State = WindowsTasks.ApplicationWindow.WindowState.Inactive;
                }
                else
                {
                    Window.BringToFront();
                }
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.Minimize();
                Window.State = WindowsTasks.ApplicationWindow.WindowState.Inactive;
            }
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.BringToFront();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.Close();
            }
        }

        /// <summary>
        /// Handler that decides weather or not to show the miPin menuitem and seperator if the AppGraber doesnt already have it pinned.
        /// This needs a little more work as the system works off the Executable path of the process, ssems to give some undesirable results in some situations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandlerForCMO(object sender, ContextMenuEventArgs e)
        {

            if (QuickLaunchAppInfo == null)
            {
                miPin.Visibility = Visibility.Visible;
                miPinSeperator.Visibility = Visibility.Visible;
            }
            else
            {
                miPin.Visibility = Visibility.Collapsed;
                miPinSeperator.Visibility = Visibility.Collapsed;
            }

        }

        private void miPin_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                if (Window.WinFileName.ToLower().Contains("applicationframehost.exe"))
                {
                    // store app, do special stuff
                    AppGrabber.AppGrabber.Instance.AddStoreApp(Window.AppUserModelID, AppCategoryType.QuickLaunch);
                }
                else
                {
                    AppGrabber.AppGrabber.Instance.AddByPath(new string[] { Window.WinFileName }, AppCategoryType.QuickLaunch);
                }
            }
        }

        private void miTaskMan_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartTaskManager();
        }

        private void btn_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Middle)
            {
                Shell.StartProcess((this.DataContext as WindowsTasks.ApplicationWindow).WinFileName);
            }
        }
    }
}