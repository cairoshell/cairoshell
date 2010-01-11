namespace SHAppBarMessage1.Common
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using SHAppBarMessage1.Win32;
    using System.Diagnostics;

    public static class SHAppBarMessageHelper
    {
        //This file is from before r416
        public static int RegisterBar(IntPtr handle, Size size)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = handle;
            if (!isBarRegistered)
            {
                uCallBack = NativeMethods.RegisterWindowMessage("AppBarMessage" + Guid.NewGuid().ToString());
                abd.uCallbackMessage = uCallBack;

                uint ret = NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_NEW, ref abd);
                isBarRegistered = true;

                ABSetPos(handle, size);
            }
            else
            {
                NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_REMOVE, ref abd);
                isBarRegistered = false;
            }

            return uCallBack;
        }

        private static bool isBarRegistered = false;

        private static int uCallBack = 0;

        public static void ABSetPos(IntPtr handle, Size size)
        {
            NativeMethods.APPBARDATA abd = new NativeMethods.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = handle;
            abd.uEdge = (int)NativeMethods.ABEdge.ABE_TOP;

            if (abd.uEdge == (int)NativeMethods.ABEdge.ABE_LEFT || abd.uEdge == (int)NativeMethods.ABEdge.ABE_RIGHT)
            {
                abd.rc.top = 0;
                abd.rc.bottom = PrimaryMonitorSize.Height;
                if (abd.uEdge == (int)NativeMethods.ABEdge.ABE_LEFT)
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
                if (abd.uEdge == (int)NativeMethods.ABEdge.ABE_TOP)
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

            switch (abd.uEdge)
            {
                case (int)NativeMethods.ABEdge.ABE_LEFT:
                    abd.rc.right = abd.rc.left + size.Width;
                    break;
                case (int)NativeMethods.ABEdge.ABE_RIGHT:
                    abd.rc.left = abd.rc.right - size.Width;
                    break;
                case (int)NativeMethods.ABEdge.ABE_TOP:
                    abd.rc.bottom = abd.rc.top + size.Height;
                    break;
                case (int)NativeMethods.ABEdge.ABE_BOTTOM:
                    abd.rc.top = abd.rc.bottom - size.Height;
                    break;
            }

            NativeMethods.SHAppBarMessage((int)NativeMethods.ABMsg.ABM_SETPOS, ref abd);
            Trace.WriteLineIf(abd.uEdge == (int)NativeMethods.ABEdge.ABE_TOP, "*** TOP EDGE - CX is: " + (abd.rc.bottom - abd.rc.top).ToString());
            NativeMethods.MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, true);
        }

        private static Size PrimaryMonitorSize
        {
            get
            {
                return new Size(NativeMethods.GetSystemMetrics(0), NativeMethods.GetSystemMetrics(1));
            }
        }
    }
}
