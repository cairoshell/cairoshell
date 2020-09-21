using System.Runtime.InteropServices;
using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using System;
using System.Text;

namespace CairoDesktop.WindowsTray
{
    public class TrayService: ITrayService
    {
        private IconDataDelegate iconDataDelegate;
        private MenuBarSizeDelegate menubarSizeDelegate;
        private SystrayDelegate trayDelegate;
        private NativeMethods.WndProcDelegate wndProcDelegate;

        private IntPtr HwndTray;
        private IntPtr HwndNotify;
        private IntPtr hInstance = Marshal.GetHINSTANCE(typeof(TrayService).Module);

        #region Set callbacks
        public void SetSystrayCallback(SystrayDelegate theDelegate)
        {
            //InteropCalls.SetSystrayCallback(theDelegate);

            trayDelegate = theDelegate;
        }

        public void SetIconDataCallback(IconDataDelegate theDelegate)
        {
            //InteropCalls.SetIconDataCallback(theDelegate);

            iconDataDelegate = theDelegate;
        }

        public void SetMenuBarSizeCallback(MenuBarSizeDelegate theDelegate)
        {
            //InteropCalls.SetMenuBarSizeCallback(theDelegate);

            menubarSizeDelegate = theDelegate;
        }
        #endregion

        public IntPtr InitializeSystray()
        {
            //return InteropCalls.InitializeSystray(Interop.NativeMethods.GetSystemMetrics(0), (float) Interop.Shell.DpiScale);

            ShutdownSystray();

            wndProcDelegate = WndProc;

            RegisterTrayWnd();
            RegisterNotifyWnd();

            return HwndTray;
        }

