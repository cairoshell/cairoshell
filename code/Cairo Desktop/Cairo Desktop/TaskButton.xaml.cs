using System;
using System.Windows;
using System.Windows.Markup;
using System.Runtime.InteropServices;

namespace CairoDesktop
{
	public partial class TaskButton
	{
        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        public WindowsTasks.ApplicationWindow Window;

        public const int WM_COMMAND = 0x0112;
        public const int WM_CLOSE = 0xF060; 
		public TaskButton()
		{
			this.InitializeComponent();
            // Set custom theme if selected
            string theme = Properties.Settings.Default.CairoTheme;
            if (theme != "Default")
                if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + theme)) this.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + theme)));

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
                int handle = FindWindow(null, WinTitle.Text);

                SendMessage(handle, WM_COMMAND, WM_CLOSE, 0);
            }
        }
	}
}