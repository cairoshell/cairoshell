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
using System.ComponentModel;
using System.Windows.Input;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for Desktop.xaml
    /// </summary>
    public partial class Desktop : Window, INotifyPropertyChanged
    {
        private WindowInteropHelper helper;
        private bool altF4Pressed;

        public Stack<string> PathHistory = new Stack<string>();
        public DesktopIcons Icons;
        public DependencyProperty IsOverlayOpenProperty = DependencyProperty.Register("IsOverlayOpen", typeof(bool), typeof(Desktop), new PropertyMetadata(new bool()));


        public bool IsOverlayOpen
        {
            get
            {
                return (bool)GetValue(IsOverlayOpenProperty);
            }
            set
            {
                SetValue(IsOverlayOpenProperty, value);

                if (value)
                    ShowOverlay();
                else
                    CloseOverlay();
            }
        }

        public Desktop()
        {
            InitializeComponent();

            Width = AppBarHelper.PrimaryMonitorSize.Width;
            Height = AppBarHelper.PrimaryMonitorSize.Height - 1;

            if (Startup.IsCairoUserShell)
            {
                sepPersonalization.Visibility = Visibility.Collapsed;
                miPersonalization.Visibility = Visibility.Collapsed;
            }

            setGridPosition();
            setBackground();
        }

        private void setBackground()
        {
            if (Startup.IsCairoUserShell)
            {
                // draw wallpaper
                string regWallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Wallpaper", "") as string;
                if (!string.IsNullOrWhiteSpace(regWallpaper) && Shell.Exists(regWallpaper))
                    TryAndEat(() => Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(regWallpaper, UriKind.Absolute)) });
            }
        }

        private void SetupPostInit()
        {
            Shell.HideWindowFromTasks(helper.Handle);

            if (Settings.EnableDesktopOverlayHotKey)
                HotKeyManager.RegisterHotKey(Settings.DesktopOverlayHotKey, OnShowDesktop);
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
            else if (msg == NativeMethods.WM_DISPLAYCHANGE && (Startup.IsCairoUserShell))
            {
                SetPosition(((uint)lParam & 0xffff), ((uint)lParam >> 16));
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void SetPosition(uint x, uint y)
        {
            Top = 0;
            Left = 0;

            Width = x;
            Height = y - 1;
            setGridPosition();
        }

        public void ResetPosition()
        {
            Top = 0;
            Left = 0;

            Width = AppBarHelper.PrimaryMonitorSize.Width;
            Height = AppBarHelper.PrimaryMonitorSize.Height - 1;
            setGridPosition();
        }

        private void setGridPosition()
        {
            grid.Width = AppBarHelper.PrimaryMonitorWorkArea.Width / Shell.DpiScale;
            grid.Height = AppBarHelper.PrimaryMonitorWorkArea.Height / Shell.DpiScale;
            grid.Margin = new Thickness(System.Windows.Forms.SystemInformation.WorkingArea.Left / Shell.DpiScale, System.Windows.Forms.SystemInformation.WorkingArea.Top / Shell.DpiScale, 0, 0);
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (!Topmost)
            {
                int result = NativeMethods.SetShellWindow(helper.Handle);
                Shell.ShowWindowBottomMost(helper.Handle);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Startup.IsShuttingDown) // show the windows desktop
                Shell.ToggleDesktopIcons(true);
            else if (altF4Pressed) // Show the Shutdown Confirmation Window
            {
                Cairo.ShowShutdownConfirmation();
                e.Cancel = true;
            }
            else // Eat it !!!
                e.Cancel = true;
        }


        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4)
            {
                altF4Pressed = true;
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Top = 0;
            helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(new HwndSourceHook(WndProc));

            if (Settings.EnableDesktop && Icons == null)
            {
                grid.Children.Add(Icons = new DesktopIcons());
                if (Settings.EnableDynamicDesktop)
                    TryAndEat(() =>
                         {
                             DesktopNavigationToolbar nav = new DesktopNavigationToolbar() { Owner = this };
                             nav.Show();
                         });
            }

            SetupPostInit();
        }

        private void PasteFromClipboard()
        {
            IDataObject clipFiles = Clipboard.GetDataObject();
            if (clipFiles.GetDataPresent(DataFormats.FileDrop))
                if (clipFiles.GetData(DataFormats.FileDrop) is string[] files)
                    foreach (string file in files)
                        if (Shell.Exists(file))
                            TryAndEat(() =>
                            {
                                FileAttributes attr = File.GetAttributes(file);
                                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                                    FileSystem.CopyDirectory(file, Icons.Locations[0].FullName + "\\" + new DirectoryInfo(file).Name, UIOption.AllDialogs);
                                else
                                    FileSystem.CopyFile(file, Icons.Locations[0].FullName + "\\" + Path.GetFileName(file), UIOption.AllDialogs);
                            });

        }


        private void miPaste_Click(object sender, RoutedEventArgs e)
        {
            PasteFromClipboard();
        }

        private void miPersonalization_Click(object sender, RoutedEventArgs e)
        {
            // doesn't work when shell because Settings app requires Explorer :(
            if (!Shell.StartProcess("desk.cpl"))
                CairoMessage.Show("Unable to open Personalization settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (IsOverlayOpen)
                IsOverlayOpen = false;
        }

        private void grid_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Topmost)
                NativeMethods.SetForegroundWindow(helper.Handle);
        }

        public void Navigate(string newLocation)
        {
            PathHistory.Push(Icons.Locations[0].FullName);
            Icons.Locations[0] = new SystemDirectory(newLocation, Dispatcher.CurrentDispatcher);
            OnPropertyChanged("CurrentDirectoryFriendly");
        }

        public string CurrentLocation
        {
            get
            {
                return Icons.Locations[0].FullName;
            }
            set
            {
                Icons.Locations[0] = new SystemDirectory(value, Dispatcher.CurrentDispatcher);
                OnPropertyChanged("CurrentDirectoryFriendly");
            }
        }

        private void CairoDesktopWindow_LocationChanged(object sender, EventArgs e)
        {
            ResetPosition();
        }

        private void OnShowDesktop(HotKey hotKey)
        {
            ToggleOverlay();
        }

        public void ToggleOverlay()
        {
            IsOverlayOpen = !IsOverlayOpen;
        }

        private void ShowOverlay()
        {
            Topmost = true;
            NativeMethods.SetForegroundWindow(helper.Handle);
            grid.Background = new SolidColorBrush(Color.FromArgb(0x88, 0, 0, 0));
            Background = null;
        }

        private void CloseOverlay()
        {
            Topmost = false;
            Shell.ShowWindowBottomMost(helper.Handle);
            grid.Background = Brushes.Transparent;
            setBackground();
        }

        private void grid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(System.Windows.Controls.ScrollViewer))
                IsOverlayOpen = false;
        }

        public string CurrentDirectoryFriendly
        {
            get
            {
                return Localization.DisplayString.sDesktop_CurrentFolder + " " + Icons.Locations[0].FullName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TryAndEat(Action action)
        {
            try { action.Invoke(); }
            catch { }
        }
    }
}