using System;
using System.Drawing;
using System.Runtime.InteropServices;
using CairoDesktop.Interop;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using CairoDesktop.Configuration;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

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

        public static int RegisterBar(Window abWindow, double width, double height, ABEdge edge = ABEdge.ABE_TOP)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
            IntPtr handle = new WindowInteropHelper(abWindow).Handle;
            abd.hWnd = handle;

            if (!appBars.Contains(handle))
            {
                uCallBack = NativeMethods.RegisterWindowMessage("AppBarMessage");
                abd.uCallbackMessage = uCallBack;

                uint ret = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_NEW, ref abd);
                appBars.Add(handle);
                Trace.WriteLine("Created AppBar for handle " + handle.ToString());

                ABSetPos(abWindow, width, height, edge);
            }
            else
            {
                NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_REMOVE, ref abd);
                appBars.Remove(handle);
                Trace.WriteLine("Removed AppBar for handle " + handle.ToString());

                return 0;
            }
            
            return uCallBack;
        }

        private static List<IntPtr> appBars = new List<IntPtr>();

        private static int uCallBack = 0;

        public static void SetWinTaskbarPos(int swp)
        {
            IntPtr taskbarHwnd = NativeMethods.FindWindow("Shell_TrayWnd", "");
            IntPtr startButtonHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
            NativeMethods.SetWindowPos(taskbarHwnd, IntPtr.Zero, 0, 0, 0, 0, swp);
            NativeMethods.SetWindowPos(startButtonHwnd, IntPtr.Zero, 0, 0, 0, 0, swp);

            // adjust secondary taskbars for multi-mon
            if (swp == NativeMethods.SWP_HIDEWINDOW)
                SetSecondaryTaskbarVisibility(NativeMethods.WindowShowStyle.Hide);
            else
                SetSecondaryTaskbarVisibility(NativeMethods.WindowShowStyle.ShowNoActivate);
        }

        public static void SetWinTaskbarState(WinTaskbarState state)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
            abd.hWnd = NativeMethods.FindWindow("System_TrayWnd");
            abd.lParam = (IntPtr)state;
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_SETSTATE, ref abd);
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
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_ACTIVATE, ref abd);

            // apparently the secondary taskbars like to pop up when app bars change
            if (Settings.WindowsTaskbarMode == 0)
                SetSecondaryTaskbarVisibility(NativeMethods.WindowShowStyle.Hide);
        }

        public static void AppBarWindowPosChanged(IntPtr hwnd)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
            abd.hWnd = hwnd;
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_WINDOWPOSCHANGED, ref abd);
        }

        public static void ABSetPos(Window abWindow, double width, double height, ABEdge edge)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(typeof(NativeMethods.APPBARDATA));
            IntPtr handle = new WindowInteropHelper(abWindow).Handle;
            abd.hWnd = handle;
            abd.uEdge = (int)edge;
            int sWidth;
            int sHeight;

            // adjust size for dpi
            Shell.TransformToPixels(width, height, out sWidth, out sHeight);

            if (abd.uEdge == (int)ABEdge.ABE_LEFT || abd.uEdge == (int)ABEdge.ABE_RIGHT)
            {
                abd.rc.top = 0;
                abd.rc.bottom = SystemInformation.WorkingArea.Bottom;
                if (abd.uEdge == (int)ABEdge.ABE_LEFT)
                {
                    abd.rc.left = SystemInformation.WorkingArea.Left;
                    abd.rc.right = abd.rc.left + sWidth;
                }
                else
                {
                    abd.rc.right = SystemInformation.WorkingArea.Right;
                    abd.rc.left = abd.rc.right - sWidth;
                }

            }
            else
            {
                abd.rc.left = SystemInformation.WorkingArea.Left;
                abd.rc.right = SystemInformation.WorkingArea.Right;
                if (abd.uEdge == (int)ABEdge.ABE_TOP)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = abd.rc.top + sHeight;
                }
                else
                {
                    abd.rc.bottom = SystemInformation.WorkingArea.Bottom;
                    abd.rc.top = abd.rc.bottom - sHeight;
                }
            }

            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_QUERYPOS, ref abd);

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

            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_SETPOS, ref abd);

            // tracing
            int h = abd.rc.bottom - abd.rc.top;
            Trace.WriteLineIf(abd.uEdge == (int)ABEdge.ABE_TOP, "Top AppBar height is " + h.ToString());
            Trace.WriteLineIf(abd.uEdge == (int)ABEdge.ABE_BOTTOM, "Bottom AppBar height is " + h.ToString());

            abWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new ResizeDelegate(DoResize), abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top);

            if (h < sHeight)
                ABSetPos(abWindow, width, height, edge);
        }

        private delegate void ResizeDelegate(IntPtr hWnd, int x, int y, int cx, int cy);
        private static void DoResize(IntPtr hWnd, int x, int y, int cx, int cy)
        {
            NativeMethods.MoveWindow(hWnd, x, y, cx, cy, true);
        }

        public static System.Drawing.Size PrimaryMonitorSize
        {
            get
            {
                return new System.Drawing.Size(Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth), Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight));
            }
        }

        public static System.Drawing.Size PrimaryMonitorDeviceSize
        {
            get
            {
                return new System.Drawing.Size(NativeMethods.GetSystemMetrics(0), NativeMethods.GetSystemMetrics(1));
            }
        }

        private static NativeMethods.RECT oldWorkArea;
        
        public static void SetWorkArea()
        {
            // Save current Working Area size
            oldWorkArea.left = SystemInformation.WorkingArea.Left;
            oldWorkArea.top = SystemInformation.WorkingArea.Top;
            oldWorkArea.right = SystemInformation.WorkingArea.Right;
            oldWorkArea.bottom = SystemInformation.WorkingArea.Bottom;

            NativeMethods.RECT rc;
            rc.left = SystemInformation.VirtualScreen.Left;
            rc.top = SystemInformation.VirtualScreen.Top + 23; // allocate menu bar space
            rc.right = SystemInformation.VirtualScreen.Right;

            // only allocate space for taskbar if enabled
            if (Settings.EnableTaskbar)
                rc.bottom = SystemInformation.VirtualScreen.Bottom - 29;
            else
                rc.bottom = SystemInformation.VirtualScreen.Bottom;

            NativeMethods.SystemParametersInfo((int)NativeMethods.SPI.SPI_SETWORKAREA, 0, ref rc, (1 | 2));
        }
        
        public static void ResetWorkArea()
        {
            NativeMethods.SystemParametersInfo((int)NativeMethods.SPI.SPI_SETWORKAREA, 0, ref oldWorkArea, (1 | 2));
        }
    }
}
