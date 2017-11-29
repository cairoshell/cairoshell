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
using CairoDesktop.SupportingClasses;
using CairoDesktop.Configuration;
using CairoDesktop.Common;
using System.Windows.Threading;

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
            this.Height = AppBarHelper.PrimaryMonitorSize.Height-1;

            if (Settings.DesktopLabelPosition == 1)
            {
                grid.Margin = new Thickness(0, 35, 0, 0);
            }

                if (Startup.IsCairoUserShell)
            {
                string regWallpaper = (string)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "Wallpaper", "");

                if (regWallpaper != string.Empty && Shell.Exists(regWallpaper))
                {
                    // draw wallpaper
                    try
                    {
                        ImageBrush bgBrush = new ImageBrush();
                        bgBrush.ImageSource = new BitmapImage(new Uri(regWallpaper, UriKind.Absolute));

                        this.Background = bgBrush;
                    } catch { }
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
                /*// Extract the WINDOWPOS structure corresponding to this message
                NativeMethods.WINDOWPOS wndPos = NativeMethods.WINDOWPOS.FromMessage(lParam);

                // Determine if the z-order is changing (absence of SWP_NOZORDER flag)
                if (!((wndPos.flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER) == NativeMethods.SetWindowPosFlags.SWP_NOZORDER))
                {
                    // add the SWP_NOZORDER flag
                    wndPos.flags = wndPos.flags | NativeMethods.SetWindowPosFlags.SWP_NOZORDER;
                    wndPos.UpdateMessage(lParam);
                }*/

                handled = true;
                return new IntPtr(NativeMethods.MA_NOACTIVATE);
            }
            else if (msg == NativeMethods.WM_DISPLAYCHANGE)
            {
                setPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void setPosition(uint x, uint y)
        {
            this.Top = 0;
            this.Left = 0;
            this.Width = x;
            this.Height = y;
        }

        public void ResetPosition()
        {
            this.Top = 0;
            this.Left = 0;
            this.Width = AppBarHelper.PrimaryMonitorSize.Width;
            this.Height = AppBarHelper.PrimaryMonitorSize.Height - 1;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            int result = NativeMethods.SetShellWindow(helper.Handle);
            Shell.ShowWindowBottomMost(helper.Handle);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Startup.IsShuttingDown)
            {
                // show the windows desktop
                Shell.ToggleDesktopIcons(true);
            }
            else
                e.Cancel = true;
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
                    try
                    {
                        DesktopNavigationToolbar nav = new DesktopNavigationToolbar() { Owner = this };
                        nav.Show();
                    }
                    catch { }
                }
            }
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
            if (!Shell.StartProcess("desk.cpl"))
            {
                CairoMessage.Show("Unable to open the Display control panel.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void grid_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NativeMethods.SetForegroundWindow(helper.Handle);
        }

        public void Navigate(string newLocation)
        {
            PathHistory.Push(Icons.Locations[0].FullName);
            Icons.Locations[0] = new SystemDirectory(newLocation, Dispatcher.CurrentDispatcher);
        }

        private void CairoDesktopWindow_LocationChanged(object sender, EventArgs e)
        {
            ResetPosition();
        }
    }
}
