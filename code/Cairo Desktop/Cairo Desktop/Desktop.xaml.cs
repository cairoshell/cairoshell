using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Interop;
using CairoDesktop.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System.Windows.Markup;
using System.IO;

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
            if (msg == WindowsTasks.NativeWindowEx.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(WindowsTasks.NativeWindowEx.MA_NOACTIVATE);
            }
            else if (msg == WindowsTasks.NativeWindowEx.WM_WINDOWPOSCHANGING)
            {
                handled = true;
                return new IntPtr(WindowsTasks.NativeWindowEx.MA_NOACTIVATE);
            }

            return IntPtr.Zero;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            int result = NativeMethods.SetShellWindow(helper.Handle);
            Shell.ShowWindowBottomMost(helper.Handle);

            if (Properties.Settings.Default.EnableDesktop && Icons == null)
            {
                Icons = new DesktopIcons();
                grid.Children.Add(Icons);

                if (Properties.Settings.Default.EnableDynamicDesktop)
                {
                    DesktopNavigationToolbar nav = new DesktopNavigationToolbar() { Owner = this };
                    nav.Show();
                }
            }
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
