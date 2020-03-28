using System;
using System.Runtime.InteropServices;
using CairoDesktop.Interop;
using System.Collections.Generic;
using System.Windows.Forms;
using CairoDesktop.Configuration;
using System.Windows;
using System.Windows.Interop;
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

        public static int RegisterBar(AppBarWindow abWindow, Screen screen, double width, double height, ABEdge edge = ABEdge.ABE_TOP)
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
                    CairoLogger.Instance.Debug("AppBarHelper: Created AppBar for handle " + handle.ToString());

                    ABSetPos(abWindow, screen, width, height, edge, true);
                }
                else
                {
                    prepareForInterop();
                    NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_REMOVE, ref abd);
                    interopDone();
                    appBars.Remove(handle);
                    CairoLogger.Instance.Debug("AppBarHelper: Removed AppBar for handle " + handle.ToString());

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
            if (Settings.Instance.EnableSysTray)
                NotificationArea.Instance.Suspend();
        }

        private static void interopDone()
        {
            // take back over
            if (Settings.Instance.EnableSysTray)
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
            if (Settings.Instance.EnableTaskbar)
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

        public static void ABSetPos(AppBarWindow abWindow, Screen screen, double width, double height, ABEdge edge, bool isCreate = false)
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
                int left = 0;
                int right = WindowManager.PrimaryMonitorDeviceSize.Width;
                int bottom = WindowManager.PrimaryMonitorDeviceSize.Height;

                /*PresentationSource ps = PresentationSource.FromVisual(abWindow);

                if (ps == null)
                {
                    // if we are racing with screen setting changes, this will be null
                    CairoLogger.Instance.Debug("AppBarHelper: Aborting ABSetPos due to window destruction");
                    return;
                }

                double dpiScale = ps.CompositionTarget.TransformToDevice.M11;*/

                if (screen != null)
                {
                    top = screen.Bounds.Y;
                    left = screen.Bounds.X;
                    right = screen.Bounds.Right;
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
                        if (!abWindow.requiresScreenEdge)
                        {
                            abd.rc.top = top + getAppBarEdgeWindowsHeight((ABEdge)abd.uEdge);
                        }
                        else
                            abd.rc.top = top;
                        abd.rc.bottom = abd.rc.top + sHeight;
                    }
                    else
                    {
                        if (!abWindow.requiresScreenEdge)
                        {
                            abd.rc.bottom = bottom + getAppBarEdgeWindowsHeight((ABEdge)abd.uEdge);
                        }
                        else
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

                // check if new coords
                bool isSameCoords = false;
                if (!isCreate) isSameCoords = abd.rc.top == (abWindow.Top * abWindow.dpiScale) && abd.rc.left == (abWindow.Left * abWindow.dpiScale) && abd.rc.bottom == (abWindow.Top * abWindow.dpiScale) + sHeight && abd.rc.right == (abWindow.Left * abWindow.dpiScale) + sWidth;
                
                if (!isSameCoords)
                {
                    CairoLogger.Instance.Debug(string.Format("AppBarHelper: {0} changing position (TxLxBxR) to {1}x{2}x{3}x{4} from {5}x{6}x{7}x{8}", abWindow.Name, abd.rc.top, abd.rc.left, abd.rc.bottom, abd.rc.right, (abWindow.Top * abWindow.dpiScale), (abWindow.Left * abWindow.dpiScale), (abWindow.Top * abWindow.dpiScale) + sHeight, (abWindow.Left * abWindow.dpiScale) + sWidth));
                    abWindow.setAppBarPosition(abd.rc);
                }

                abWindow.afterAppBarPos(isSameCoords, abd.rc);

                if (abd.rc.bottom - abd.rc.top < sHeight)
                    ABSetPos(abWindow, screen, width, height, edge);
            }
        }

        private static int getAppBarEdgeWindowsHeight(ABEdge edge)
        {
            int edgeHeight = 0;

            foreach (MenuBar menuBar in WindowManager.Instance.MenuBarWindows)
            {
                if (menuBar.requiresScreenEdge && menuBar.appBarEdge == edge) edgeHeight += Convert.ToInt32(menuBar.Height);
            }

            foreach (Taskbar taskbar in WindowManager.Instance.TaskbarWindows)
            {
                if (taskbar.requiresScreenEdge && taskbar.appBarEdge == edge) edgeHeight += Convert.ToInt32(taskbar.Height);
            }

            return edgeHeight;
        }
    }
}
