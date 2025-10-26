using ManagedShell.Interop;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CairoDesktop.DynamicDesktop.Services;
using ManagedShell.Common.Helpers;

namespace CairoDesktop.DynamicDesktop
{
    /// <summary>
    /// Interaction logic for DesktopOverlay.xaml
    /// </summary>
    public partial class DesktopOverlay : Window
    {
        private readonly DesktopManager _desktopManager;
        public IntPtr Handle;

        public DesktopOverlay(DesktopManager manager)
        {
            InitializeComponent();
            
            _desktopManager = manager;
        }

        public void ResetPosition()
        {
            Rect posRect = _desktopManager.GetUsableDesktopRect();

            int swp = (int)NativeMethods.SetWindowPosFlags.SWP_NOZORDER | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE;
            if (posRect.Width < 0 || posRect.Height < 0)
            {
                swp |= (int)NativeMethods.SetWindowPosFlags.SWP_NOSIZE;
            }

            grid.Width = posRect.Width / DpiHelper.DpiScale;
            grid.Height = posRect.Height / DpiHelper.DpiScale;

            NativeMethods.SetWindowPos(Handle, IntPtr.Zero, (int)posRect.X, (int)posRect.Top, (int)posRect.Width, (int)posRect.Height, swp);
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

            ResetPosition();
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
