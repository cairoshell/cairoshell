using System;
using System.Drawing;
using System.Runtime.InteropServices;
using CairoDesktop.Interop;
using System.Diagnostics;
using System.Collections.Generic;

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

        public static int RegisterBar(IntPtr handle, Size size, ABEdge edge = ABEdge.ABE_TOP)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = handle;

            if (!appBars.Contains(handle))
            {
                //uCallBack = NativeMethods.RegisterWindowMessage("AppBarMessage" + Guid.NewGuid().ToString());
                uCallBack = NativeMethods.RegisterWindowMessage("AppBarMessage");
                abd.uCallbackMessage = uCallBack;

                uint ret = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_NEW, ref abd);
                appBars.Add(handle);

                ABSetPos(handle, size, edge);
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

        public static void SetWinTaskbarState(WinTaskbarState state)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = (int)Marshal.SizeOf(abd);
            abd.hWnd = NativeMethods.FindWindow("System_TrayWnd");
            abd.lParam = (IntPtr)state;
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_SETSTATE, ref abd);
        }

        public static void ABSetPos(IntPtr handle, Size size, ABEdge edge)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = handle;
            abd.uEdge = (int)edge;

            if (abd.uEdge == (int)ABEdge.ABE_LEFT || abd.uEdge == (int)ABEdge.ABE_RIGHT)
            {
                abd.rc.top = 0;
                abd.rc.bottom = PrimaryMonitorSize.Height;
                if (abd.uEdge == (int)ABEdge.ABE_LEFT)
                {
                    abd.rc.left = 0;
                    abd.rc.right = size.Width;
                }
                else
                {
                    abd.rc.right = PrimaryMonitorSize.Width;
                    abd.rc.left = abd.rc.right - size.Width;
                }

            }
            else
            {
                abd.rc.left = 0;
                abd.rc.right = PrimaryMonitorSize.Width;
                if (abd.uEdge == (int)ABEdge.ABE_TOP)
                {
                    abd.rc.top = 0;
                    abd.rc.bottom = size.Height;
                }
                else
                {
                    abd.rc.bottom = PrimaryMonitorSize.Height;
                    abd.rc.top = abd.rc.bottom - size.Height;
                }
            }

            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_QUERYPOS, ref abd);
            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_QUERYPOS, ref abd);

            switch (abd.uEdge)
            {
                case (int)ABEdge.ABE_LEFT:
                    abd.rc.right = abd.rc.left + size.Width;
                    break;
                case (int)ABEdge.ABE_RIGHT:
                    abd.rc.left = abd.rc.right - size.Width;
                    break;
                case (int)ABEdge.ABE_TOP:
                    abd.rc.bottom = abd.rc.top + size.Height;
                    break;
                case (int)ABEdge.ABE_BOTTOM:
                    abd.rc.top = abd.rc.bottom - size.Height;
                    break;
            }

            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_SETPOS, ref abd);

            // tracing
            Trace.WriteLineIf(abd.uEdge == (int)ABEdge.ABE_TOP, "Top AppBar height is " + (abd.rc.bottom - abd.rc.top).ToString());
            Trace.WriteLineIf(abd.uEdge == (int)ABEdge.ABE_BOTTOM, "Bottom AppBar height is " + (abd.rc.bottom - abd.rc.top).ToString());

            NativeMethods.MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
        }

        public static Size PrimaryMonitorSize
        {
            get
            {
                return new Size(NativeMethods.GetSystemMetrics(0), NativeMethods.GetSystemMetrics(1));
            }
        }
    }
}
