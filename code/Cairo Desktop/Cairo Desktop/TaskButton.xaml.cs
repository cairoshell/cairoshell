using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
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

        public const int WM_COMMAND = 0x0112;
        public const int WM_CLOSE = 0xF060; 
		public TaskButton()
		{
			this.InitializeComponent();
			// Insert code required on object creation below this point.
            // Sets the Theme for Cairo
            string theme = Properties.Settings.Default.CairoTheme;
            if (theme != "Cairo.xaml")
            {
                ResourceDictionary CairoDictionary = (ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + theme));
                this.Resources.MergedDictionaries[0] = CairoDictionary;
            }
            
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
                int handle = FindWindow(null, WinTitle.Text);

                SendMessage(handle, WM_COMMAND, WM_CLOSE, 0);
            }
        }
	}
}