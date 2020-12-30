using ManagedShell.Interop;
using CairoDesktop.SupportingClasses;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ManagedShell.Common.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for MenuBarShadow.xaml
    /// </summary>
    public partial class MenuBarShadow : Window
    {
        private double DpiScale = 1;
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
                DpiScale = VisualTreeHelper.GetDpi(this).DpiScaleX;

                double desiredTop = MenuBar.Top + MenuBar.ActualHeight;
                double desiredLeft = MenuBar.Left;
                double desiredHeight = 14;
                double desiredWidth = MenuBar.ActualWidth;

                if (DpiScale != MenuBar.DpiScale)
                {
                    // we want to always match the menu bar DPI for correct positioning
                    long newDpi = (int)(MenuBar.DpiScale * 96) & 0xFFFF | (int)(MenuBar.DpiScale * 96) << 16;

                    NativeMethods.Rect newRect = new NativeMethods.Rect
                    {
                        Top = (int)(desiredTop * MenuBar.DpiScale),
                        Left = (int)(desiredLeft * MenuBar.DpiScale),
                        Bottom = (int)((desiredTop + desiredHeight) * MenuBar.DpiScale),
                        Right = (int)((desiredLeft + desiredWidth) * MenuBar.DpiScale)
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
                DpiScale = (wparam.ToInt32() & 0xFFFF) / 96d;
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

            WindowHelper.ExcludeWindowFromPeek(helper.Handle);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
            var windowManager = CairoApplication.Current.Host.Services.GetService<WindowManager>();

            if (!CairoApplication.IsShuttingDown && !windowManager.IsSettingDisplays && !AllowClose)
            {
                IsClosing = false;
                e.Cancel = true;
            }
        }
    }
}
