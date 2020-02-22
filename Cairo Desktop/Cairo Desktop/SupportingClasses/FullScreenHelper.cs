using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using CairoDesktop.WindowsTasks;
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

        private static IntPtr resizeEventHook;
        private readonly NativeMethods.WinEventProc resizeEventProc;
        private DispatcherTimer fullscreenCheck;

        private bool useNewBehavior = true;

        private WindowsTasksService tasksSvc = WindowsTasksService.Instance;

        public ObservableCollection<FullScreenApp> FullScreenApps = new ObservableCollection<FullScreenApp>();

        private FullScreenHelper()
        {
            if (!useNewBehavior)
            {
                resizeEventProc = ResizeEventCallback;
                tasksSvc.Windows.CollectionChanged += Windows_CollectionChanged;

                if (resizeEventHook == IntPtr.Zero)
                {
                    resizeEventHook = NativeMethods.SetWinEventHook(
                        NativeMethods.EVENT_OBJECT_LOCATIONCHANGE,
                        NativeMethods.EVENT_OBJECT_LOCATIONCHANGE,
                        IntPtr.Zero,
                        resizeEventProc,
                        0,
                        0,
                        NativeMethods.WINEVENT_OUTOFCONTEXT | NativeMethods.WINEVENT_SKIPOWNPROCESS);
                }
            }
            else
            {
                fullscreenCheck = new DispatcherTimer(DispatcherPriority.Background, System.Windows.Application.Current.Dispatcher);
                fullscreenCheck.Interval = new TimeSpan(0, 0, 0, 0, 100);
                fullscreenCheck.Tick += FullscreenCheck_Tick;
                fullscreenCheck.Start();
            }
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
                CairoLogger.Instance.Debug("NEW Removing full screen apps");
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
                    CairoLogger.Instance.Debug("NEW Adding full screen app");
                    FullScreenApps.Add(appNew);
                }
            }
        }

        private FullScreenApp getFullScreenApp(IntPtr hWnd)
        {
            NativeMethods.RECT rect;
            NativeMethods.GetWindowRect(hWnd, out rect);

            // check if this is a full screen app
            foreach (Screen screen in Startup.screenState)
            {
                if (rect.top == screen.Bounds.Top && rect.left == screen.Bounds.Left && rect.bottom == screen.Bounds.Bottom && rect.right == screen.Bounds.Right)
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

                    // this is a full screen app on this screen
                    return new FullScreenApp { hWnd = hWnd, screen = screen, rect = rect };
                }
            }

            return null;
        }

        private void Windows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<FullScreenApp> toRemove = new List<FullScreenApp>();

            // if a window that we see as full screen has gone away, lets remove it from our collection
            foreach (FullScreenApp app in FullScreenApps)
            {
                bool exists = false;

                foreach (ApplicationWindow window in tasksSvc.Windows)
                {
                    if (app.hWnd == window.Handle)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    toRemove.Add(app);
                }
            }

            foreach (FullScreenApp app in toRemove)
            {
                CairoLogger.Instance.Debug("Removing closed full screen app");
                FullScreenApps.Remove(app);
            }
        }

        private void ResizeEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hWnd != IntPtr.Zero && idObject == 0 && idChild == 0)
            {
                NativeMethods.RECT rect;
                NativeMethods.GetWindowRect(hWnd, out rect);
                FullScreenApp existingApp = null;
                ApplicationWindow window = new ApplicationWindow(hWnd, tasksSvc);

                if (tasksSvc.Windows.Contains(window))
                {
                    // first check if this window is already in our list. if so, remove it if necessary
                    foreach (FullScreenApp app in FullScreenApps)
                    {
                        if (hWnd == app.hWnd)
                        {
                            if (rect.top == app.rect.top && rect.left == app.rect.left && rect.bottom == app.rect.bottom && rect.right == app.rect.right)
                            {
                                // no change, do nothing
                                return;
                            }
                            else
                            {
                                // there was a change
                                existingApp = app;
                            }
                            break;
                        }
                    }

                    // remove any changed windows we found
                    if (existingApp != null)
                    {
                        CairoLogger.Instance.Debug("Removing full screen app");
                        FullScreenApps.Remove(existingApp);
                        return;
                    }

                    // check if this is a new full screen app
                    foreach (Screen screen in Startup.screenState)
                    {
                        if (rect.top == screen.Bounds.Top && rect.left == screen.Bounds.Left && rect.bottom == screen.Bounds.Bottom && rect.right == screen.Bounds.Right)
                        {
                            // this is a full screen app on this screen
                            CairoLogger.Instance.Debug("Adding full screen app");
                            FullScreenApps.Add(new FullScreenApp { hWnd = hWnd, screen = screen, rect = rect });
                            break;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!useNewBehavior)
            {
                NativeMethods.UnhookWinEvent(resizeEventHook);
                tasksSvc.Windows.CollectionChanged -= Windows_CollectionChanged;
            }
            else
            {
                fullscreenCheck.Stop();
            }
        }

        public class FullScreenApp
        {
            public IntPtr hWnd;
            public Screen screen;
            public NativeMethods.RECT rect;
        }
    }
}
