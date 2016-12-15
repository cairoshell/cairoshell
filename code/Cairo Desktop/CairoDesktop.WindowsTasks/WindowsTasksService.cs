using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows;
using System.Collections.ObjectModel;
using CairoDesktop.Interop;
using DR = System.Drawing;

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
                
                SetTaskmanWindow(_HookWin.Handle);
                //'Register to receive shell-related events
                RegisterShellHookWindow(_HookWin.Handle);

                //'Assume no error occurred
                WM_SHELLHOOKMESSAGE = RegisterWindowMessage("SHELLHOOK");
                _HookWin.MessageReceived += ShellWinProc;

                SetMinimizedMetrics();
                

                int msg = RegisterWindowMessage("TaskbarCreated");
                IntPtr ptr = new IntPtr(0xffff);
                IntPtr hDeskWnd = GetDesktopWindow();
                Shell.SendMessageTimeout(ptr, (uint)msg, IntPtr.Zero, IntPtr.Zero, 2, 200, ref ptr);
                Shell.SendMessageTimeout(hDeskWnd, 0x0400, IntPtr.Zero, IntPtr.Zero, 2, 200, ref hDeskWnd);

                EnumWindows(new CallBackPtr((hwnd, lParam) =>
                {
                    ApplicationWindow win = new ApplicationWindow(hwnd, this);
                    if(win.ShowInTaskbar)
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

        private void AddWindow(IntPtr hand)
        {
            try
            {
                var win = new ApplicationWindow(hand, this);

                if (win.ShowInTaskbar)
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

        private void ShellWinProc(System.Windows.Forms.Message msg)
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
                                if(win.ShowInTaskbar)
                                    Windows.Add(win);
                                break;

                            case HSHELL_WINDOWDESTROYED:
                                Trace.WriteLine("Destroyed: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    this.Windows.Remove(win);
                                }
                                break;

                            case HSHELL_WINDOWREPLACING:
                                Trace.WriteLine("Replacing: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    this.Windows.Remove(win);
                                }
                                break;
                            case HSHELL_WINDOWREPLACED:
                                Trace.WriteLine("Replaced: " + msg.LParam.ToString());
                                if (this.Windows.Contains(win))
                                {
                                    win = this.Windows.First(wnd => wnd.Handle == msg.LParam);
                                    win.State = ApplicationWindow.WindowState.Inactive;
                                }
                                else
                                {
                                    win.State = ApplicationWindow.WindowState.Inactive;
                                    if (win.ShowInTaskbar)
                                        Windows.Add(win);
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
                                        if (win.ShowInTaskbar)
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
                                        if (win.ShowInTaskbar)
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
                                    if (win.ShowInTaskbar)
                                        Windows.Add(win);
                                }
                                break;

                            case HSHELL_ACTIVATESHELLWINDOW:
                                Trace.WriteLine("Activate shell window called.");
                                break;

                            case HSHELL_ENDTASK:
                                Trace.WriteLine("EndTask called.");
                                if (this.Windows.Contains(win))
                                {
                                    this.Windows.Remove(win);
                                }
                                break;

                            case HSHELL_GETMINRECT:
                                Trace.WriteLine("GetMinRect called.");
                                SHELLHOOKINFO winHandle = (SHELLHOOKINFO)Marshal.PtrToStructure(msg.LParam, typeof(SHELLHOOKINFO));
                                winHandle.rc.Top = 0;
                                winHandle.rc.Left = 0;
                                winHandle.rc.Bottom = 100;
                                winHandle.rc.Right = 100;
                                Marshal.StructureToPtr(winHandle, msg.LParam, true);
                                msg.Result = winHandle.hwnd;
                                break;

                            case HSHELL_REDRAW:
                                Trace.WriteLine("Redraw called.");
                                this.OnRedraw(msg.LParam);
                                break;

                            // TaskMan needs to return true if we provide our own task manager to prevent explorers.
                            // case HSHELL_TASKMAN:
                            //     Trace.WriteLine("TaskMan Message received.");
                            //     break;

                            default:
                                Trace.WriteLine("Unknown called. " + msg.Msg.ToString());
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

        public delegate bool CallBackPtr(IntPtr hwnd, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(CallBackPtr callPtr, int lPar);

        [DllImport("user32.dll")]
        private static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        private static extern bool SetTaskmanWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool DeregisterShellHookWindow(IntPtr hWnd);

        [DllImport("Shell32.dll")]
        private static extern bool RegisterShellHook(IntPtr hWnd, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, IntPtr pvParam, SPIF fWinIni);

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
        
        [Flags]
        public enum SPIF
        {
           None =          0x00,
           SPIF_UPDATEINIFILE =    0x01,  // Writes the new system-wide parameter setting to the user profile.
           SPIF_SENDCHANGE =       0x02,  // Broadcasts the WM_SETTINGCHANGE message after updating the user profile.
           SPIF_SENDWININICHANGE = 0x02   // Same as SPIF_SENDCHANGE.
        }

        public enum SPI : uint
        {
            SPI_GETMINIMIZEDMETRICS = 0x002B,
            SPI_SETMINIMIZEDMETRICS = 0x002C
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MinimizedMetrics
        {
            public uint cbSize;
            public int iWidth;
            public int iHorzGap;
            public int iVertGap;
            public MinimizedMetricsArrangement iArrange;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Top;
            public int Left;
            public int Bottom;
            public int Right;

            public RECT(int left_, int top_, int right_, int bottom_)
            {
                Left = left_;
                Top = top_;
                Right = right_;
                Bottom = bottom_;
            }

            public int Height { get { return Bottom - Top; } }
            public int Width { get { return Right - Left; } }
            public DR.Size Size { get { return new DR.Size(Width, Height); } }

            public Point Location { get { return new Point(Left, Top); } }

            // Handy method for converting to a System.Drawing.Rectangle
            public DR.Rectangle ToRectangle()
            { return DR.Rectangle.FromLTRB(Left, Top, Right, Bottom); }

            public static RECT FromRectangle(DR.Rectangle rectangle)
            {
                return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            public override int GetHashCode()
            {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                  ^ ((Width << 0x1a) | (Width >> 6))
                  ^ ((Height << 7) | (Height >> 0x19));
            }

            #region Operator overloads

            public static implicit operator DR.Rectangle(RECT rect)
            {
                return rect.ToRectangle();
            }

            public static implicit operator RECT(DR.Rectangle rect)
            {
                return FromRectangle(rect);
            }

            #endregion

        }

        [Flags]
        public enum MinimizedMetricsArrangement
        {
            BottomLeft = 0,
            BottomRight = 1,
            TopLeft = 2,
            TopRight = 3,
            Left = 0,
            Right = 0,
            Up = 4,
            Down = 4,
            Hide = 8
        }

        	[StructLayout(LayoutKind.Sequential)]
	        private struct SHELLHOOKINFO
            {
		        public IntPtr hwnd;
		        public RECT rc;
            }
    }
}
