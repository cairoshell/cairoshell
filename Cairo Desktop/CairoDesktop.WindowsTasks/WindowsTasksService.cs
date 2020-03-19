using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows;
using System.Collections.ObjectModel;
using static CairoDesktop.Interop.NativeMethods;
using CairoDesktop.Common.Logging;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.Specialized;

namespace CairoDesktop.WindowsTasks
{
    public class WindowsTasksService : DependencyObject, IDisposable
    {
        private NativeWindowEx _HookWin;
        private object _windowsLock = new object();

        private static int WM_SHELLHOOKMESSAGE = -1;
        private static int WM_TASKBARCREATEDMESSAGE = -1;
        private static int TASKBARBUTTONCREATEDMESSAGE = -1;

        public static bool IsStarting = false;
        
        private static WindowsTasksService _instance = new WindowsTasksService();
        public static WindowsTasksService Instance
        {
            get { return _instance; }
        }

        private WindowsTasksService()
        {
            this.initialize();
        }

        private void initialize()
        {
            IsStarting = true;
            
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

                // set window for ITaskbarList
                setTaskbarListHwnd();

                // adjust minimize animation
                SetMinimizedMetrics();

                // prepare collections
                groupedWindows = CollectionViewSource.GetDefaultView(Windows);
                groupedWindows.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                groupedWindows.CollectionChanged += groupedWindows_Changed;
                groupedWindows.Filter = groupedWindows_Filter;
                var taskbarItemsView = groupedWindows as ICollectionViewLiveShaping;
                taskbarItemsView.IsLiveFiltering = true;
                taskbarItemsView.LiveFilteringProperties.Add("ShowInTaskbar");

                // enumerate windows already opened
                EnumWindows(new CallBackPtr((hwnd, lParam) =>
                {
                    ApplicationWindow win = new ApplicationWindow(hwnd, this);
                    if(win.ShowInTaskbar && !Windows.Contains(win))
                        Windows.Add(win);
                    return true;
                }), 0);
            }
            catch (Exception ex)
            {
                CairoLogger.Instance.Info("Unable to start WindowsTasksService: " + ex.Message);
            }
            
            IsStarting = false;
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
            DeregisterShellHookWindow(_HookWin.Handle);
            // May be contributing to #95
            //RegisterShellHook(_HookWin.Handle, 0);// 0 = RSH_UNREGISTER - this seems to be undocumented....
            _HookWin.DestroyHandle();
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

        private void addWindow(ApplicationWindow win)
        {
            if (!Windows.Contains(win))
            {
                Windows.Add(win);

                // Only send TaskbarButtonCreated if we are shell, and if OS is not Server Core
                // This is because if Explorer is running, it will send the message, so we don't need to
                // Server Core doesn't support ITaskbarList, so sending this message on that OS could cause some assuming apps to crash
                if (Interop.Shell.IsCairoConfiguredAsShell && !Interop.Shell.IsServerCore) SendNotifyMessage(win.Handle, (uint)TASKBARBUTTONCREATEDMESSAGE, UIntPtr.Zero, IntPtr.Zero);
            }
        }

        private void removeWindow(ApplicationWindow win)
        {
            if (Windows.Contains(win))
            {
                do
                {
                    if (Configuration.Settings.Instance.EnableTaskbarPolling)
                        win.VisCheck.Stop();
                    Windows.Remove(win);
                }
                while (Windows.Contains(win));
            }
        }

