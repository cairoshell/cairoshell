using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Forms;
using System.Windows.Threading;

namespace CairoDesktop.SupportingClasses
{
    public class FullScreenHelper: IDisposable
    {
        private static FullScreenHelper instance;

        public static FullScreenHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FullScreenHelper();
                }

                return instance;
            }
        }

        private DispatcherTimer fullscreenCheck;

        public ObservableCollection<FullScreenApp> FullScreenApps = new ObservableCollection<FullScreenApp>();

        private FullScreenHelper()
        {
            fullscreenCheck = new DispatcherTimer(DispatcherPriority.Background, System.Windows.Application.Current.Dispatcher);
            fullscreenCheck.Interval = new TimeSpan(0, 0, 0, 0, 100);
            fullscreenCheck.Tick += FullscreenCheck_Tick;
            fullscreenCheck.Start();
        }

        private void FullscreenCheck_Tick(object sender, EventArgs e)
        {
            IntPtr hWnd = NativeMethods.GetForegroundWindow();

            List<FullScreenApp> removeApps = new List<FullScreenApp>();
            bool skipAdd = false;

            // first check if this window is already in our list. if so, remove it if necessary
            foreach (FullScreenApp app in FullScreenApps)
            {
                FullScreenApp appCurrentState = getFullScreenApp(app.hWnd);

                if (app.hWnd == hWnd && appCurrentState != null)
                {
                    // this window, still same screen, do nothing
                    skipAdd = true;
                    continue;
                }

                if (appCurrentState == null || app.screen != appCurrentState.screen)
                {
                    removeApps.Add(app);
                }
            }

            // remove any changed windows we found
            if (removeApps.Count > 0)
            {
                CairoLogger.Instance.Debug("Removing full screen app(s)");
                foreach (FullScreenApp existingApp in removeApps)
                {
                    FullScreenApps.Remove(existingApp);
                }
            }

            // check if this is a new full screen app
            if (!skipAdd)
            {
                FullScreenApp appNew = getFullScreenApp(hWnd);
                if (appNew != null)
                {
                    CairoLogger.Instance.Debug("Adding full screen app");
                    FullScreenApps.Add(appNew);
                }
            }
        }

        private FullScreenApp getFullScreenApp(IntPtr hWnd)
        {
            NativeMethods.Rect rect;
            NativeMethods.GetWindowRect(hWnd, out rect);

            // check if this is a full screen app
            foreach (Screen screen in WindowManager.Instance.ScreenState)
            {
                if (rect.Top == screen.Bounds.Top && rect.Left == screen.Bounds.Left && rect.Bottom == screen.Bounds.Bottom && rect.Right == screen.Bounds.Right)
                {
                    // make sure this is not us
                    uint hwndProcId;
                    NativeMethods.GetWindowThreadProcessId(hWnd, out hwndProcId);
                    if (hwndProcId == NativeMethods.GetCurrentProcessId())
                    {
                        return null;
                    }

                    // make sure this is fullscreenable
                    int style = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_STYLE);
                    if (((int)NativeMethods.WindowStyles.WS_CAPTION & style) == (int)NativeMethods.WindowStyles.WS_CAPTION || ((int)NativeMethods.WindowStyles.WS_THICKFRAME & style) == (int)NativeMethods.WindowStyles.WS_THICKFRAME || !NativeMethods.IsWindow(hWnd) || !NativeMethods.IsWindowVisible(hWnd) || NativeMethods.IsIconic(hWnd))
                    {
                        return null;
                    }

                    // make sure this is not the shell desktop
                    StringBuilder cName = new StringBuilder(256);
                    NativeMethods.GetClassName(hWnd, cName, cName.Capacity);
                    if (cName.ToString() == "Progman" || cName.ToString() == "WorkerW")
                    {
                        return null;
                    }

                    // make sure this is not a cloaked window
                    if (Shell.IsWindows8OrBetter)
                    {
                        uint cloaked;
                        int cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(uint));
                        NativeMethods.DwmGetWindowAttribute(hWnd, NativeMethods.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out cloaked, cbSize);

                        if (cloaked > 0)
                        {
                            return null;
                        }
                    }

                    // this is a full screen app on this screen
                    return new FullScreenApp { hWnd = hWnd, screen = screen, rect = rect };
                }
            }

            return null;
        }

        public void Dispose()
        {
            fullscreenCheck.Stop();
        }

        public class FullScreenApp
        {
            public IntPtr hWnd;
            public Screen screen;
            public NativeMethods.Rect rect;
        }
    }
}
