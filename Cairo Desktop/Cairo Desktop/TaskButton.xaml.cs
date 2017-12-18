using CairoDesktop.Interop;
using System;
using System.Windows;

namespace CairoDesktop
{
	public partial class TaskButton
	{
        public WindowsTasks.ApplicationWindow Window;

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
                IntPtr handle = NativeMethods.FindWindow(null, WinTitle.Text);

                // if the window exists, send close, otherwise remove from the collection
                if (handle != IntPtr.Zero)
                    NativeMethods.SendMessageTimeout(handle, WindowsTasks.WindowsTasksService.WM_COMMAND, WindowsTasks.WindowsTasksService.WM_CLOSE, 0, 2, 200, ref handle);
                else
                    Window.TasksService.Windows.Remove(Window);
            }
        }

        private void miTaskMan_Click(object sender, RoutedEventArgs e)
        {
            Shell.StartProcess("taskmgr.exe");
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