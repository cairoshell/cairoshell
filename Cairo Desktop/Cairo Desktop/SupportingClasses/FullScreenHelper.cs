using CairoDesktop.Common.Logging;
using CairoDesktop.Interop;
using CairoDesktop.WindowsTasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

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

        private WindowsTasksService tasksSvc = WindowsTasksService.Instance;

        public ObservableCollection<FullScreenApp> FullScreenApps = new ObservableCollection<FullScreenApp>();

        private FullScreenHelper()
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
            NativeMethods.UnhookWinEvent(resizeEventHook);
            tasksSvc.Windows.CollectionChanged -= Windows_CollectionChanged;
        }

        public class FullScreenApp
        {
            public IntPtr hWnd;
            public Screen screen;
            public NativeMethods.RECT rect;
        }
    }
}
