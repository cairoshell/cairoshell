using System;
using System.IO;
//using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System.Diagnostics;
//using System.Windows.Navigation;
using System.Windows.Markup;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Taskbar.xaml
    /// </summary>
    public partial class Taskbar : Window
    {
        //This file is from before r416
        public AppGrabber.AppGrabber appGrabber;
        //private int appbarMessageId = -1;

        private String configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoAppConfig.xml";

        public Taskbar()
        {
            InitializeComponent();
            // Set custom theme if selected
            string theme = Properties.Settings.Default.CairoTheme;
            if (theme != "Default")
                this.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + theme)));

            appGrabber = AppGrabber.AppGrabber.Instance;
            this.quickLaunchList.ItemsSource = appGrabber.QuickLaunch;
            this.TaskbarBorder.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth - 36;
            // Dodgy - set to top of task bar.
            this.Top = SystemParameters.WorkArea.Bottom - this.Height;
        }

        private void Taskbar_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Manually call dispose on window close...
            (TasksList.DataContext as WindowsTasks.WindowsTasksService).Dispose();
            (TasksList2.DataContext as WindowsTasks.WindowsTasksService).Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.CairoTaskbarTaskList.IsOpen == false)
            {
                this.CairoTaskbarTaskList.IsOpen = true;
                this.CairoTaskbarTaskList.StaysOpen = false;
            }
            else
            {
                this.CairoTaskbarTaskList.IsOpen = false;
                this.CairoTaskbarTaskList.StaysOpen = false;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // Dodgy - set to top of task bar.
            this.Top = SystemParameters.WorkArea.Bottom - this.Height;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            source.AddHook(WndProc);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(MA_NOACTIVATE);
            }
            else
            {
                return IntPtr.Zero;
            }
        }
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATE = 0x0003;
    }
}
