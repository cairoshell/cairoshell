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
using System.Collections.Generic;
using CairoDesktop.SupportingClasses;
using System.Windows.Interop;
using System.Windows.Forms;
//using System.Windows.Shapes;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Desktop.xaml
    /// </summary>
    public partial class Desktop : Window
    {
        Stack<string> pathHistory = new Stack<string>();
        public Desktop()
        {
            InitializeComponent();
            if (Properties.Settings.Default.EnableDynamicDesktop)
            {
                this.DesktopNavToolbar.IsOpen = true;
                this.DesktopNavToolbar.StaysOpen = true;
            }
            else
            {
                this.DesktopNavToolbar.IsOpen = false;
                this.DesktopNavToolbar.StaysOpen = false;
            }
        }

        #region sorry for commenting this out
        //public string oldPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //public string oldPath2 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //public string oldPath3 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //private void Back_Click(object sender, RoutedEventArgs e)
        //{
        //    SystemDirectory desktopSysDir = new SystemDirectory(oldPath, Dispatcher.CurrentDispatcher);
        //    SystemDirectory oldSysDir = new SystemDirectory(oldPath2, Dispatcher.CurrentDispatcher);
        //    Locations.Remove(desktopSysDir as SystemDirectory);
        //    Locations.Add(oldSysDir);
        //    oldPath = oldPath2;
        //}

        //private void Home_Click(object sender, RoutedEventArgs e)
        //{
        //    SystemDirectory oldSysDir = new SystemDirectory(oldPath, Dispatcher.CurrentDispatcher);
        //    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        //    if (Directory.Exists(desktopPath))
        //    {
        //        SystemDirectory desktopSysDir = new SystemDirectory(desktopPath, Dispatcher.CurrentDispatcher);
        //        Locations.Remove(oldSysDir as SystemDirectory);
        //        Locations.Add(desktopSysDir);
        //        oldPath = desktopPath;
        //    }
        //}

        //private void Fwd_Click(object sender, RoutedEventArgs e)
        //{
        //    SystemDirectory desktopSysDir = new SystemDirectory(oldPath, Dispatcher.CurrentDispatcher);
        //    SystemDirectory oldSysDir = new SystemDirectory(oldPath3, Dispatcher.CurrentDispatcher);
        //    Locations.Remove(desktopSysDir as SystemDirectory);
        //    Locations.Add(oldSysDir);
        //    oldPath = oldPath3;
        //}



        //public InvokingObservableCollection<SystemDirectory> Locations
        //{
        //    get
        //    {
        //        return GetValue(DesktopIcons.locationsProperty) as InvokingObservableCollection<SystemDirectory>;
        //    }
        //    set
        //    {
        //        if (!this.Dispatcher.CheckAccess())
        //        {
        //            this.Dispatcher.Invoke((Action)(() => this.Locations = value), null);
        //            return;
        //        }

        //        SetValue(DesktopIcons.locationsProperty, value);
        //    }
        //}
        #endregion

        private void GoChangeDesktopAddress(object sender, RoutedEventArgs e)
        {
            //SystemDirectory oldSysDir = new SystemDirectory(oldPath, Dispatcher.CurrentDispatcher);
            //oldPath2 = oldPath;
            //oldPath3 = oldPath;
            string desktopPath = "";
            if (Directory.Exists(desktopPath))
            {
                //SystemDirectory desktopSysDir = new SystemDirectory(desktopPath, Dispatcher.CurrentDispatcher);
                //Locations.Remove(oldSysDir as SystemDirectory);
                //Locations.Add(desktopSysDir);
                //oldPath = desktopPath;

                DirectoryInfo dir = new DirectoryInfo(desktopPath);
                if (dir != null)
                {
                    pathHistory.Push(DesktopIcons.Locations[0].DirectoryInfo.FullName);
                    DesktopIcons.Locations[0] = new SystemDirectory(dir.FullName, Dispatcher.CurrentDispatcher);
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //CairoMessage.Show("This will go back.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);
            DirectoryInfo parent = DesktopIcons.Locations[0].DirectoryInfo.Parent;
            if (parent != null)
            {
                pathHistory.Push(DesktopIcons.Locations[0].DirectoryInfo.FullName);
                DesktopIcons.Locations[0] = new SystemDirectory(parent.FullName, Dispatcher.CurrentDispatcher);
            }

        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            //CairoMessage.Show("This will go to %USERPROFILE%\\Desktop.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);

            pathHistory.Push(DesktopIcons.Locations[0].DirectoryInfo.FullName);
            DesktopIcons.Locations[0] = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Dispatcher.CurrentDispatcher);

        }

        private void Fwd_Click(object sender, RoutedEventArgs e)
        {
            //CairoMessage.Show("This will go forward.", "Cairo Desktop", MessageBoxButton.OK, MessageBoxImage.Information);
            if (pathHistory.Count > 0)
                DesktopIcons.Locations[0] = new SystemDirectory(pathHistory.Pop(), Dispatcher.CurrentDispatcher);
        }


        private void ShowWindowBottomMost_Internal(IntPtr handle)
        {
            NativeMethods.SetWindowPos(
                handle,
                (IntPtr)NativeMethods.HWND_BOTTOMMOST,
                0,
                0,
                0,
                0,
                NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
        }

        public void ShowWindowBottomMost(IntPtr handle)
        {
            this.ShowWindowBottomMost_Internal((new WindowInteropHelper(this)).Handle);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            WindowInteropHelper f = new WindowInteropHelper(this);
            this.ShowWindowBottomMost(f.Handle);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            fbd.SelectedPath = DesktopIcons.Locations[0].DirectoryInfo.FullName;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo dir = new DirectoryInfo(fbd.SelectedPath);
                if (dir != null)
                {
                    pathHistory.Push(DesktopIcons.Locations[0].DirectoryInfo.FullName);
                    DesktopIcons.Locations[0] = new SystemDirectory(dir.FullName, Dispatcher.CurrentDispatcher);
                }
            }
        }
    }
}
