using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows;
using System.Collections.ObjectModel;
using CairoDesktop.Interop;

namespace CairoDesktop.WindowsTasks
{
    public class WindowsTasksService : DependencyObject, IDisposable
    {
        public delegate void RedrawHandler(IntPtr windowHandle);
        public event RedrawHandler Redraw;

        private NativeWindowEx _HookWin;
        private object _windowsLock = new object();

        private static int WM_SHELLHOOKMESSAGE = -1;
        private const int WH_SHELL = 10;

        private const int HSHELL_WINDOWCREATED = 1;
        private const int HSHELL_WINDOWDESTROYED = 2;
        private const int HSHELL_ACTIVATESHELLWINDOW = 3;

        //Windows NT
        private const int HSHELL_WINDOWACTIVATED = 4;
        private const int HSHELL_GETMINRECT = 5;
        private const int HSHELL_REDRAW = 6;
        private const int HSHELL_TASKMAN = 7;
        private const int HSHELL_LANGUAGE = 8;
        private const int HSHELL_SYSMENU = 9;
        private const int HSHELL_ENDTASK = 10;
        //Windows 2000
        private const int HSHELL_ACCESSIBILITYSTATE = 11;
        private const int HSHELL_APPCOMMAND = 12;

        //Windows XP
        private const int HSHELL_WINDOWREPLACED = 13;
        private const int HSHELL_WINDOWREPLACING = 14;

        private const int HSHELL_HIGHBIT = 0x8000;
        private const int HSHELL_FLASH = (HSHELL_REDRAW | HSHELL_HIGHBIT);
        private const int HSHELL_RUDEAPPACTIVATED = (HSHELL_WINDOWACTIVATED | HSHELL_HIGHBIT);

        public const int WM_COMMAND = 0x0112;
        public const int WM_CLOSE = 0xF060;

        public WindowsTasksService()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            try
            {
                Trace.WriteLine("Starting WindowsTasksService");
                _HookWin = new NativeWindowEx();
                _HookWin.CreateHandle(new CreateParams());
                
                NativeMethods.SetTaskmanWindow(_HookWin.Handle);
                //'Register to receive shell-related events
                NativeMethods.RegisterShellHookWindow(_HookWin.Handle);

                //'Assume no error occurred
                WM_SHELLHOOKMESSAGE = NativeMethods.RegisterWindowMessage("SHELLHOOK");
                _HookWin.MessageReceived += ShellWinProc;

                SetMinimizedMetrics();
                

                //int msg = NativeMethods.RegisterWindowMessage("TaskbarCreated");
                IntPtr ptr = new IntPtr(0xffff);
                //IntPtr hDeskWnd = NativeMethods.GetDesktopWindow();
                //NativeMethods.SendMessageTimeout(ptr, (uint)msg, IntPtr.Zero, IntPtr.Zero, 2, 200, ref ptr);
                //NativeMethods.SendMessageTimeout(hDeskWnd, 0x0400, IntPtr.Zero, IntPtr.Zero, 2, 200, ref hDeskWnd);

                NativeMethods.EnumWindows(new NativeMethods.CallBackPtr((hwnd, lParam) =>
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
        }

        public void Dispose()
        {
            Trace.WriteLine("Disposing of WindowsTasksService");
            NativeMethods.DeregisterShellHookWindow(_HookWin.Handle);
            // May be contributing to #95
            //RegisterShellHook(_HookWin.Handle, 0);// 0 = RSH_UNREGISTER - this seems to be undocumented....
            _HookWin.DestroyHandle();
        }

        private void SetMinimizedMetrics()
        {
            NativeMethods.MinimizedMetrics mm = new NativeMethods.MinimizedMetrics
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.MinimizedMetrics))
            };

