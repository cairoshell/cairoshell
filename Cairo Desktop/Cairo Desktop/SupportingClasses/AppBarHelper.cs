using System;
using System.Runtime.InteropServices;
using CairoDesktop.Interop;
using System.Collections.Generic;
using System.Windows.Forms;
using CairoDesktop.Configuration;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using CairoDesktop.WindowsTray;
using CairoDesktop.Common.Logging;

namespace CairoDesktop.SupportingClasses
{
    public static class AppBarHelper
    {
        public enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        public enum WinTaskbarState : int
        {
            AutoHide = 1,
            OnTop = 0
        }

        private static object appBarLock = new object();

        public static int RegisterBar(Window abWindow, Screen screen, double width, double height, ABEdge edge = ABEdge.ABE_TOP)
        {
            lock (appBarLock)
            {
                NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
                IntPtr handle = new WindowInteropHelper(abWindow).Handle;
                abd.hWnd = handle;

                if (!appBars.Contains(handle))
                {
                    uCallBack = NativeMethods.RegisterWindowMessage("AppBarMessage");
                    abd.uCallbackMessage = uCallBack;

                    prepareForInterop();
                    uint ret = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_NEW, ref abd);
                    interopDone();
                    appBars.Add(handle);
                    CairoLogger.Instance.Debug("Created AppBar for handle " + handle.ToString());

                    ABSetPos(abWindow, screen, width, height, edge, true);
                }
                else
                {
                    prepareForInterop();
                    NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_REMOVE, ref abd);
                    interopDone();
                    appBars.Remove(handle);
                    CairoLogger.Instance.Debug("Removed AppBar for handle " + handle.ToString());

                    return 0;
                }
            }
            
