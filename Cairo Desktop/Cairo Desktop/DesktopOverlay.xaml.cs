using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CairoDesktop
{
    /// <summary>
    /// Interaction logic for DesktopOverlay.xaml
    /// </summary>
    public partial class DesktopOverlay : Window
    {
        private DesktopManager desktopManager;
        public IntPtr Handle;

        public DesktopOverlay(DesktopManager manager)
        {
            InitializeComponent();

            desktopManager = manager;
            ResetPosition();
        }

        public void ResetPosition()
        {
            double top = System.Windows.Forms.SystemInformation.WorkingArea.Top / Shell.DpiScale;
            double taskbarHeight = 0;

            if (Settings.Instance.TaskbarMode == 1 && WindowManager.Instance.TaskbarWindows.Count > 0)
            {
                // special case, since work area is not reduced with this setting
                // this keeps the desktop going beneath the TaskBar

                // get the TaskBar's height
                Taskbar taskbar = WindowManager.GetScreenWindow(WindowManager.Instance.TaskbarWindows, System.Windows.Forms.Screen.PrimaryScreen);

                if (taskbar != null)
                {
                    taskbarHeight = taskbar.ActualHeight;
                }

                // top TaskBar means we should push down
                if (Settings.Instance.TaskbarPosition == 1)
                {
                    top += taskbarHeight;
                }
            }

            Width = WindowManager.PrimaryMonitorWorkArea.Width / Shell.DpiScale;
            Height = (WindowManager.PrimaryMonitorWorkArea.Height / Shell.DpiScale) - taskbarHeight;

            grid.Width = Width;
            grid.Height = Height;

            Top = top;
            Left = System.Windows.Forms.SystemInformation.WorkingArea.Left / Shell.DpiScale;
        }

        public void BringToFront()
        {
            NativeMethods.SetWindowPos(Handle, (IntPtr)NativeMethods.WindowZOrder.HWND_TOPMOST, 0, 0, 0, 0, (int)NativeMethods.SetWindowPosFlags.SWP_NOMOVE | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE | (int)NativeMethods.SetWindowPosFlags.SWP_NOSIZE);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            desktopManager.IsOverlayOpen = false;
        }

        private void DesktopOverlayWindow_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            Handle = helper.Handle;

            Shell.HideWindowFromTasks(Handle);
        }

        private void DesktopOverlayWindow_LocationChanged(object sender, EventArgs e)
        {
            ResetPosition();
        }

        private void DesktopOverlayWindow_KeyDown(object sender, KeyEventArgs e)
        {
            desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void DesktopOverlayWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void DesktopOverlayWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void DesktopOverlayWindow_DragOver(object sender, DragEventArgs e)
        {
            desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void DesktopOverlayWindow_Drop(object sender, DragEventArgs e)
        {
            desktopManager.DesktopWindow?.RaiseEvent(e);
        }

        private void grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            desktopManager.DesktopWindow?.grid.RaiseEvent(e);
        }
    }
}
