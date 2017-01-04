using System;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Interop;
using CairoDesktop.Interop;
using CairoDesktop.Configuration;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopNavigationToolbar.xaml
    /// </summary>
    public partial class DesktopNavigationToolbar : Window
    {
        private WindowInteropHelper helper;
        public DesktopNavigationToolbar()
        {
            InitializeComponent();

            setLocation();
        }

        private void setLocation()
        {
            this.Top = SystemParameters.WorkArea.Bottom - this.Height - 150;
            this.Left = (SystemParameters.WorkArea.Width / 2) - (this.Width / 2);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo parent = (this.Owner as Desktop).Icons.Locations[0].DirectoryInfo.Parent;
            if (parent != null)
            {
                (this.Owner as Desktop).PathHistory.Push((this.Owner as Desktop).Icons.Locations[0].FullName);
                (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory(parent.FullName, Dispatcher.CurrentDispatcher);
            }

        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            (this.Owner as Desktop).PathHistory.Push((this.Owner as Desktop).Icons.Locations[0].FullName);

            string defaultDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string userDesktopPath = Settings.DesktopDirectory;

            if (Directory.Exists(userDesktopPath))
            {
                (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory(userDesktopPath, Dispatcher.CurrentDispatcher);
            }
            else if (Directory.Exists(defaultDesktopPath))
            {
                (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory(defaultDesktopPath, Dispatcher.CurrentDispatcher);
            }
        }

        private void Fwd_Click(object sender, RoutedEventArgs e)
        {
            if ((this.Owner as Desktop).PathHistory.Count > 0)
                (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory((this.Owner as Desktop).PathHistory.Pop(), Dispatcher.CurrentDispatcher);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select a folder to display as your desktop:";
            fbd.ShowNewFolderButton = false;
            fbd.SelectedPath = (this.Owner as Desktop).Icons.Locations[0].FullName;

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo dir = new DirectoryInfo(fbd.SelectedPath);
                if (dir != null)
                {
                    (this.Owner as Desktop).PathHistory.Push((this.Owner as Desktop).Icons.Locations[0].FullName);
                    (this.Owner as Desktop).Icons.Locations[0] = new SystemDirectory(dir.FullName, Dispatcher.CurrentDispatcher);
                }
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }
            else if (msg == NativeMethods.WM_WINDOWPOSCHANGING)
            {
                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }

            return IntPtr.Zero;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            helper = new WindowInteropHelper(this);

            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            DispatcherTimer autoResize = new DispatcherTimer(new TimeSpan(0, 0, 2), DispatcherPriority.Normal, delegate
            {
                setLocation();
            }, this.Dispatcher);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            //Set the window style to noactivate.
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE) | NativeMethods.WS_EX_NOACTIVATE);
        }
    }
}
