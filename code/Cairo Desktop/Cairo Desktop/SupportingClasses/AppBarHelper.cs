using System;
using System.Drawing;
using System.Runtime.InteropServices;
using CairoDesktop.Interop;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using CairoDesktop.Configuration;

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

        public static int RegisterBar(IntPtr handle, double width, double height, ABEdge edge = ABEdge.ABE_TOP)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = handle;

            if (!appBars.Contains(handle))
            {
                uCallBack = NativeMethods.RegisterWindowMessage("AppBarMessage" + Guid.NewGuid().ToString());
                abd.uCallbackMessage = uCallBack;

                uint ret = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_NEW, ref abd);
                appBars.Add(handle);

                ABSetPos(handle, width, height, edge);
            }
            else
            {
                NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_REMOVE, ref abd);
                appBars.Remove(handle);
            }

            return uCallBack;
        }

        private static List<IntPtr> appBars = new List<IntPtr>();

        private static int uCallBack = 0;

        public static void SetWinTaskbarPos(int swp)
        {
            IntPtr taskbarHwnd = NativeMethods.FindWindow("Shell_traywnd", "");
            IntPtr startButtonHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
            NativeMethods.SetWindowPos(taskbarHwnd, IntPtr.Zero, 0, 0, 0, 0, swp);
            NativeMethods.SetWindowPos(startButtonHwnd, IntPtr.Zero, 0, 0, 0, 0, swp);
        }

        public static void SetWinTaskbarState(WinTaskbarState state)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(abd);
            abd.hWnd = NativeMethods.FindWindow("System_TrayWnd");
            abd.lParam = (IntPtr)state;
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_SETSTATE, ref abd);
        }

        public static void AppBarActivate(IntPtr hwnd)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(abd);
            abd.hWnd = hwnd;
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_ACTIVATE, ref abd);
        }

        public static void AppBarWindowPosChanged(IntPtr hwnd)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(abd);
            abd.hWnd = hwnd;
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_WINDOWPOSCHANGED, ref abd);
        }

        public static void ABSetPos(IntPtr handle, double width, double height, ABEdge edge)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = handle;
            abd.uEdge = (int)edge;
            int sWidth;
            int sHeight;

            // adjust size for dpi
            TransformToPixels(width, height, out sWidth, out sHeight);

            if (abd.uEdge == (int)ABEdge.ABE_LEFT || abd.uEdge == (int)ABEdge.ABE_RIGHT)
            {
                abd.rc.top = 0;
                abd.rc.bottom = PrimaryMonitorDeviceSize.Height;
                if (abd.uEdge == (int)ABEdge.ABE_LEFT)
                {
                    abd.rc.left = 0;
                    abd.rc.right = sWidth;
                }
                else
                {
                    abd.rc.right = PrimaryMonitorDeviceSize.Width;
                    abd.rc.left = abd.rc.right - sWidth;
                }

            }
            else
            {
                abd.rc.left = 0;
                abd.rc.right = PrimaryMonitorDeviceSize.Width;
                if (abd.uEdge == (int)ABEdge.ABE_TOP)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = sHeight;
                }
                else
                {
                    abd.rc.bottom = PrimaryMonitorDeviceSize.Height;
                    abd.rc.top = abd.rc.bottom - sHeight;
                }
            }

            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_QUERYPOS, ref abd);

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

            NativeMethods.MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);

            if (h < sHeight)
                ABSetPos(handle, width, height, edge);
        }

        public static Size PrimaryMonitorSize
        {
            get
            {
                return new Size(Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth), Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight));
            }
        }

        public static Size PrimaryMonitorDeviceSize
        {
            get
            {
                return new Size(NativeMethods.GetSystemMetrics(0), NativeMethods.GetSystemMetrics(1));
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

        /// <summary>
        /// Transforms device independent units (1/96 of an inch)
        /// to pixels
        /// </summary>
        /// <param name="unitX">a device independent unit value X</param>
        /// <param name="unitY">a device independent unit value Y</param>
        /// <param name="pixelX">returns the X value in pixels</param>
        /// <param name="pixelY">returns the Y value in pixels</param>
        public static void TransformToPixels(double unitX, double unitY, out int pixelX, out int pixelY)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                pixelX = (int)((g.DpiX / 96) * unitX);
                pixelY = (int)((g.DpiY / 96) * unitY);
            }
        }

        public static void TransformFromPixels(double unitX, double unitY, out int pixelX, out int pixelY)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                pixelX = (int)(unitX / (g.DpiX / 96));
                pixelY = (int)(unitY / (g.DpiY / 96));
            }
        }
    }
}
