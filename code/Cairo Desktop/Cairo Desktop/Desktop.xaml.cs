using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.IO;
using System.Windows.Threading;
//using System.Windows.Shapes;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Desktop.xaml
    /// </summary>
    public partial class Desktop : Window
    {
        public Desktop()
        {
            InitializeComponent();
            if (Properties.Settings.Default.EnableDynamicDesktop)
            {
                this.DesktopNavToolbar.IsOpen = true;
                this.DesktopNavToolbar.StaysOpen = true;
                this.DesktopAddressToolbar.IsOpen = true;
                this.DesktopAddressToolbar.StaysOpen = true;
            }
            else
            {
                this.DesktopNavToolbar.IsOpen = false;
                this.DesktopNavToolbar.StaysOpen = false;
                this.DesktopAddressToolbar.IsOpen = false;
                this.DesktopAddressToolbar.StaysOpen = false;
            }
        }

        public string oldPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public string oldPath2 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public string oldPath3 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            SystemDirectory desktopSysDir = new SystemDirectory(oldPath, Dispatcher.CurrentDispatcher);
            SystemDirectory oldSysDir = new SystemDirectory(oldPath2, Dispatcher.CurrentDispatcher);
            Locations.Remove(desktopSysDir as SystemDirectory);
            Locations.Add(oldSysDir);
            oldPath = oldPath2;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            SystemDirectory oldSysDir = new SystemDirectory(oldPath, Dispatcher.CurrentDispatcher);
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (Directory.Exists(desktopPath))
            {
                SystemDirectory desktopSysDir = new SystemDirectory(desktopPath, Dispatcher.CurrentDispatcher);
                Locations.Remove(oldSysDir as SystemDirectory);
                Locations.Add(desktopSysDir);
                oldPath = desktopPath;
            }
        }

        private void Fwd_Click(object sender, RoutedEventArgs e)
        {
            SystemDirectory desktopSysDir = new SystemDirectory(oldPath, Dispatcher.CurrentDispatcher);
            SystemDirectory oldSysDir = new SystemDirectory(oldPath3, Dispatcher.CurrentDispatcher);
            Locations.Remove(desktopSysDir as SystemDirectory);
            Locations.Add(oldSysDir);
            oldPath = oldPath3;
        }
        
        private void GoChangeDesktopAddress(object sender, RoutedEventArgs e)
        {
            SystemDirectory oldSysDir = new SystemDirectory(oldPath, Dispatcher.CurrentDispatcher);
            oldPath2 = oldPath;
            oldPath3 = oldPath;
            string desktopPath = DesktopLocAddress.Text;
            if (Directory.Exists(desktopPath))
            {
                SystemDirectory desktopSysDir = new SystemDirectory(desktopPath, Dispatcher.CurrentDispatcher);
                Locations.Remove(oldSysDir as SystemDirectory);
                Locations.Add(desktopSysDir);
                oldPath = desktopPath;
            }
        }

        public InvokingObservableCollection<SystemDirectory> Locations
        {
            get
            {
                return GetValue(DesktopIcons.locationsProperty) as InvokingObservableCollection<SystemDirectory>;
            }
            set
            {
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke((Action)(() => this.Locations = value), null);
                    return;
                }

                SetValue(DesktopIcons.locationsProperty, value);
            }
        }
    }
}