        private void ShellWinProc(Message msg)
        {
            if (msg.Msg == WM_SHELLHOOKMESSAGE)
            {
                try
                {
                    var win = new ApplicationWindow(msg.LParam, this);

                    lock (this._windowsLock)
                    {
                        switch (msg.WParam.ToInt32())
                        {
                            case HSHELL_WINDOWCREATED:
                                CairoLogger.Instance.Debug("Created: " + msg.LParam.ToString());
                                addWindow(win);
                                break;

                            case HSHELL_WINDOWDESTROYED:
                                CairoLogger.Instance.Debug("Destroyed: " + msg.LParam.ToString());
                                removeWindow(win);
                                break;

                            case HSHELL_WINDOWREPLACING:
                                CairoLogger.Instance.Debug("Replacing: " + msg.LParam.ToString());
                                if (Windows.Contains(win))
                                {
                                    win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.State = ApplicationWindow.WindowState.Inactive;
                                    win.OnPropertyChanged("ShowInTaskbar");
                                }
                                else
                                {
                                    win.State = ApplicationWindow.WindowState.Inactive;
                                    addWindow(win);
                                }
                                break;
                            case HSHELL_WINDOWREPLACED:
                                CairoLogger.Instance.Debug("Replaced: " + msg.LParam.ToString());
                                removeWindow(win);
                                break;

                            case HSHELL_WINDOWACTIVATED:
                            case HSHELL_RUDEAPPACTIVATED:
                                CairoLogger.Instance.Debug("Activated: " + msg.LParam.ToString());

                                foreach (var aWin in Windows.Where(w => w.State == ApplicationWindow.WindowState.Active))
                                {
                                    aWin.State = ApplicationWindow.WindowState.Inactive;
                                }

                                if (msg.LParam != IntPtr.Zero)
                                {

                                    if (Windows.Contains(win))
                                    {
                                        win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                        win.State = ApplicationWindow.WindowState.Active;
                                        win.OnPropertyChanged("ShowInTaskbar");
                                    }
                                    else
                                    {
                                        win.State = ApplicationWindow.WindowState.Active;
                                        addWindow(win);
                                    }

                                    foreach (ApplicationWindow wind in Windows)
                                    {
                                        if (wind.WinFileName == win.WinFileName)
                                            wind.OnPropertyChanged("ShowInTaskbar");
                                    }
                                }
                                break;

                            case HSHELL_FLASH:
                                CairoLogger.Instance.Debug("Flashing window: " + msg.LParam.ToString());
                                if (Windows.Contains(win))
                                {
                                    win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.State = ApplicationWindow.WindowState.Flashing;
                                }
                                else
                                {
                                    win.State = ApplicationWindow.WindowState.Flashing;
                                    addWindow(win);
                                }
                                break;

                            case HSHELL_ACTIVATESHELLWINDOW:
                                CairoLogger.Instance.Debug("Activate shell window called.");
                                break;

                            case HSHELL_ENDTASK:
                                CairoLogger.Instance.Debug("EndTask called: " + msg.LParam.ToString());
                                removeWindow(win);
                                break;

                            case HSHELL_GETMINRECT:
                                CairoLogger.Instance.Debug("GetMinRect called: " + msg.LParam.ToString());
                                SHELLHOOKINFO winHandle = (SHELLHOOKINFO)Marshal.PtrToStructure(msg.LParam, typeof(SHELLHOOKINFO));
                                winHandle.rc = new RECT { bottom = 100, left = 0, right = 100, top = 0 };
                                Marshal.StructureToPtr(winHandle, msg.LParam, true);
                                msg.Result = winHandle.hwnd;
                                return; // return here so the result isnt reset to DefWindowProc

                            case HSHELL_REDRAW:
                                CairoLogger.Instance.Debug("Redraw called: " + msg.LParam.ToString());
                                if (Windows.Contains(win))
                                {
                                    win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.OnPropertyChanged("ShowInTaskbar");
                                    win.OnPropertyChanged("Title");
                                    win.SetIcon();

                                    foreach (ApplicationWindow wind in Windows)
                                    {
                                        if (wind.WinFileName == win.WinFileName)
                                        {
                                            wind.OnPropertyChanged("ShowInTaskbar");
                                            win.OnPropertyChanged("Title");
                                            win.SetIcon();
                                        }
                                    }
                                }
                                else
                                {
                                    addWindow(win);
                                }
                                break;

                            // TaskMan needs to return true if we provide our own task manager to prevent explorers.
                            // case HSHELL_TASKMAN:
                            //     SingletonLogger.Instance.Info("TaskMan Message received.");
                            //     break;

                            default:
                                if (Windows.Contains(win))
                                {
                                    win = Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.OnPropertyChanged("ShowInTaskbar");
                                    win.OnPropertyChanged("Title");
                                    win.SetIcon();
                                }
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
