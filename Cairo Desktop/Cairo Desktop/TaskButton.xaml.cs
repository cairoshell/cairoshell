using CairoDesktop.AppGrabber;
using CairoDesktop.Interop;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CairoDesktop
{
    public partial class TaskButton
    {

        private WindowsTasks.ApplicationWindow _applicationWindow;
        public WindowsTasks.ApplicationWindow Window
        {
            get => _applicationWindow;
            set
            {
                _applicationWindow = value;
                InitializeApplicationInfo();
            }
        }

        private bool hasInitializedApplicationInfoYet = false;
        private ApplicationInfo _applicationInfo;
        public ApplicationInfo App
        {
            get
            {
                if (!hasInitializedApplicationInfoYet)
                    InitializeApplicationInfo();


                return _applicationInfo;
            }

            set => _applicationInfo = value;
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

            //if (ShouldShowMiPin())
            //{
            //    miPin.Visibility = Visibility.Visible;
            //    miPinSeperator.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    miPin.Visibility = Visibility.Hidden;
            //    miPinSeperator.Visibility = Visibility.Hidden;
            //}

        }

        private bool ShouldShowMiPin()
        {
            bool result = true;

            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
                if (!object.ReferenceEquals(App, null))
                    if (AppGrabber.AppGrabber.Instance.QuickLaunch.Contains(App))
                        result = false;

            return result;
        }

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        private static int GetWindowProcessID(IntPtr hwnd)
        {
            GetWindowThreadProcessId(hwnd, out int pid);
            return pid;
        }

        private void miPin_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                if (!object.ReferenceEquals(App, null))
                {
                    AppGrabber.AppGrabber.Instance.AddToQuickLaunch(App);
                }
            }
        }

        private ApplicationInfo GetAppGrabberApplicationInfoFromFilePath(string filePath)
        {
            ApplicationInfo result = null;

            var apps = AppGrabber.AppGrabber.Instance.ProgramList;
            foreach (ApplicationInfo app in apps)
            {
                var suppliedPath = System.IO.Path.GetFullPath(filePath).TrimEnd('\\');

                var applicationInfoPath = string.Empty;
                if (!string.IsNullOrWhiteSpace(app.Target))
                    applicationInfoPath = System.IO.Path.GetFullPath(app.Target).TrimEnd('\\');


                if (string.Compare(suppliedPath, applicationInfoPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    result = app;
                    break;
                }
            }

            return result;
        }

        private void InitializeApplicationInfo()
        {
            var Window = (DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                int pid = GetWindowProcessID(Window.Handle);
                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);

                string file = string.Empty;
                using (System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher($"SELECT ExecutablePath FROM Win32_Process WHERE ProcessId = {p.Id}"))
                    foreach (System.Management.ManagementObject mo in mos.Get())
                        file = mo["ExecutablePath"] as string;

                _applicationInfo = GetAppGrabberApplicationInfoFromFilePath(file);

                hasInitializedApplicationInfoYet = true;
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