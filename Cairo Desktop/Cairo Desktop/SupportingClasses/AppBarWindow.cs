using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace CairoDesktop.SupportingClasses
{
    public class AppBarWindow : Window
    {
        // Screen properties
        public System.Windows.Forms.Screen Screen;
        internal double dpiScale = 1.0;
        protected bool processScreenChanges = false;

        private enum ScreenSetupReason
        {
            DeviceChange,
            DisplayChange,
            DpiChange
        }

        // Window properties
        private WindowInteropHelper helper;
        private bool isRaising;
        public IntPtr Handle;
        public bool IsClosing = false;
        protected double desiredHeight = 0;

        // AppBar properties
        private int appbarMessageId = -1;
        internal NativeMethods.ABEdge appBarEdge = NativeMethods.ABEdge.ABE_TOP;
        protected bool enableAppBar = true;
        internal bool requiresScreenEdge = false;

        public AppBarWindow()
        {
            Closing += OnClosing;
            SourceInitialized += OnSourceInitialized;

            AllowsTransparency = true;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Title = "";
            Topmost = true;
            UseLayoutRounding = true;
            WindowStyle = WindowStyle.None;
        }

        #region Events
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            // set up helper and get handle
            helper = new WindowInteropHelper(this);
            Handle = helper.Handle;

            // set up window procedure
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            // set initial DPI. We do it here so that we get the correct value when DPI has changed since initial user logon to the system.
            if (Screen.Primary)
            {
                Shell.DpiScale = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M11;
            }

            dpiScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            setPosition();

            if (Shell.IsCairoRunningAsShell)
            {
                // set position again, on a delay, in case one display has a different DPI. for some reason the system overrides us if we don't wait
                delaySetPosition();
            }

            // register appbar
            if (!Shell.IsCairoRunningAsShell && enableAppBar) appbarMessageId = AppBarHelper.RegisterBar(this, ActualWidth * dpiScale, desiredHeight * dpiScale, appBarEdge);

            // hide from alt-tab etc
            Shell.HideWindowFromTasks(Handle);

            // register for full-screen notifications
            FullScreenHelper.Instance.FullScreenApps.CollectionChanged += FullScreenApps_CollectionChanged;

            postInit();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;

            customClosing();

            if (Startup.IsShuttingDown && Screen.Primary)
            {
                // unregister appbar
                if (AppBarHelper.appBars.Contains(Handle))
                    AppBarHelper.RegisterBar(this, ActualWidth * dpiScale, desiredHeight * dpiScale);

                // unregister full-screen notifications
                FullScreenHelper.Instance.FullScreenApps.CollectionChanged -= FullScreenApps_CollectionChanged;

                // dispose the full screen helper since we are the primary instance
                FullScreenHelper.Instance.Dispose();
            }
            else if (WindowManager.Instance.IsSettingDisplays || Startup.IsShuttingDown)
            {
                // unregister appbar
                if (AppBarHelper.appBars.Contains(Handle))
                    AppBarHelper.RegisterBar(this, ActualWidth * dpiScale, desiredHeight * dpiScale);

                // unregister full-screen notifications
                FullScreenHelper.Instance.FullScreenApps.CollectionChanged -= FullScreenApps_CollectionChanged;
            }
            else
            {
                IsClosing = false;
                e.Cancel = true;
            }
        }

        private void FullScreenApps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            bool found = false;

            foreach (FullScreenHelper.FullScreenApp app in FullScreenHelper.Instance.FullScreenApps)
            {
                if (app.screen.DeviceName == Screen.DeviceName)
                {
                    // we need to not be on top now
                    found = true;
                    break;
                }
            }

            if (found && Topmost)
            {
                setFullScreenMode(true);
            }
            else if (!found && !Topmost)
            {
                setFullScreenMode(false);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == appbarMessageId && appbarMessageId != -1)
            {
                switch ((NativeMethods.AppBarNotifications)wParam.ToInt32())
                {
                    case NativeMethods.AppBarNotifications.PosChanged:
                        AppBarHelper.ABSetPos(this, ActualWidth * dpiScale, desiredHeight * dpiScale, appBarEdge);
                        break;

                    case NativeMethods.AppBarNotifications.WindowArrange:
                        if ((int)lParam != 0) // before
                        {
                            Visibility = Visibility.Collapsed;
                        }
                        else // after
                        {
                            Visibility = Visibility.Visible;
                        }

                        break;

                    case NativeMethods.AppBarNotifications.FullScreenApp:
                        AppBarHelper.SetWinTaskbarVisibility((int)NativeMethods.SetWindowPosFlags.SWP_HIDEWINDOW);
                        break;
                }
                handled = true;
            }
            else if (msg == (int)NativeMethods.WM.ACTIVATE && enableAppBar && !Shell.IsCairoRunningAsShell && !Startup.IsShuttingDown)
            {
                AppBarHelper.AppBarActivate(hwnd);
            }
            else if (msg == (int)NativeMethods.WM.WINDOWPOSCHANGING)
            {
                // Extract the WINDOWPOS structure corresponding to this message
                NativeMethods.WINDOWPOS wndPos = NativeMethods.WINDOWPOS.FromMessage(lParam);

                // Determine if the z-order is changing (absence of SWP_NOZORDER flag)
                // If we are intentionally trying to become topmost, make it so
                if (isRaising && (wndPos.flags & NativeMethods.SetWindowPosFlags.SWP_NOZORDER) == 0)
                {
                    // Sometimes Windows thinks we shouldn't go topmost, so poke here to make it happen.
                    wndPos.hwndInsertAfter = (IntPtr)NativeMethods.WindowZOrder.HWND_TOPMOST;
                    wndPos.UpdateMessage(lParam);
                }
            }
            else if (msg == (int)NativeMethods.WM.WINDOWPOSCHANGED && enableAppBar && !Shell.IsCairoRunningAsShell && !Startup.IsShuttingDown)
            {
                AppBarHelper.AppBarWindowPosChanged(hwnd);
            }
            else if (msg == (int)NativeMethods.WM.DPICHANGED)
            {
                if (Screen.Primary)
                {
                    Shell.DpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;
                }

                dpiScale = (wParam.ToInt32() & 0xFFFF) / 96d;

                setScreenProperties(ScreenSetupReason.DpiChange);
            }
            else if (msg == (int)NativeMethods.WM.DISPLAYCHANGE)
            {
                setScreenProperties(ScreenSetupReason.DisplayChange);
                handled = true;
            }
            else if (msg == (int)NativeMethods.WM.DEVICECHANGE && (int)wParam == 0x0007)
            {
                setScreenProperties(ScreenSetupReason.DeviceChange);
                handled = true;
            }

            // call custom implementations' window procedure
            return customWndProc(hwnd, msg, wParam, lParam, ref handled);
        }
        #endregion

        #region Helpers
        private void delaySetPosition()
        {
            // delay changing things when we are shell. it seems that explorer AppBars do this too.
            // if we don't, the system moves things to bad places
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
            timer.Start();
            timer.Tick += (sender1, args) =>
            {
                setPosition();
                timer.Stop();
            };
        }

        internal void setScreenPosition()
        {
            // set our position if running as shell, otherwise let AppBar do the work
            if (Shell.IsCairoRunningAsShell || !enableAppBar) delaySetPosition();
            else if (enableAppBar) AppBarHelper.ABSetPos(this, ActualWidth * dpiScale, desiredHeight * dpiScale, appBarEdge);
        }

        internal void setAppBarPosition(NativeMethods.Rect rect)
        {
            Top = rect.Top / dpiScale;
            Left = rect.Left / dpiScale;
            Width = (rect.Right - rect.Left) / dpiScale;
            Height = (rect.Bottom - rect.Top) / dpiScale;
        }

        private void setScreenProperties(ScreenSetupReason reason)
        {
            // process screen changes if we are on the primary display (or any display in the case of a DPI change, since only the changed display receives that message)
            // and the designated window. suppress this if we are shutting down (which can trigger this method on multi-dpi setups due to window movements)
            if ((Screen.Primary || reason == ScreenSetupReason.DpiChange) && processScreenChanges && !Startup.IsShuttingDown)
            {
                CairoLogger.Instance.Debug("AppBarWindow: Calling screen setup due to " + reason);
                WindowManager.Instance.NotifyDisplayChange(); // update Cairo window list based on new screen setup
            }
        }

        private void setFullScreenMode(bool entering)
        {
            if (entering)
            {
                CairoLogger.Instance.Debug(string.Format("{0} on {1} conceeding to full-screen app", Name, Screen.DeviceName));

                Topmost = false;
                Shell.ShowWindowBottomMost(Handle);
            }
            else
            {
                CairoLogger.Instance.Debug(string.Format("{0} on {1} returning to normal state", Name, Screen.DeviceName));

                isRaising = true;
                Topmost = true;
                Shell.ShowWindowTopMost(Handle);
                isRaising = false;
            }
        }
        #endregion

        #region Virtual methods
        internal virtual void afterAppBarPos(bool isSameCoords, NativeMethods.Rect rect)
        {
            // apparently the taskbars like to pop up when app bars change
            if (Settings.Instance.EnableTaskbar && !Startup.IsShuttingDown)
            {
                AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.AutoHide);
                AppBarHelper.SetWinTaskbarVisibility((int)NativeMethods.SetWindowPosFlags.SWP_HIDEWINDOW);
            }

            if (!isSameCoords)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
                timer.Start();
                timer.Tick += (sender1, args) =>
                {
                    // set position again, since WPF may have overridden the original change from AppBarHelper
                    setAppBarPosition(rect);

                    if (Screen.Primary && WindowManager.Instance.DesktopWindow != null)
                        WindowManager.Instance.DesktopWindow.ResetPosition();

                    timer.Stop();
                };
            }
        }

        protected virtual void postInit() { }

        protected virtual void customClosing() { }

        protected virtual IntPtr customWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return IntPtr.Zero;
        }

        internal virtual void setPosition() { }
        #endregion
    }
}
