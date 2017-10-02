using CairoDesktop.Interop;
using System;
using System.Windows;

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
            var windowObject = this.DataContext as WindowsTasks.ApplicationWindow;
            if (windowObject != null)
            {
                if (windowObject.State == WindowsTasks.ApplicationWindow.WindowState.Active)
                {
                    windowObject.Minimize();
                    windowObject.State = WindowsTasks.ApplicationWindow.WindowState.Inactive;
                }
                else
                {
                    windowObject.BringToFront();
                }
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            var windowObject = this.DataContext as WindowsTasks.ApplicationWindow;
            if (windowObject != null)
            {
                windowObject.Minimize();
                windowObject.State = WindowsTasks.ApplicationWindow.WindowState.Inactive;
            }
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            var windowObject = this.DataContext as WindowsTasks.ApplicationWindow;
            if (windowObject != null)
            {
                windowObject.BringToFront();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                IntPtr handle = NativeMethods.FindWindow(null, WinTitle.Text);

                // if the window exists, send close, otherwise remove from the collection
                if (handle != IntPtr.Zero)
                    NativeMethods.SendMessageTimeout(handle, WindowsTasks.WindowsTasksService.WM_COMMAND, WindowsTasks.WindowsTasksService.WM_CLOSE, 0, 2, 200, ref handle);
                else
                    Window.TasksService.Windows.Remove(Window);
            }
        }
    }
}