        /// <summary>
        /// Starts the system tray listener (send the TaskbarCreated message).
        /// </summary>
        public void Run()
        {
            //InteropCalls.Run();

            if (HwndTray != IntPtr.Zero)
            {
                NativeMethods.SetWindowPos(HwndTray, IntPtr.Zero, 0, 0, 0, 0,
                    (int) NativeMethods.SetWindowPosFlags.SWP_NOMOVE |
                    (int) NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE |
                    (int) NativeMethods.SetWindowPosFlags.SWP_NOSIZE);

                int msg = NativeMethods.RegisterWindowMessage("TaskbarCreated");

                if (msg > 0)
                {
                    CairoLogger.Instance.Debug("TrayService: Sending TaskbarCreated message");
                    NativeMethods.SendNotifyMessage(NativeMethods.HWND_BROADCAST,
                        (uint) msg, UIntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        public void ShutdownSystray()
        {
            //InteropCalls.ShutdownSystray();

            if (HwndNotify != IntPtr.Zero)
            {
                NativeMethods.DestroyWindow(HwndNotify);
                NativeMethods.UnregisterClass("TrayNotifyWnd", hInstance);
                CairoLogger.Instance.Debug("TrayService: Unregistered TrayNotifyWnd");
            }

            if (HwndTray != IntPtr.Zero)
            {
                NativeMethods.DestroyWindow(HwndTray);
                NativeMethods.UnregisterClass("Shell_TrayWnd", hInstance);
                CairoLogger.Instance.Debug("TrayService: Unregistered Shell_TrayWnd");
            }
        }

        public IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch ((NativeMethods.WM)msg)
            {
                case NativeMethods.WM.COPYDATA:
                    if (lParam == IntPtr.Zero)
                    {
                        CairoLogger.Instance.Debug("TrayService: CopyData is null");
                        break;
                    }

                    NativeMethods.COPYDATASTRUCT copyData =
                        (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.COPYDATASTRUCT));

                    switch ((int)copyData.dwData)
                    {
                        case 0:
                            // AppBar message
                            if (Marshal.SizeOf(typeof(NativeMethods.APPBARMSGDATAV3)) == copyData.cbData)
                            {
                                NativeMethods.APPBARMSGDATAV3 amd = (NativeMethods.APPBARMSGDATAV3)Marshal.PtrToStructure(copyData.lpData,
                                    typeof(NativeMethods.APPBARMSGDATAV3));

                                if (Marshal.SizeOf(typeof(NativeMethods.APPBARDATAV2)) != amd.abd.cbSize)
                                {
                                    CairoLogger.Instance.Debug("TrayService: Size incorrect for APPBARMSGDATAV3");
                                    break;
                                }

                                if (AppBarMessageAction(amd))
                                {
                                    return (IntPtr) 1;
                                }
                            }
                            else
                            {
                                CairoLogger.Instance.Debug("TrayService: AppBar message received, but with unknown size");
                            }
                            break;
                        case 1:
                            NativeMethods.SHELLTRAYDATA trayData =
                                (NativeMethods.SHELLTRAYDATA)Marshal.PtrToStructure(copyData.lpData,
                                    typeof(NativeMethods.SHELLTRAYDATA));
                            if (trayDelegate != null)
                            {
                                if (trayDelegate(trayData.dwMessage, trayData.nid))
                                {
                                    return (IntPtr) 1;
                                }
                                
                                CairoLogger.Instance.Debug("TrayService: Ignored notify icon message");
                            }
                            else
                            {
                                CairoLogger.Instance.Info("TrayService: TrayDelegate is null");
                            }
                            break;
                        case 3:
                            NativeMethods.WINNOTIFYICONIDENTIFIER iconData =
                                (NativeMethods.WINNOTIFYICONIDENTIFIER)Marshal.PtrToStructure(copyData.lpData,
                                    typeof(NativeMethods.WINNOTIFYICONIDENTIFIER));

                            if (iconDataDelegate != null)
                            {
                                return iconDataDelegate(iconData.dwMessage, iconData.hWnd, iconData.uID,
                                    iconData.guidItem);
                            }

                            CairoLogger.Instance.Info("TrayService: IconDataDelegate is null");
                            break;
                    }
                    
                    break;
                case NativeMethods.WM.WINDOWPOSCHANGED:
                    NativeMethods.WINDOWPOS wpos = (NativeMethods.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(NativeMethods.WINDOWPOS));

                    if ((wpos.flags & NativeMethods.SetWindowPosFlags.SWP_SHOWWINDOW) != 0)
                    {
                        NativeMethods.SetWindowLong(HwndTray, NativeMethods.GWL_STYLE,
                            NativeMethods.GetWindowLong(HwndTray, NativeMethods.GWL_STYLE) &
                            ~(int)NativeMethods.WindowStyles.WS_VISIBLE);

                        CairoLogger.Instance.Debug("TrayService: Shell_TrayWnd became visible; hiding");
                    }
                    break;
            }

            if (msg == (int)NativeMethods.WM.COPYDATA || msg == (int)NativeMethods.WM.ACTIVATEAPP)
            {
                IntPtr fwdResult = IntPtr.Zero;

                NativeMethods.EnumWindows((enumHwnd, enumLParam) =>
                {
                    if (enumHwnd != HwndTray && enumHwnd != hWnd)
                    {
                        StringBuilder className = new StringBuilder(256);
                        NativeMethods.GetClassName(enumHwnd, className, 256);

                        if (className.ToString() == "Shell_TrayWnd")
                        {
                            CairoLogger.Instance.Debug($"TrayService: Forwarding message {msg} to other Shell_TrayWnd");
                            fwdResult = NativeMethods.SendMessage(enumHwnd, msg, wParam, lParam);
                        }
                    }

                    return true;
                }, 0);

                return fwdResult;
            }

            return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private bool AppBarMessageAction(NativeMethods.APPBARMSGDATAV3 amd)
        {
            // only handle ABM_GETTASKBARPOS, send other AppBar messages to default handler
            switch ((NativeMethods.ABMsg)amd.dwMessage)
            {
                case NativeMethods.ABMsg.ABM_GETTASKBARPOS:
                    IntPtr hShared = NativeMethods.SHLockShared((IntPtr)amd.hSharedMemory, (uint)amd.dwSourceProcessId);
                    NativeMethods.APPBARDATAV2 abd = (NativeMethods.APPBARDATAV2)Marshal.PtrToStructure(hShared, typeof(NativeMethods.APPBARDATAV2));
                    if (menubarSizeDelegate != null)
                    {
                        NativeMethods.MenuBarSizeData msd = menubarSizeDelegate();
                        abd.rc = msd.rc;
                        abd.uEdge = (uint) msd.edge;
                    }
                    else
                    {
                        CairoLogger.Instance.Info("TrayService: MenuBarSizeDelegate is null");
                    }
                    Marshal.StructureToPtr(abd, hShared, false);
                    NativeMethods.SHUnlockShared(hShared);
                    CairoLogger.Instance.Debug("TrayService: Responded to ABM_GETTASKBARPOS");
                    return true;
            }
            return false;
        }

        #region Window creation helpers
        private ushort RegisterWndClass(string name)
        {
            NativeMethods.WNDCLASS newClass = new NativeMethods.WNDCLASS
            {
                lpszClassName = name,
                hInstance = hInstance,
                style = 0x8,
                lpfnWndProc = wndProcDelegate
            };

            return NativeMethods.RegisterClass(ref newClass);
        }

        private void RegisterTrayWnd()
        {
            ushort trayClassReg = RegisterWndClass("Shell_TrayWnd");
            if (trayClassReg == 0)
            {
                CairoLogger.Instance.Info($"TrayService: Error registering Shell_TrayWnd class ({Marshal.GetLastWin32Error()})");
            }

            HwndTray = NativeMethods.CreateWindowEx(
                NativeMethods.ExtendedWindowStyles.WS_EX_TOPMOST |
                NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW, trayClassReg, "",
                NativeMethods.WindowStyles.WS_POPUP | NativeMethods.WindowStyles.WS_CLIPCHILDREN |
                NativeMethods.WindowStyles.WS_CLIPSIBLINGS, 0, 0, NativeMethods.GetSystemMetrics(0),
                (int)(23 * Shell.DpiScale), IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (HwndTray == IntPtr.Zero)
            {
                CairoLogger.Instance.Info($"TrayService: Error creating Shell_TrayWnd window ({Marshal.GetLastWin32Error()})");
            }
            else
            {
                CairoLogger.Instance.Debug("TrayService: Created Shell_TrayWnd");
            }
        }

        private void RegisterNotifyWnd()
        {
            ushort trayNotifyClassReg = RegisterWndClass("TrayNotifyWnd");
            if (trayNotifyClassReg == 0)
            {
                CairoLogger.Instance.Info($"TrayService: Error registering TrayNotifyWnd class ({Marshal.GetLastWin32Error()})");
            }

            HwndNotify = NativeMethods.CreateWindowEx(0, trayNotifyClassReg, null,
                NativeMethods.WindowStyles.WS_CHILD | NativeMethods.WindowStyles.WS_CLIPCHILDREN |
                NativeMethods.WindowStyles.WS_CLIPSIBLINGS, 0, 0, NativeMethods.GetSystemMetrics(0),
                (int)(23 * Shell.DpiScale), HwndTray, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (HwndNotify == IntPtr.Zero)
            {
                CairoLogger.Instance.Info($"TrayService: Error creating TrayNotifyWnd window ({Marshal.GetLastWin32Error()})");
            }
            else
            {
                CairoLogger.Instance.Debug("TrayService: Created TrayNotifyWnd");
            }
        }
        #endregion
    }
}