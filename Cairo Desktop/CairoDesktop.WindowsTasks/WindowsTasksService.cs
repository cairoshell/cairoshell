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
                _HookWin.MessageReceived += ShellWinProc;

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
                        this.Windows.Add(win);
                    return true;
                }), 0);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
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
                SystemParametersInfo(SPI.SPI_GETMINIMIZEDMETRICS, mm.cbSize, mmPtr, SPIF.None);
                mm.iWidth = 140;
                mm.iArrange |= MinimizedMetricsArrangement.Hide;
                Marshal.StructureToPtr(mm, mmPtr, true);
                SystemParametersInfo(SPI.SPI_SETMINIMIZEDMETRICS, mm.cbSize, mmPtr, SPIF.None);
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
                Windows.Add(win);
        }

        private void removeWindow(ApplicationWindow win)
        {
            if (this.Windows.Contains(win))
            {
                do
                {
                    if (Configuration.Settings.EnableTaskbarPolling)
                        win.VisCheck.Stop();
                    this.Windows.Remove(win);
                }
                while (this.Windows.Contains(win));
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
                                if (this.Windows.Contains(win))
                                {
                                    win = this.Windows.First(wnd => wnd.Handle == msg.LParam);
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

                                foreach (var aWin in this.Windows.Where(w => w.State == ApplicationWindow.WindowState.Active))
                                {
                                    aWin.State = ApplicationWindow.WindowState.Inactive;
                                }

                                if (msg.LParam != IntPtr.Zero)
                                {

                                    if (this.Windows.Contains(win))
                                    {
                                        win = this.Windows.First(wnd => wnd.Handle == msg.LParam);
                                        win.State = ApplicationWindow.WindowState.Active;
                                        win.OnPropertyChanged("ShowInTaskbar");
                                    }
                                    else
                                    {
                                        win.State = ApplicationWindow.WindowState.Active;
                                        addWindow(win);
                                    }

                                    foreach (ApplicationWindow wind in this.Windows)
                                    {
                                        if (wind.WinFileName == win.WinFileName)
                                            wind.OnPropertyChanged("ShowInTaskbar");
                                    }
                                }
                                break;

                            case HSHELL_FLASH:
                                CairoLogger.Instance.Debug("Flashing window: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    win = this.Windows.First(wnd => wnd.Handle == msg.LParam);
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
                                break;

                            case HSHELL_REDRAW:
                                CairoLogger.Instance.Debug("Redraw called: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    win = this.Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.OnPropertyChanged("ShowInTaskbar");
                                    win.OnPropertyChanged("Title");
                                    win.SetIcon();

                                    foreach (ApplicationWindow wind in this.Windows)
                                    {
                                        if (wind.WinFileName == win.WinFileName)
                                        {
                                            wind.OnPropertyChanged("ShowInTaskbar");
                                            win.OnPropertyChanged("Title");
                                            win.SetIcon();
                                        }
                                    }
                                }
                                break;

                            // TaskMan needs to return true if we provide our own task manager to prevent explorers.
                            // case HSHELL_TASKMAN:
                            //     SingletonLogger.Instance.Info("TaskMan Message received.");
                            //     break;

                            default:
                                if (this.Windows.Contains(win))
                                {
                                    win = this.Windows.First(wnd => wnd.Handle == msg.LParam);
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
            
            msg.Result = DefWindowProc(msg.HWnd, msg.Msg, msg.WParam, msg.LParam);
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