            return uCallBack;
        }

        public static List<IntPtr> appBars = new List<IntPtr>();

        private static int uCallBack = 0;

        private static void prepareForInterop()
        {
            // get shell window back so we can do appbar stuff
            if (Settings.EnableSysTray)
                NotificationArea.Instance.Suspend();
        }

        private static void interopDone()
        {
            // take back over
            if (Settings.EnableSysTray)
                NotificationArea.Instance.MakeActive();
        }

        public static void SetWinTaskbarPos(int swp)
        {
            IntPtr taskbarHwnd = NativeMethods.FindWindow("Shell_TrayWnd", "");
            IntPtr taskbarInsertAfter = (IntPtr)1;

            if (NotificationArea.Instance.Handle != null && NotificationArea.Instance.Handle != IntPtr.Zero)
            {
                while (taskbarHwnd == NotificationArea.Instance.Handle)
                {
                    taskbarHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, taskbarHwnd, "Shell_TrayWnd", "");
                }
            }

            IntPtr startButtonHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
            NativeMethods.SetWindowPos(taskbarHwnd, taskbarInsertAfter, 0, 0, 0, 0, swp | (int)NativeMethods.SetWindowPosFlags.SWP_NOMOVE | (int)NativeMethods.SetWindowPosFlags.SWP_NOSIZE | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE);
            NativeMethods.SetWindowPos(startButtonHwnd, taskbarInsertAfter, 0, 0, 0, 0, swp | (int)NativeMethods.SetWindowPosFlags.SWP_NOMOVE | (int)NativeMethods.SetWindowPosFlags.SWP_NOSIZE | (int)NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE);
            
            // adjust secondary taskbars for multi-mon
            if (swp == (int)NativeMethods.SetWindowPosFlags.SWP_HIDEWINDOW)
                SetSecondaryTaskbarVisibility(NativeMethods.WindowShowStyle.Hide);
            else
                SetSecondaryTaskbarVisibility(NativeMethods.WindowShowStyle.ShowNoActivate);
        }

        public static void SetWinTaskbarState(WinTaskbarState state)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
            abd.hWnd = NativeMethods.FindWindow("Shell_TrayWnd");

            if (NotificationArea.Instance.Handle != null && NotificationArea.Instance.Handle != IntPtr.Zero)
            {
                while (abd.hWnd == NotificationArea.Instance.Handle)
                {
                    abd.hWnd = NativeMethods.FindWindowEx(IntPtr.Zero, abd.hWnd, "Shell_TrayWnd", "");
                }
            }

            abd.lParam = (IntPtr)state;
            prepareForInterop();
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_SETSTATE, ref abd);
            interopDone();
        }

        private static void SetSecondaryTaskbarVisibility(NativeMethods.WindowShowStyle shw)
        {
            bool complete = false;
            IntPtr secTaskbarHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_SecondaryTrayWnd", null);

            // if we have 3+ monitors there may be multiple secondary taskbars
            while (!complete)
            {
                if (secTaskbarHwnd != IntPtr.Zero)
                {
                    NativeMethods.ShowWindowAsync(secTaskbarHwnd, shw);
                    secTaskbarHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, secTaskbarHwnd, "Shell_SecondaryTrayWnd", null);
                }
                else
                    complete = true;
            }
        }

        public static void AppBarActivate(IntPtr hwnd)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
            abd.hWnd = hwnd;
            abd.lParam = (IntPtr)Convert.ToInt32(true);
            prepareForInterop();
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_ACTIVATE, ref abd);
            interopDone();

            // apparently the taskbars like to pop up when app bars change
            if (Settings.EnableTaskbar)
            {
                SetSecondaryTaskbarVisibility(NativeMethods.WindowShowStyle.Hide);
            }
        }

        public static void AppBarWindowPosChanged(IntPtr hwnd)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
            abd.hWnd = hwnd;
            prepareForInterop();
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_WINDOWPOSCHANGED, ref abd);
            interopDone();
        }

        public static void ABSetPos(Window abWindow, Screen screen, double width, double height, ABEdge edge, bool isCreate = false)
        {
            lock (appBarLock)
            {
                NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
                abd.cbSize = Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
                IntPtr handle = new WindowInteropHelper(abWindow).Handle;
                abd.hWnd = handle;
                abd.uEdge = (int)edge;
                int sWidth = (int)width;
                int sHeight = (int)height;

                int top = 0;
                int left = SystemInformation.WorkingArea.Left;
                int right = SystemInformation.WorkingArea.Right;
                int bottom = PrimaryMonitorDeviceSize.Height;

                double dpiScale = PresentationSource.FromVisual(abWindow).CompositionTarget.TransformToDevice.M11;

                if (screen != null)
                {
                    top = screen.Bounds.Y;
                    left = screen.WorkingArea.Left;
                    right = screen.WorkingArea.Right;
                    bottom = screen.Bounds.Bottom;
                }

                if (abd.uEdge == (int)ABEdge.ABE_LEFT || abd.uEdge == (int)ABEdge.ABE_RIGHT)
                {
                    abd.rc.top = top;
                    abd.rc.bottom = bottom;
                    if (abd.uEdge == (int)ABEdge.ABE_LEFT)
                    {
                        abd.rc.left = left;
                        abd.rc.right = abd.rc.left + sWidth;
                    }
                    else
                    {
                        abd.rc.right = right;
                        abd.rc.left = abd.rc.right - sWidth;
                    }

                }
                else
                {
                    abd.rc.left = left;
                    abd.rc.right = right;
                    if (abd.uEdge == (int)ABEdge.ABE_TOP)
                    {
                        if (abWindow is Taskbar)
                            abd.rc.top = top + Convert.ToInt32(Startup.MenuBarWindow.Height);
                        else
                            abd.rc.top = top;
                        abd.rc.bottom = abd.rc.top + sHeight;
                    }
                    else
                    {
                        abd.rc.bottom = bottom;
                        abd.rc.top = abd.rc.bottom - sHeight;
                    }
                }

                prepareForInterop();
                NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_QUERYPOS, ref abd);
                interopDone();

                // system doesn't adjust all edges for us, do some adjustments
                switch (abd.uEdge)
                {
                    case (int)ABEdge.ABE_LEFT:
                        abd.rc.right = abd.rc.left + sWidth;
                        break;
                    case (int)ABEdge.ABE_RIGHT:
                        abd.rc.left = abd.rc.right - sWidth;
                        break;
                    case (int)ABEdge.ABE_TOP:
                        abd.rc.bottom = abd.rc.top + sHeight;
                        break;
                    case (int)ABEdge.ABE_BOTTOM:
                        abd.rc.top = abd.rc.bottom - sHeight;
                        break;
                }

                prepareForInterop();
                NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_SETPOS, ref abd);
                interopDone();

                // tracing
                bool isSameCoords = false;
                if (!isCreate) isSameCoords = abd.rc.top == (abWindow.Top * dpiScale) && abd.rc.left == (abWindow.Left * dpiScale) && abd.rc.bottom == (abWindow.Top * dpiScale) + sHeight && abd.rc.right == (abWindow.Left * dpiScale) + sWidth;
                int h = abd.rc.bottom - abd.rc.top;
                if (!isSameCoords) CairoLogger.Instance.Debug(abWindow.Name + " AppBar height is " + h.ToString());

                abWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new ResizeDelegate(DoResize), abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, isSameCoords);

                if (h < sHeight)
                    ABSetPos(abWindow, screen, width, height, edge);
            }
        }

        private delegate void ResizeDelegate(IntPtr hWnd, int x, int y, int cx, int cy, bool isSameCoords);
        private static void DoResize(IntPtr hWnd, int x, int y, int cx, int cy, bool isSameCoords)
        {
            if (!isSameCoords) NativeMethods.MoveWindow(hWnd, x, y, cx, cy, true);

            // apparently the taskbars like to pop up when app bars change
            if (Settings.EnableTaskbar)
            {
                SetWinTaskbarPos((int)NativeMethods.SetWindowPosFlags.SWP_HIDEWINDOW);
            }

            if (!isSameCoords)
            {
                foreach (MenuBarShadow barShadow in Startup.MenuBarShadowWindows)
                {
                    if (barShadow.MenuBar != null && barShadow.MenuBar.handle == hWnd)
                        barShadow.SetPosition();
                }

                if (Startup.DesktopWindow != null)
                    Startup.DesktopWindow.ResetPosition();
            }
        }

        public static System.Drawing.Size PrimaryMonitorSize
        {
            get
            {
                return new System.Drawing.Size(Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth / Shell.DpiScaleAdjustment), Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight / Shell.DpiScaleAdjustment));
            }
        }

        public static System.Drawing.Size PrimaryMonitorDeviceSize
        {
            get
            {
                return new System.Drawing.Size(NativeMethods.GetSystemMetrics(0), NativeMethods.GetSystemMetrics(1));
            }
        }

        public static System.Drawing.Size PrimaryMonitorWorkArea
        {
            get
            {
                return new System.Drawing.Size(SystemInformation.WorkingArea.Right - SystemInformation.WorkingArea.Left, SystemInformation.WorkingArea.Bottom - SystemInformation.WorkingArea.Top);
            }
        }
        
        public static void SetWorkArea(Screen screen)
        {
            NativeMethods.RECT rc;
            rc.left = screen.Bounds.Left;
            rc.right = screen.Bounds.Right;

            // only allocate space for taskbar if enabled
            if (Settings.EnableTaskbar && Settings.TaskbarMode == 0)
            {
                if (Settings.TaskbarPosition == 1)
                {
                    rc.top = screen.Bounds.Top + (int)(Startup.MenuBarWindow.ActualHeight * Shell.DpiScale) + (int)(Startup.TaskbarWindow.ActualHeight * Shell.DpiScale);
                    rc.bottom = screen.Bounds.Bottom;
                }
                else
                {
                    rc.top = screen.Bounds.Top + (int)(Startup.MenuBarWindow.ActualHeight * Shell.DpiScale);
                    rc.bottom = screen.Bounds.Bottom - (int)(Startup.TaskbarWindow.ActualHeight * Shell.DpiScale);
                }
            }
            else
            {
                rc.top = screen.Bounds.Top + (int)(Startup.MenuBarWindow.ActualHeight * Shell.DpiScale);
                rc.bottom = screen.Bounds.Bottom;
            }

            NativeMethods.SystemParametersInfo((int)NativeMethods.SPI.SPI_SETWORKAREA, 0, ref rc, (1 | 2));

            if (Startup.DesktopWindow != null)
                Startup.DesktopWindow.ResetPosition();
        }
        
        public static void ResetWorkArea()
        {
            // set work area back to full screen size. we can't assume what pieces of the old workarea may or may not be still used
            NativeMethods.RECT oldWorkArea;
            oldWorkArea.left = SystemInformation.VirtualScreen.Left;
            oldWorkArea.top = SystemInformation.VirtualScreen.Top;
            oldWorkArea.right = SystemInformation.VirtualScreen.Right;
            oldWorkArea.bottom = SystemInformation.VirtualScreen.Bottom;

            NativeMethods.SystemParametersInfo((int)NativeMethods.SPI.SPI_SETWORKAREA, 0, ref oldWorkArea, (1 | 2));
        }
    }
}
