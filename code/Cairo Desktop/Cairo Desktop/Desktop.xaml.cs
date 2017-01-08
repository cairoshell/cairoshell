using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Interop;
using CairoDesktop.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;
using CairoDesktop.SupportingClasses;
using CairoDesktop.Configuration;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Desktop.xaml
    /// </summary>
    public partial class Desktop : Window
    {
        public Stack<string> PathHistory = new Stack<string>();
        private WindowInteropHelper helper;
        public DesktopIcons Icons;
        public Desktop()
        {
            InitializeComponent();

            this.Width = AppBarHelper.PrimaryMonitorSize.Width;
            this.Height = AppBarHelper.PrimaryMonitorSize.Height;

            if (Startup.IsCairoUserShell)
            {
                // draw wallpaper
                ImageBrush bgBrush = new ImageBrush();
                bgBrush.ImageSource = new BitmapImage(new Uri((string)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "Wallpaper", ""), UriKind.Absolute));

                this.Background = bgBrush;
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

        private void Window_Activated(object sender, EventArgs e)
        {
            int result = NativeMethods.SetShellWindow(helper.Handle);
            Shell.ShowWindowBottomMost(helper.Handle);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // show the windows desktop
            Interop.Shell.ToggleDesktopIcons(true);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.Top = 0;

            helper = new WindowInteropHelper(this);

            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            if (Settings.EnableDesktop && Icons == null)
            {
                Icons = new DesktopIcons();
                grid.Children.Add(Icons);

                if (Settings.EnableDynamicDesktop)
                {
                    DesktopNavigationToolbar nav = new DesktopNavigationToolbar() { Owner = this };
                    nav.Show();
                }
            }

            DispatcherTimer autoResize = new DispatcherTimer(new TimeSpan(0, 0, 5), DispatcherPriority.Normal, delegate
            {
                this.Top = 0;
                this.Left = 0;
                this.Width = AppBarHelper.PrimaryMonitorSize.Width;
                this.Height = AppBarHelper.PrimaryMonitorSize.Height;
            }, this.Dispatcher);
        }

        private void pasteFromClipboard()
        {
            IDataObject clipFiles = Clipboard.GetDataObject();

            if(clipFiles.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])clipFiles.GetData(DataFormats.FileDrop);

                foreach(string file in files)
                {
                    if(Shell.Exists(file))
                    {
                        try
                        {
                            FileAttributes attr = File.GetAttributes(file);
                            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                FileSystem.CopyDirectory(file, Icons.Locations[0].FullName + "\\" + new DirectoryInfo(file).Name, UIOption.AllDialogs);
                            else
                                FileSystem.CopyFile(file, Icons.Locations[0].FullName + "\\" + Path.GetFileName(file), UIOption.AllDialogs);
                        }
                        catch { }
                    }
                }
            }
        }

        private void miPaste_Click(object sender, RoutedEventArgs e)
        {
            pasteFromClipboard();
        }

        private void miPersonalization_Click(object sender, RoutedEventArgs e)
        {
            // doesn't work because Settings app requires Explorer :(
            try
            {
                Shell.StartProcess("desk.cpl");
            }
            catch { }
        }
    }
}
