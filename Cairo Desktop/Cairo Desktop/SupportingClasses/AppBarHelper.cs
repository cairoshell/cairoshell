using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using CairoDesktop.Configuration;
using System.Windows.Interop;
using CairoDesktop.WindowsTray;
using CairoDesktop.Common.Logging;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.SupportingClasses
{
    public static class AppBarHelper
    {

        public enum WinTaskbarState : int
        {
            AutoHide = 1,
            OnTop = 0
        }

        private static object appBarLock = new object();
        public static List<IntPtr> appBars = new List<IntPtr>();
        private static int uCallBack = 0;

        public static int RegisterBar(AppBarWindow abWindow, double width, double height, ABEdge edge = ABEdge.ABE_TOP)
        {
            lock (appBarLock)
            {
                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
                IntPtr handle = new WindowInteropHelper(abWindow).Handle;
                abd.hWnd = handle;

                if (!appBars.Contains(handle))
                {
                    uCallBack = RegisterWindowMessage("AppBarMessage");
                    abd.uCallbackMessage = uCallBack;

                    prepareForInterop();
                    uint ret = SHAppBarMessage((int)ABMsg.ABM_NEW, ref abd);
                    interopDone();
                    appBars.Add(handle);
                    CairoLogger.Instance.Debug("AppBarHelper: Created AppBar for handle " + handle.ToString());

                    ABSetPos(abWindow, width, height, edge, true);
                }
                else
                {
                    prepareForInterop();
                    SHAppBarMessage((int)ABMsg.ABM_REMOVE, ref abd);
                    interopDone();
                    appBars.Remove(handle);
                    CairoLogger.Instance.Debug("AppBarHelper: Removed AppBar for handle " + handle.ToString());

                    return 0;
                }
            }
            
            return uCallBack;
        }

        private static void prepareForInterop()
        {
            // get shell window back so we can do appbar stuff
            if (Settings.Instance.EnableSysTray)
                NotificationArea.Instance.Suspend();
        }

        private static void interopDone()
        {
            // take back over
            if (Settings.Instance.EnableSysTray)
                NotificationArea.Instance.MakeActive();
        }

        public static void SetWinTaskbarVisibility(int swp)
        {
            // only run this if our taskbar is enabled, or if we are showing the Windows taskbar
            if (swp != (int) SetWindowPosFlags.SWP_HIDEWINDOW || Settings.Instance.EnableTaskbar)
            {
                IntPtr taskbarHwnd = findTaskbarHwnd();
                IntPtr startButtonHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);

                if (taskbarHwnd != IntPtr.Zero && (swp == (int)SetWindowPosFlags.SWP_HIDEWINDOW) == IsWindowVisible(taskbarHwnd))
                {
                    SetWindowPos(taskbarHwnd, (IntPtr)WindowZOrder.HWND_BOTTOM, 0, 0, 0, 0, swp | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE | (int)SetWindowPosFlags.SWP_NOACTIVATE);
                    if (startButtonHwnd != IntPtr.Zero) SetWindowPos(startButtonHwnd, (IntPtr)WindowZOrder.HWND_BOTTOM, 0, 0, 0, 0, swp | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE | (int)SetWindowPosFlags.SWP_NOACTIVATE);
                }

                // adjust secondary taskbars for multi-mon
                SetSecondaryTaskbarVisibility(swp);
            }
        }

        private static void SetSecondaryTaskbarVisibility(int swp)
        {
            IntPtr secTaskbarHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_SecondaryTrayWnd", null);

            // if we have 3+ monitors there may be multiple secondary taskbars
            while (secTaskbarHwnd != IntPtr.Zero)
            {
                if ((swp == (int)SetWindowPosFlags.SWP_HIDEWINDOW) == IsWindowVisible(secTaskbarHwnd))
                {
                    SetWindowPos(secTaskbarHwnd, (IntPtr)WindowZOrder.HWND_BOTTOM, 0, 0, 0, 0, swp | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE | (int)SetWindowPosFlags.SWP_NOACTIVATE);
                }
                secTaskbarHwnd = FindWindowEx(IntPtr.Zero, secTaskbarHwnd, "Shell_SecondaryTrayWnd", null);
            }
        }

        public static void SetWinTaskbarState(WinTaskbarState state)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(typeof(APPBARDATA));
            abd.hWnd = findTaskbarHwnd();
            abd.lParam = (IntPtr)state;

            prepareForInterop();
            SHAppBarMessage((int)ABMsg.ABM_SETSTATE, ref abd);
            interopDone();
        }

        private static IntPtr findTaskbarHwnd()
        {
            IntPtr taskbarHwnd = FindWindow("Shell_TrayWnd", "");

            if (NotificationArea.Instance.Handle != null && NotificationArea.Instance.Handle != IntPtr.Zero)
            {
                while (taskbarHwnd == NotificationArea.Instance.Handle)
                {
                    taskbarHwnd = FindWindowEx(IntPtr.Zero, taskbarHwnd, "Shell_TrayWnd", "");
                }
            }

            return taskbarHwnd;
        }

        public static void AppBarActivate(IntPtr hwnd)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(typeof(APPBARDATA));
            abd.hWnd = hwnd;
            abd.lParam = (IntPtr)Convert.ToInt32(true);
            prepareForInterop();
            SHAppBarMessage((int)ABMsg.ABM_ACTIVATE, ref abd);
            interopDone();

            // apparently the taskbars like to pop up when app bars change
            if (Settings.Instance.EnableTaskbar)
            {
                SetSecondaryTaskbarVisibility((int)SetWindowPosFlags.SWP_HIDEWINDOW);
            }
        }

        public static void AppBarWindowPosChanged(IntPtr hwnd)
        {
            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(typeof(APPBARDATA));
            abd.hWnd = hwnd;
            prepareForInterop();
            SHAppBarMessage((int)ABMsg.ABM_WINDOWPOSCHANGED, ref abd);
            interopDone();

            // apparently the taskbars like to pop up when app bars change
            if (Settings.Instance.EnableTaskbar)
            {
                SetSecondaryTaskbarVisibility((int)SetWindowPosFlags.SWP_HIDEWINDOW);
            }
        }

        public static void ABSetPos(AppBarWindow abWindow, double width, double height, ABEdge edge, bool isCreate = false)
        {
            lock (appBarLock)
            {
                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
                abd.hWnd = abWindow.Handle;
                abd.uEdge = (int)edge;
                int sWidth = (int)width;
                int sHeight = (int)height;

                int top = 0;
                int left = 0;
                int right = WindowManager.PrimaryMonitorDeviceSize.Width;
                int bottom = WindowManager.PrimaryMonitorDeviceSize.Height;

                if (abWindow.Screen != null)
                {
                    top = abWindow.Screen.Bounds.Y;
                    left = abWindow.Screen.Bounds.X;
                    right = abWindow.Screen.Bounds.Right;
                    bottom = abWindow.Screen.Bounds.Bottom;
                }

                if (abd.uEdge == (int)ABEdge.ABE_LEFT || abd.uEdge == (int)ABEdge.ABE_RIGHT)
                {
                    abd.rc.Top = top;
                    abd.rc.Bottom = bottom;
                    if (abd.uEdge == (int)ABEdge.ABE_LEFT)
                    {
                        abd.rc.Left = left;
                        abd.rc.Right = abd.rc.Left + sWidth;
                    }
                    else
                    {
                        abd.rc.Right = right;
                        abd.rc.Left = abd.rc.Right - sWidth;
                    }

                }
                else
                {
                    abd.rc.Left = left;
                    abd.rc.Right = right;
                    if (abd.uEdge == (int)ABEdge.ABE_TOP)
                    {
                        if (!abWindow.requiresScreenEdge)
                        {
                            abd.rc.Top = top + Convert.ToInt32(GetAppBarEdgeWindowsHeight((ABEdge)abd.uEdge, abWindow.Screen));
                        }
                        else
                            abd.rc.Top = top;
                        abd.rc.Bottom = abd.rc.Top + sHeight;
                    }
                    else
                    {
                        if (!abWindow.requiresScreenEdge)
                        {
                            abd.rc.Bottom = bottom - Convert.ToInt32(GetAppBarEdgeWindowsHeight((ABEdge)abd.uEdge, abWindow.Screen));
                        }
                        else
                            abd.rc.Bottom = bottom;
                        abd.rc.Top = abd.rc.Bottom - sHeight;
                    }
                }

                prepareForInterop();
                SHAppBarMessage((int)ABMsg.ABM_QUERYPOS, ref abd);
                interopDone();

                // system doesn't adjust all edges for us, do some adjustments
                switch (abd.uEdge)
                {
                    case (int)ABEdge.ABE_LEFT:
                        abd.rc.Right = abd.rc.Left + sWidth;
                        break;
                    case (int)ABEdge.ABE_RIGHT:
                        abd.rc.Left = abd.rc.Right - sWidth;
                        break;
                    case (int)ABEdge.ABE_TOP:
                        abd.rc.Bottom = abd.rc.Top + sHeight;
                        break;
                    case (int)ABEdge.ABE_BOTTOM:
                        abd.rc.Top = abd.rc.Bottom - sHeight;
                        break;
                }

                prepareForInterop();
                SHAppBarMessage((int)ABMsg.ABM_SETPOS, ref abd);
                interopDone();

                // check if new coords
                bool isSameCoords = false;
                if (!isCreate) isSameCoords = abd.rc.Top == (abWindow.Top * abWindow.dpiScale) && abd.rc.Left == (abWindow.Left * abWindow.dpiScale) && abd.rc.Bottom == (abWindow.Top * abWindow.dpiScale) + sHeight && abd.rc.Right == (abWindow.Left * abWindow.dpiScale) + sWidth;

                if (!isSameCoords)
                {
                    CairoLogger.Instance.Debug(string.Format("AppBarHelper: {0} changing position (TxLxBxR) to {1}x{2}x{3}x{4} from {5}x{6}x{7}x{8}", abWindow.Name, abd.rc.Top, abd.rc.Left, abd.rc.Bottom, abd.rc.Right, (abWindow.Top * abWindow.dpiScale), (abWindow.Left * abWindow.dpiScale), (abWindow.Top * abWindow.dpiScale) + sHeight, (abWindow.Left * abWindow.dpiScale) + sWidth));
                    abWindow.setAppBarPosition(abd.rc);
                }

                abWindow.afterAppBarPos(isSameCoords, abd.rc);

                if (abd.rc.Bottom - abd.rc.Top < sHeight)
                    ABSetPos(abWindow, width, height, edge);
            }
        }

        public static double GetAppBarEdgeWindowsHeight(ABEdge edge, Screen screen)
        {
            double edgeHeight = 0;

            foreach (MenuBar menuBar in WindowManager.Instance.MenuBarWindows)
            {
                if (menuBar.requiresScreenEdge && menuBar.appBarEdge == edge && menuBar.Screen.DeviceName == screen.DeviceName) edgeHeight += menuBar.Height;
            }

            foreach (Taskbar taskbar in WindowManager.Instance.TaskbarWindows)
            {
                if (taskbar.requiresScreenEdge && taskbar.appBarEdge == edge && taskbar.Screen.DeviceName == screen.DeviceName) edgeHeight += taskbar.Height;
            }

            return edgeHeight;
        }
    }
}
