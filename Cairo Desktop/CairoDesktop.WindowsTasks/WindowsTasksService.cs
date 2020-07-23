using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Data;

using CairoDesktop.Common.Logging;
using static CairoDesktop.Interop.NativeMethods;

namespace CairoDesktop.WindowsTasks
{
    public class WindowsTasksService : DependencyObject, IDisposable
    {
        private NativeWindowEx _HookWin;
        private object _windowsLock = new object();

        private static int WM_SHELLHOOKMESSAGE = -1;
        private static int WM_TASKBARCREATEDMESSAGE = -1;
        private static int TASKBARBUTTONCREATEDMESSAGE = -1;

        private static IntPtr uncloakEventHook = IntPtr.Zero;
        private WinEventProc uncloakEventProc;

        public static WindowsTasksService Instance { get; } = new WindowsTasksService();

        private WindowsTasksService()
        {
            initialize();
        }

        private void initialize()
        {
            try
            {
                CairoLogger.Instance.Debug("Starting WindowsTasksService");

                // create window to receive task events
                _HookWin = new NativeWindowEx();
                _HookWin.CreateHandle(new CreateParams());

                // prevent other shells from working properly
                SetTaskmanWindow(_HookWin.Handle);

                // register to receive task events
                RegisterShellHookWindow(_HookWin.Handle);
                WM_SHELLHOOKMESSAGE = RegisterWindowMessage("SHELLHOOK");
                WM_TASKBARCREATEDMESSAGE = RegisterWindowMessage("TaskbarCreated");
                TASKBARBUTTONCREATEDMESSAGE = RegisterWindowMessage("TaskbarButtonCreated");
                _HookWin.MessageReceived += ShellWinProc;

                if (Interop.Shell.IsWindows8OrBetter)
                {
                    // set event hook for uncloak events
                    uncloakEventProc = UncloakEventCallback;

                    if (uncloakEventHook == IntPtr.Zero)
                    {
                        uncloakEventHook = SetWinEventHook(
                            EVENT_OBJECT_UNCLOAKED,
                            EVENT_OBJECT_UNCLOAKED,
                            IntPtr.Zero,
                            uncloakEventProc,
                            0,
                            0,
                            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
                    }
                }

                // set window for ITaskbarList
                setTaskbarListHwnd();

                // adjust minimize animation
                SetMinimizedMetrics();

                // prepare collections
                groupedWindows = CollectionViewSource.GetDefaultView(Windows);
                groupedWindows.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                groupedWindows.CollectionChanged += groupedWindows_Changed;
                groupedWindows.Filter = groupedWindows_Filter;

                if (groupedWindows is ICollectionViewLiveShaping taskbarItemsView)
                {
                    taskbarItemsView.IsLiveFiltering = true;
                    taskbarItemsView.LiveFilteringProperties.Add("ShowInTaskbar");
                    taskbarItemsView.IsLiveGrouping = true;
                    taskbarItemsView.LiveGroupingProperties.Add("Category");
                }

                // enumerate windows already opened
                EnumWindows(new CallBackPtr((hwnd, lParam) =>
                {
                    ApplicationWindow win = new ApplicationWindow(hwnd, this);

                    if (win.CanAddToTaskbar && win.ShowInTaskbar && !Windows.Contains(win))
                        Windows.Add(win);

                    return true;
                }), 0);

                // register for app grabber changes so that our app association is accurate
                AppGrabber.AppGrabber.Instance.CategoryList.CategoryChanged += CategoryList_CategoryChanged;
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Info("Unable to start WindowsTasksService: " + ex.Message);
            }
        }

        private void groupedWindows_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            // yup, do nothing. helps prevent a NRE
        }

        private bool groupedWindows_Filter(object item)
        {
            ApplicationWindow window = item as ApplicationWindow;

            if (window.ShowInTaskbar)
                return true;
            else
                return false;
        }

        public void Dispose()
        {
            CairoLogger.Instance.Debug("Disposing of WindowsTasksService");
            AppGrabber.AppGrabber.Instance.CategoryList.CategoryChanged -= CategoryList_CategoryChanged;
            DeregisterShellHookWindow(_HookWin.Handle);
            if (uncloakEventHook != IntPtr.Zero) UnhookWinEvent(uncloakEventHook);
            // May be contributing to #95
            //RegisterShellHook(_HookWin.Handle, 0);// 0 = RSH_UNREGISTER - this seems to be undocumented....
            _HookWin.DestroyHandle();
        }

