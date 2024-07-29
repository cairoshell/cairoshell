using ManagedShell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.ObjectModel;
using ManagedShell.Common.Helpers;
using System.Windows.Media;

namespace CairoDesktop.MenuBar
{
    /// <summary>
    /// Interaction logic for MenuBarShadow.xaml
    /// </summary>
    public partial class MenuBarShadow : Window
    {
        private readonly ICairoApplication _cairoApplication;
        private readonly MenuBar _menuBar;
        private readonly IWindowManager _windowManager;

        private double DpiScale = 1;
        public bool IsClosing;
        public bool AllowClose;

        private IntPtr Handle;
        private ScaleTransform _verticalFlipTransform = new ScaleTransform(1, -1);

        public MenuBarShadow(ICairoApplication cairoApplication, IWindowManager windowManager, MenuBar bar)
        {
            _cairoApplication = cairoApplication;
            _menuBar = bar;
            _windowManager = windowManager;

            InitializeComponent();
        }

        public void SetPosition()
        {
            if (_menuBar != null)
            {
                double desiredHeight = 14;
                double desiredTop = _menuBar.AppBarEdge == ManagedShell.AppBar.AppBarEdge.Top ? _menuBar.Top + _menuBar.ActualHeight : _menuBar.Top - desiredHeight;
                double desiredLeft = _menuBar.Left;
                double desiredWidth = _menuBar.ActualWidth;

                if (DpiScale != _menuBar.DpiScale)
                {
                    // we want to always match the menu bar DPI for correct positioning
                    long newDpi = (int)(_menuBar.DpiScale * 96) & 0xFFFF | (int)(_menuBar.DpiScale * 96) << 16;

                    NativeMethods.Rect newRect = new NativeMethods.Rect
                    {
                        Top = (int)(desiredTop * _menuBar.DpiScale),
                        Left = (int)(desiredLeft * _menuBar.DpiScale),
                        Bottom = (int)((desiredTop + desiredHeight) * _menuBar.DpiScale),
                        Right = (int)((desiredLeft + desiredWidth) * _menuBar.DpiScale)
                    };
                    IntPtr newRectPtr = Marshal.AllocHGlobal(Marshal.SizeOf(newRect));
                    Marshal.StructureToPtr(newRect, newRectPtr, false);
                    NativeMethods.SendMessage(Handle, (int)NativeMethods.WM.DPICHANGED, (IntPtr)newDpi, newRectPtr);
                    Marshal.FreeHGlobal(newRectPtr);
                }
                else
                {
                    NativeMethods.SetWindowPos(Handle, IntPtr.Zero, Convert.ToInt32(desiredLeft * DpiScale), Convert.ToInt32(desiredTop * DpiScale), Convert.ToInt32(desiredWidth * DpiScale), Convert.ToInt32(desiredHeight * DpiScale), (int)NativeMethods.SetWindowPosFlags.SWP_NOZORDER | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE);
                }

                if (_menuBar.AppBarEdge == ManagedShell.AppBar.AppBarEdge.Bottom && ShadowImage.RenderTransform != _verticalFlipTransform)
                {
                    ShadowImage.RenderTransform = _verticalFlipTransform;
                }
                else if (_menuBar.AppBarEdge != ManagedShell.AppBar.AppBarEdge.Bottom && ShadowImage.RenderTransform == _verticalFlipTransform)
                {
                    ShadowImage.RenderTransform = null;
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
                SetPosition();
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

            SetPosition();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;

            if (!_cairoApplication.IsShuttingDown && !_windowManager.IsSettingDisplays && !AllowClose)
            {
                IsClosing = false;
                e.Cancel = true;
            }
        }
    }
}
