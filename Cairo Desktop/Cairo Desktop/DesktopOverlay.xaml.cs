using CairoDesktop.Configuration;
using ManagedShell.Interop;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CairoDesktop.Services;
using ManagedShell.AppBar;
using ManagedShell.Common.Helpers;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopOverlay.xaml
    /// </summary>
    public partial class DesktopOverlay : Window
    {
        private readonly DesktopManager _desktopManager;
        private readonly AppBarManager _appBarManager;
        public IntPtr Handle;

        public DesktopOverlay(DesktopManager manager, AppBarManager appBarManager)
        {
            InitializeComponent();
            
            _desktopManager = manager;
            _appBarManager = appBarManager;

            ResetPosition();
        }

        public void ResetPosition()
        {
            double top = System.Windows.Forms.SystemInformation.WorkingArea.Top / DpiHelper.DpiScale;
            double taskbarHeight = 0;

            if (Settings.Instance.TaskbarMode == 1)
            {
                // special case, since work area is not reduced with this setting
                // this keeps the desktop going beneath the TaskBar

                // get the TaskBar's height
                double dpiScale = 1;
                AppBarScreen screen = AppBarScreen.FromPrimaryScreen();
                NativeMethods.Rect workAreaRect = _appBarManager.GetWorkArea(ref dpiScale, screen, false, false);
                taskbarHeight = (screen.Bounds.Bottom - workAreaRect.Bottom) / dpiScale;

                // top TaskBar means we should push down
                if (Settings.Instance.TaskbarPosition == 1)
                {
                    top = workAreaRect.Top / dpiScale;
                }
            }

            Width = WindowManager.PrimaryMonitorWorkArea.Width / DpiHelper.DpiScale;
            Height = (WindowManager.PrimaryMonitorWorkArea.Height / DpiHelper.DpiScale) - taskbarHeight;

            grid.Width = Width;
            grid.Height = Height;

            Top = top;
            Left = System.Windows.Forms.SystemInformation.WorkingArea.Left / DpiHelper.DpiScale;
        }

        public void BringToFront()
        {
            NativeMethods.SetWindowPos(Handle, (IntPtr)NativeMethods.WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, (int)NativeMethods.SetWindowPosFlags.SWP_NOMOVE | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE | (int)NativeMethods.SetWindowPosFlags.SWP_NOSIZE);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _desktopManager.IsOverlayOpen = false;
        }

        private void DesktopOverlayWindow_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            Handle = helper.Handle;

            WindowHelper.HideWindowFromTasks(Handle);
        }

        private void DesktopOverlayWindow_LocationChanged(object sender, EventArgs e)
        {
            ResetPosition();
        }

        private void DesktopOverlayWindow_KeyDown(object sender, KeyEventArgs e)
        {
            _desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void DesktopOverlayWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void DesktopOverlayWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void DesktopOverlayWindow_DragOver(object sender, DragEventArgs e)
        {
            _desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void DesktopOverlayWindow_Drop(object sender, DragEventArgs e)
        {
            _desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _desktopManager.DesktopWindow?.grid.RaiseEvent(e);
        }
    }
}
