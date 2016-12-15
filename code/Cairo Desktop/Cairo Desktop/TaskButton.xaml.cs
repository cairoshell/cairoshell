using System;
using System.Windows;
using System.Windows.Markup;

namespace CairoDesktop
{
	public partial class TaskButton
	{
        public WindowsTasks.ApplicationWindow Window;
        
		public TaskButton()
		{
			this.InitializeComponent();
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as CairoDesktop.WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                if (Window.State == CairoDesktop.WindowsTasks.ApplicationWindow.WindowState.Active)
                {
                    Window.Minimize();
                    Window.State = CairoDesktop.WindowsTasks.ApplicationWindow.WindowState.Inactive;
                }
                else
                {
                    Window.BringToFront();
                }
            }
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as CairoDesktop.WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.Minimize();
                Window.State = CairoDesktop.WindowsTasks.ApplicationWindow.WindowState.Inactive;
            }
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as CairoDesktop.WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                Window.BringToFront();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var Window = (this.DataContext as CairoDesktop.WindowsTasks.ApplicationWindow);
            if (Window != null)
            {
                IntPtr handle = WindowsTasks.NativeWindowEx.FindWindow(null, WinTitle.Text);

                Interop.Shell.SendMessageTimeout(handle, WindowsTasks.WindowsTasksService.WM_COMMAND, WindowsTasks.WindowsTasksService.WM_CLOSE, 0, 2, 200, ref handle);
            }
        }
	}
}