            IntPtr mmPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.MinimizedMetrics)));

            try
            {
                Marshal.StructureToPtr(mm, mmPtr, true);
                NativeMethods.SystemParametersInfo(NativeMethods.SPI.SPI_GETMINIMIZEDMETRICS, mm.cbSize, mmPtr, NativeMethods.SPIF.None);
                
                mm.iArrange |= NativeMethods.MinimizedMetricsArrangement.Hide;
                Marshal.StructureToPtr(mm, mmPtr, true);
                NativeMethods.SystemParametersInfo(NativeMethods.SPI.SPI_SETMINIMIZEDMETRICS, mm.cbSize, mmPtr, NativeMethods.SPIF.None);
            }
            finally
            {
            	Marshal.DestroyStructure(mmPtr, typeof(NativeMethods.MinimizedMetrics));
                Marshal.FreeHGlobal(mmPtr);
            }
        }

        private void AddWindow(IntPtr hand)
        {
            try
            {
                var win = new ApplicationWindow(hand, this);

                if (win.ShowInTaskbar && !Windows.Contains(win))
                {
                    lock (this._windowsLock)
                    {
                        Windows.Add(win);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception: " + ex.ToString());
                Debugger.Break();
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
                                Trace.WriteLine("Created: " + msg.LParam.ToString());
                                if(win.ShowInTaskbar && !Windows.Contains(win))
                                    Windows.Add(win);
                                break;

                            case HSHELL_WINDOWDESTROYED:
                                Trace.WriteLine("Destroyed: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    do
                                    {
                                        this.Windows.Remove(win);
                                    }
                                    while (this.Windows.Contains(win));
                                }
                                break;

                            case HSHELL_WINDOWREPLACING:
                                Trace.WriteLine("Replacing: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    win = this.Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.State = ApplicationWindow.WindowState.Inactive;
                                }
                                else
                                {
                                    win.State = ApplicationWindow.WindowState.Inactive;
                                    if (win.ShowInTaskbar && !Windows.Contains(win))
                                        Windows.Add(win);
                                }
                                break;
                            case HSHELL_WINDOWREPLACED:
                                Trace.WriteLine("Replaced: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    this.Windows.Remove(win);
                                }
                                break;

                            case HSHELL_WINDOWACTIVATED:
                                Trace.WriteLine("Activated: " + msg.LParam.ToString());

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
                                    }
                                    else
                                    {
                                        win.State = ApplicationWindow.WindowState.Active;
                                        if (win.ShowInTaskbar && !Windows.Contains(win))
                                            Windows.Add(win);
                                    }
                                }
                                break;

                            case HSHELL_RUDEAPPACTIVATED:
                                Trace.WriteLine("Activated: " + msg.LParam.ToString());

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
                                    }
                                    else
                                    {
                                        win.State = ApplicationWindow.WindowState.Active;
                                        if (win.ShowInTaskbar && !Windows.Contains(win))
                                            Windows.Add(win);
                                    }
                                }
                                break;

                            case HSHELL_FLASH:
                                Trace.WriteLine("Flashing window: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    win = this.Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.State = ApplicationWindow.WindowState.Flashing;
                                }
                                else
                                {
                                    win.State = ApplicationWindow.WindowState.Flashing;
                                    if (win.ShowInTaskbar && !Windows.Contains(win))
                                        Windows.Add(win);
                                }
                                break;

                            case HSHELL_ACTIVATESHELLWINDOW:
                                Trace.WriteLine("Activate shell window called.");
                                break;

                            case HSHELL_ENDTASK:
                                Trace.WriteLine("EndTask called: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    this.Windows.Remove(win);
                                }
                                break;

                            case HSHELL_GETMINRECT:
                                Trace.WriteLine("GetMinRect called: " + msg.LParam.ToString());
                                NativeMethods.SHELLHOOKINFO winHandle = (NativeMethods.SHELLHOOKINFO)Marshal.PtrToStructure(msg.LParam, typeof(NativeMethods.SHELLHOOKINFO));
                                winHandle.rc = new NativeMethods.RECT { bottom = 100, left = 0, right = 100, top = 0 };
                                Marshal.StructureToPtr(winHandle, msg.LParam, true);
                                msg.Result = winHandle.hwnd;
                                break;

                            case HSHELL_REDRAW:
                                Trace.WriteLine("Redraw called: " + msg.LParam.ToString());
                                this.OnRedraw(msg.LParam);
                                break;

                            // TaskMan needs to return true if we provide our own task manager to prevent explorers.
                            // case HSHELL_TASKMAN:
                            //     Trace.WriteLine("TaskMan Message received.");
                            //     break;

                            default:
                                Trace.WriteLine("Unknown called: " + msg.LParam.ToString() + " Message " + msg.Msg.ToString());
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Exception: " + ex.ToString());
                    Debugger.Break();
                }
            }
        }

        private void OnRedraw(IntPtr windowHandle)
        {
            if (this.Redraw != null)
            {
                this.Redraw(windowHandle);
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

        private static DependencyProperty windowsProperty = DependencyProperty.Register("Windows", typeof(ObservableCollection<ApplicationWindow>), typeof(WindowsTasksService), new PropertyMetadata(new ObservableCollection<ApplicationWindow>()));
    }
}
