using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for MenuBarShadow.xaml
    /// </summary>
    public partial class MenuBarShadow : Window
    {
        private double dpiScale = 1;
        public bool IsClosing;
        public bool AllowClose;

        public MenuBar MenuBar;
        private IntPtr Handle;

        public MenuBarShadow(MenuBar bar)
        {
            MenuBar = bar;

            InitializeComponent();

            SetPosition();
        }

        public void SetPosition()
        {
            if (MenuBar != null)
            {
                dpiScale = VisualTreeHelper.GetDpi(this).DpiScaleX;

                double desiredTop = MenuBar.Top + MenuBar.ActualHeight;
                double desiredLeft = MenuBar.Left;
                double desiredHeight = 14;
                double desiredWidth = MenuBar.ActualWidth;

                if (dpiScale != MenuBar.dpiScale)
                {
                    // we want to always match the menu bar DPI for correct positioning
                    long newDpi = (int)(MenuBar.dpiScale * 96) & 0xFFFF | (int)(MenuBar.dpiScale * 96) << 16;

                    NativeMethods.Rect newRect = new NativeMethods.Rect
                    {
                        Top = (int)(desiredTop * MenuBar.dpiScale),
                        Left = (int)(desiredLeft * MenuBar.dpiScale),
                        Bottom = (int)((desiredTop + desiredHeight) * MenuBar.dpiScale),
                        Right = (int)((desiredLeft + desiredWidth) * MenuBar.dpiScale)
                    };
                    IntPtr newRectPtr = Marshal.AllocHGlobal(Marshal.SizeOf(newRect));
                    Marshal.StructureToPtr(newRect, newRectPtr, false);
                    NativeMethods.SendMessage(Handle, (int)NativeMethods.WM.DPICHANGED, (IntPtr)newDpi, newRectPtr);
                    Marshal.FreeHGlobal(newRectPtr);
                }
                else
                {
                    // reset properties so we actually reposition
                    Top = 0;

                    Top = desiredTop;
                    Left = desiredLeft;
                    Height = desiredHeight;
                    Width = desiredWidth;
                }
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            int HTTRANSPARENT = (-1);
            if (msg == (int)NativeMethods.WM.NCHITTEST)
            {
                handled = true;
                return (IntPtr)HTTRANSPARENT;
            }
            else if (msg == (int)NativeMethods.WM.DPICHANGED)
            {
                dpiScale = (wparam.ToInt32() & 0xFFFF) / 96d;
            }
            else
            {
                handled = false;
            }
            return IntPtr.Zero;
        }

        private void MenuBarShadow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            Handle = helper.Handle;
            HwndSource source = HwndSource.FromHwnd(Handle);

            source.AddHook(WndProc);

            // Makes click-through by adding transparent style
            // basically same as Shell.HideWindowFromTasks(helper.Handle);
            NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE) | (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW | (int)NativeMethods.ExtendedWindowStyles.WS_EX_TRANSPARENT);

            Shell.ExcludeWindowFromPeek(helper.Handle);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
            if (!CairoApplication.IsShuttingDown && !WindowManager.Instance.IsSettingDisplays && !AllowClose)
            {
                IsClosing = false;
                e.Cancel = true;
            }
        }
    }
}