        private void CategoryList_CategoryChanged(object sender, EventArgs e)
        {
            foreach (ApplicationWindow window in Windows)
            {
                window.SetCategory();
            }
        }

        private void SetMinimizedMetrics()
        {
            MinimizedMetrics mm = new MinimizedMetrics
            {
                cbSize = (uint)Marshal.SizeOf(typeof(MinimizedMetrics))
            };

            IntPtr mmPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MinimizedMetrics)));

            try
            {
                Marshal.StructureToPtr(mm, mmPtr, true);
                SystemParametersInfo(SPI.GETMINIMIZEDMETRICS, mm.cbSize, mmPtr, SPIF.None);
                mm.iWidth = 140;
                mm.iArrange |= MinimizedMetricsArrangement.Hide;
                Marshal.StructureToPtr(mm, mmPtr, true);
                SystemParametersInfo(SPI.SETMINIMIZEDMETRICS, mm.cbSize, mmPtr, SPIF.None);
            }
            finally
            {
                Marshal.DestroyStructure(mmPtr, typeof(MinimizedMetrics));
                Marshal.FreeHGlobal(mmPtr);
            }
        }

        private ApplicationWindow addWindow(IntPtr hWnd, ApplicationWindow.WindowState initialState = ApplicationWindow.WindowState.Inactive, bool sanityCheck = false)
        {
            ApplicationWindow win = new ApplicationWindow(hWnd, this);

            // set window state if a non-default value is provided
            if (initialState != ApplicationWindow.WindowState.Inactive) win.State = initialState;

            // add window unless we need to validate it is eligible to show in taskbar
            if (!sanityCheck || win.CanAddToTaskbar) Windows.Add(win);

            // Only send TaskbarButtonCreated if we are shell, and if OS is not Server Core
            // This is because if Explorer is running, it will send the message, so we don't need to
            // Server Core doesn't support ITaskbarList, so sending this message on that OS could cause some assuming apps to crash
            if (Interop.Shell.IsCairoRunningAsShell && !Interop.Shell.IsServerCore) SendNotifyMessage(win.Handle, (uint)TASKBARBUTTONCREATEDMESSAGE, UIntPtr.Zero, IntPtr.Zero);

            return win;
        }

        private void removeWindow(IntPtr hWnd)
        {
            if (Windows.Any(i => i.Handle == hWnd))
            {
                do
                {
                    ApplicationWindow win = Windows.First(wnd => wnd.Handle == hWnd);
                    win.Dispose();
                    Windows.Remove(win);
                }
                while (Windows.Any(i => i.Handle == hWnd));
            }
        }

        private void ShellWinProc(Message msg)
        {
            if (msg.Msg == WM_SHELLHOOKMESSAGE)
            {
                try
                {
                    lock (_windowsLock)
                    {
                        switch ((HSHELL)msg.WParam.ToInt32())
                        {
                            case HSHELL.WINDOWCREATED:
                                CairoLogger.Instance.Debug("Created: " + msg.LParam.ToString());
                                if (!Windows.Any(i => i.Handle == msg.LParam))
                                {
                                    addWindow(msg.LParam);
                                }
                                else
                                {
                                    ApplicationWindow win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.UpdateProperties();
                                }
                                break;

                            case HSHELL.WINDOWDESTROYED:
                                CairoLogger.Instance.Debug("Destroyed: " + msg.LParam.ToString());
                                removeWindow(msg.LParam);
                                break;

                            case HSHELL.WINDOWREPLACING:
                                CairoLogger.Instance.Debug("Replacing: " + msg.LParam.ToString());
                                if (Windows.Any(i => i.Handle == msg.LParam))
                                {
                                    ApplicationWindow win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.State = ApplicationWindow.WindowState.Inactive;
                                    win.SetShowInTaskbar();
                                }
                                else
                                {
                                    addWindow(msg.LParam);
                                }
                                break;
                            case HSHELL.WINDOWREPLACED:
                                CairoLogger.Instance.Debug("Replaced: " + msg.LParam.ToString());
                                removeWindow(msg.LParam);
                                break;

                            case HSHELL.WINDOWACTIVATED:
                            case HSHELL.RUDEAPPACTIVATED:
                                CairoLogger.Instance.Debug("Activated: " + msg.LParam.ToString());

                                foreach (var aWin in Windows.Where(w => w.State == ApplicationWindow.WindowState.Active))
                                {
                                    aWin.State = ApplicationWindow.WindowState.Inactive;
                                }

                                if (msg.LParam != IntPtr.Zero)
                                {
                                    ApplicationWindow win = null;

                                    if (Windows.Any(i => i.Handle == msg.LParam))
                                    {
                                        win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                        win.State = ApplicationWindow.WindowState.Active;
                                        win.SetShowInTaskbar();
                                    }
                                    else
                                    {
                                        win = addWindow(msg.LParam, ApplicationWindow.WindowState.Active);
                                    }

                                    if (win != null)
                                    {
                                        foreach (ApplicationWindow wind in Windows)
                                        {
                                            if (wind.WinFileName == win.WinFileName)
                                                wind.SetShowInTaskbar();
                                        }
                                    }
                                }
                                break;

                            case HSHELL.FLASH:
                                CairoLogger.Instance.Debug("Flashing window: " + msg.LParam.ToString());
                                if (Windows.Any(i => i.Handle == msg.LParam))
                                {
                                    ApplicationWindow win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.State = ApplicationWindow.WindowState.Flashing;
                                }
                                else
                                {
                                    addWindow(msg.LParam, ApplicationWindow.WindowState.Flashing, true);
                                }
                                break;

                            case HSHELL.ACTIVATESHELLWINDOW:
                                CairoLogger.Instance.Debug("Activate shell window called.");
                                break;

                            case HSHELL.ENDTASK:
                                CairoLogger.Instance.Debug("EndTask called: " + msg.LParam.ToString());
                                removeWindow(msg.LParam);
                                break;

                            case HSHELL.GETMINRECT:
                                CairoLogger.Instance.Debug("GetMinRect called: " + msg.LParam.ToString());
                                SHELLHOOKINFO winHandle = (SHELLHOOKINFO)Marshal.PtrToStructure(msg.LParam, typeof(SHELLHOOKINFO));
                                winHandle.rc = new Interop.NativeMethods.Rect { Bottom = 100, Left = 0, Right = 100, Top = 0 };
                                Marshal.StructureToPtr(winHandle, msg.LParam, true);
                                msg.Result = winHandle.hwnd;
                                return; // return here so the result isnt reset to DefWindowProc

                            case HSHELL.REDRAW:
                                CairoLogger.Instance.Debug("Redraw called: " + msg.LParam.ToString());
                                if (Windows.Any(i => i.Handle == msg.LParam))
                                {
                                    ApplicationWindow win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.UpdateProperties();

                                    foreach (ApplicationWindow wind in Windows)
                                    {
                                        if (wind.WinFileName == win.WinFileName)
                                        {
                                            wind.UpdateProperties();
                                        }
                                    }
                                }
                                else
                                {
                                    addWindow(msg.LParam, ApplicationWindow.WindowState.Inactive, true);
                                }
                                break;

                            // TaskMan needs to return true if we provide our own task manager to prevent explorers.
                            // case HSHELL.TASKMAN:
                            //     SingletonLogger.Instance.Info("TaskMan Message received.");
                            //     break;

                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    CairoLogger.Instance.Error("Error in ShellWinProc. ", ex);
                    Debugger.Break();
                }
            }
            else if (msg.Msg == WM_TASKBARCREATEDMESSAGE)
            {
                CairoLogger.Instance.Debug("TaskbarCreated received, setting ITaskbarList window");
                setTaskbarListHwnd();
            }
            else
            {
                // Handle ITaskbarList functions, most not implemented yet

                ApplicationWindow win = null;

                switch (msg.Msg)
                {
                    case (int)WM.USER + 50:
                        // ActivateTab
                        // Also sends WM_SHELLHOOK message
                        CairoLogger.Instance.Debug("ITaskbarList: ActivateTab HWND:" + msg.LParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 60:
                        // MarkFullscreenWindow
                        CairoLogger.Instance.Debug("ITaskbarList: MarkFullscreenWindow HWND:" + msg.LParam + " Entering? " + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 64:
                        // SetProgressValue
                        CairoLogger.Instance.Debug("ITaskbarList: SetProgressValue HWND:" + msg.WParam + " Progress: " + msg.LParam);

                        win = new ApplicationWindow(msg.WParam, this);
                        if (Windows.Contains(win))
                        {
                            win = Windows.First(wnd => wnd.Handle == msg.WParam);
                            win.ProgressValue = (int)msg.LParam;
                        }

                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 65:
                        // SetProgressState
                        CairoLogger.Instance.Debug("ITaskbarList: SetProgressState HWND:" + msg.WParam + " Flags: " + msg.LParam);

                        win = new ApplicationWindow(msg.WParam, this);
                        if (Windows.Contains(win))
                        {
                            win = Windows.First(wnd => wnd.Handle == msg.WParam);
                            win.ProgressState = (TBPFLAG)msg.LParam;
                        }

                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 67:
                        // RegisterTab
                        CairoLogger.Instance.Debug("ITaskbarList: RegisterTab MDI HWND:" + msg.LParam + " Tab HWND: " + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 68:
                        // UnregisterTab
                        CairoLogger.Instance.Debug("ITaskbarList: UnregisterTab Tab HWND: " + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 71:
                        // SetTabOrder
                        CairoLogger.Instance.Debug("ITaskbarList: SetTabOrder HWND:" + msg.WParam + " Before HWND: " + msg.LParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 72:
                        // SetTabActive
                        CairoLogger.Instance.Debug("ITaskbarList: SetTabActive HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 75:
                        // Unknown
                        CairoLogger.Instance.Debug("ITaskbarList: Unknown HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 76:
                        // ThumbBarAddButtons
                        CairoLogger.Instance.Debug("ITaskbarList: ThumbBarAddButtons HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 77:
                        // ThumbBarUpdateButtons
                        CairoLogger.Instance.Debug("ITaskbarList: ThumbBarUpdateButtons HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 78:
                        // ThumbBarSetImageList
                        CairoLogger.Instance.Debug("ITaskbarList: ThumbBarSetImageList HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 79:
                        // SetOverlayIcon - Icon
                        CairoLogger.Instance.Debug("ITaskbarList: SetOverlayIcon - Icon HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 80:
                        // SetThumbnailTooltip
                        CairoLogger.Instance.Debug("ITaskbarList: SetThumbnailTooltip HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 81:
                        // SetThumbnailClip
                        CairoLogger.Instance.Debug("ITaskbarList: SetThumbnailClip HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 85:
                        // SetOverlayIcon - Description
                        CairoLogger.Instance.Debug("ITaskbarList: SetOverlayIcon - Description HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                    case (int)WM.USER + 87:
                        // SetTabProperties
                        CairoLogger.Instance.Debug("ITaskbarList: SetTabProperties HWND:" + msg.WParam);
                        msg.Result = IntPtr.Zero;
                        return;
                }
            }

            msg.Result = DefWindowProc(msg.HWnd, msg.Msg, msg.WParam, msg.LParam);
        }

        private void UncloakEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hWnd != IntPtr.Zero && idObject == 0 && idChild == 0)
            {
                if (Windows.Any(i => i.Handle == hWnd))
                {
                    ApplicationWindow win = Windows.First(wnd => wnd.Handle == hWnd);
                    win.Uncloak();
                }
            }
        }

        private void setTaskbarListHwnd()
        {
            // set property to receive ITaskbarList messages
            IntPtr taskbarHwnd = FindWindow("Shell_TrayWnd", "");
            if (taskbarHwnd != IntPtr.Zero)
            {
                SetProp(taskbarHwnd, "TaskbandHWND", _HookWin.Handle);
            }
        }

        public ObservableCollection<ApplicationWindow> Windows
        {
            get
            {
                return base.GetValue(windowsProperty) as ObservableCollection<ApplicationWindow>;
            }
            set
            {
                SetValue(windowsProperty, value);
            }
        }

        private DependencyProperty windowsProperty = DependencyProperty.Register("Windows", typeof(ObservableCollection<ApplicationWindow>), typeof(WindowsTasksService), new PropertyMetadata(new ObservableCollection<ApplicationWindow>()));

        private ICollectionView groupedWindows;
        public ICollectionView GroupedWindows
        {
            get
            {
                return groupedWindows;
            }
        }
    }
}
