
namespace CairoDesktop.WindowsTray
{
    using System;

    /// <summary>
    /// The wrapper class the for C++ Windows Hooks library.
    /// </summary>
    public class WindowsHooksWrapper : IWindowsHooksWrapper
    {
        //private SystrayDelegate trayDelegate;
        //private WndProcDelegate wndProcDelegate;

        /// <summary>
        /// Sets the delegate for the system tray callback.
        /// </summary>
        /// <param name="theDelegate">The system tray callback delegate.</param>
        public void SetSystrayCallback(SystrayDelegate theDelegate)
        {
            InteropCalls.SetSystrayCallback(theDelegate);

            //trayDelegate = theDelegate;
        }

        /// <summary>
        /// Sets the delegate for the icon data callback.
        /// </summary>
        /// <param name="theDelegate">The system tray callback delegate.</param>
        public void SetIconDataCallback(IconDataDelegate theDelegate)
        {
            InteropCalls.SetIconDataCallback(theDelegate);
        }

        /// <summary>
        /// Initializes the system tray hooks.
        /// </summary>
        public IntPtr InitializeSystray()
        {
            return InteropCalls.InitializeSystray(Interop.NativeMethods.GetSystemMetrics(0), (float)Interop.Shell.DpiScale);

            /*IntPtr hInstance = Marshal.GetHINSTANCE(typeof(WindowsHooksWrapper).Module);

            wndProcDelegate = WndProc;

            // create Shell_TrayWnd

            WNDCLASSEX trayClass = new WNDCLASSEX();
            trayClass.cbSize = Marshal.SizeOf(trayClass);
            trayClass.lpszClassName = "Shell_TrayWnd";
            trayClass.lpfnWndProc = wndProcDelegate;
            trayClass.style = 0x8;
            trayClass.hInstance = hInstance;
            UInt16 trayClassReg = RegisterClassEx(ref trayClass);
            if (trayClassReg == 0)
            {
                // error
                SingletonLogger.Instance.Info(Marshal.GetLastWin32Error());
            }

            IntPtr hWndTray = CreateWindowEx(WindowStylesEx.WS_EX_TOPMOST | WindowStylesEx.WS_EX_TOOLWINDOW, trayClassReg, "", WindowStyles.WS_POPUP, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            if(hWndTray == IntPtr.Zero)
            {
                // error
                SingletonLogger.Instance.Info(Marshal.GetLastWin32Error());
            }

            // create TrayNotifyWnd
            WNDCLASSEX trayNotifyClass = new WNDCLASSEX();
            trayNotifyClass.cbSize = Marshal.SizeOf(trayNotifyClass);
            trayNotifyClass.lpszClassName = "TrayNotifyWnd";
            trayNotifyClass.lpfnWndProc = wndProcDelegate;
            trayNotifyClass.style = 0x8;
            trayNotifyClass.hInstance = hInstance;
            UInt16 trayNotifyClassReg = RegisterClassEx(ref trayNotifyClass);
            if (trayNotifyClassReg == 0)
            {
                // error
                SingletonLogger.Instance.Info(Marshal.GetLastWin32Error());
            }

            IntPtr hWndNotify = CreateWindowEx(0, trayNotifyClassReg, null, WindowStyles.WS_CHILD, 0, 0, 0, 0, hWndTray, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (hWndNotify == IntPtr.Zero)
            {
                // error
                SingletonLogger.Instance.Info(Marshal.GetLastWin32Error());
            }*/
        }

        /// <summary>
        /// Starts the system tray listener (send the TaskbarCreated message).
        /// </summary>
        public void Run()
        {
            InteropCalls.Run();            
        }

        /// <summary>
        /// Shuts down the system tray hooks.
        /// </summary>
        public void ShutdownSystray()
        {
            InteropCalls.ShutdownSystray();
        }

        /*public IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            SingletonLogger.Instance.Info(msg);
            if (msg == WM_COPYDATA)
            {
                COPYDATASTRUCT copyData = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));

                if ((int)copyData.dwData == 1)
                {
                    NOTIFYICONDATA nicData = (NOTIFYICONDATA)Marshal.PtrToStructure(copyData.lpData, typeof(NOTIFYICONDATA));
                    uint TrayCmd = (byte)copyData.lpData;

                    trayDelegate(TrayCmd, nicData);
                }
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }*/
    }
}
