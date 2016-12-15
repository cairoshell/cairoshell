using System;
using System.Windows;
using System.Windows.Markup;

namespace CairoDesktop
{
    public partial class TaskListButton
    {
        public TaskListButton()
        {
            this.InitializeComponent();
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            var windowObject = this.DataContext as CairoDesktop.WindowsTasks.ApplicationWindow;
            if (windowObject != null)
            {
                if (windowObject.State == CairoDesktop.WindowsTasks.ApplicationWindow.WindowState.Active)
                {
                    windowObject.Minimize();
                    windowObject.State = CairoDesktop.WindowsTasks.ApplicationWindow.WindowState.Inactive;
                }
                else
                {
                    windowObject.BringToFront();
                }
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            var windowObject = this.DataContext as CairoDesktop.WindowsTasks.ApplicationWindow;
            if (windowObject != null)
            {
                windowObject.Minimize();
                windowObject.State = CairoDesktop.WindowsTasks.ApplicationWindow.WindowState.Inactive;
            }
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            var windowObject = this.DataContext as CairoDesktop.WindowsTasks.ApplicationWindow;
            if (windowObject != null)
            {
                windowObject.BringToFront();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var windowObject = this.DataContext as CairoDesktop.WindowsTasks.ApplicationWindow;
            if (windowObject != null)
            {
                IntPtr handle = WindowsTasks.NativeWindowEx.FindWindow(null, WinTitle.Text);

                Interop.Shell.SendMessageTimeout(handle, WindowsTasks.WindowsTasksService.WM_COMMAND, WindowsTasks.WindowsTasksService.WM_CLOSE, 0, 2, 200, ref handle);
            }
        }
    }
}