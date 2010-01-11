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
        private int appbarMessageId = -1;

        private String configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CairoAppConfig.xml";

        public Taskbar()
        {
            InitializeComponent();
            if (Properties.Settings.Default.MenuBarWhite)
            {
                ResourceDictionary CairoDictionary = (ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + "CairoStyles_alt.xaml"));
                this.Resources.MergedDictionaries[0] = CairoDictionary;
            }
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
    }
}
