using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace CairoDesktop.WindowsTasks
{
    public class NativeWindowEx : System.Windows.Forms.NativeWindow
    {
        public delegate IntPtr WndProcDelegate(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public delegate void MessageReceivedEventHandler(System.Windows.Forms.Message m);

        public event MessageReceivedEventHandler MessageReceived;

        [DllImport("user32.dll")]
        public static extern int RegisterClassEx(ref WNDCLASSEX pcWndClassEx);

        [DllImport("user32.dll")]
        public static extern bool UnregisterClass(string lpClassname, IntPtr hInstance);

        [DllImport("user32.dll")]
        public static extern IntPtr GetModuleHandle(string filename);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hwndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder sb, int Length);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool IsAppHungWindow(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClassLong(IntPtr handle, int longClass);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClassLongPtr(IntPtr handle, int longClass);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr handle, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr handle, int wCmd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam,
            ref RECT pvParam, uint fWinIni);

        /// <summary>For SystemParametersInfo</summary>
        public enum SPI : int
        {
            SPI_SETWORKAREA = 0x002F,
            SPI_GETWORKAREA = 0x0030
        }

        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int MA_NOACTIVATE = 0x0003;
        public const int WM_WINDOWPOSCHANGING = 0x0046;

        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        /// <summary>Shows a Window</summary>
        /// <remarks>
        /// <para>To perform certain special effects when showing or hiding a 
        /// window, use AnimateWindow.</para>
        ///<para>The first time an application calls ShowWindow, it should use 
        ///the WinMain function's nCmdShow parameter as its nCmdShow parameter. 
        ///Subsequent calls to ShowWindow must use one of the values in the 
        ///given list, instead of the one specified by the WinMain function's 
        ///nCmdShow parameter.</para>
        ///<para>As noted in the discussion of the nCmdShow parameter, the 
        ///nCmdShow value is ignored in the first call to ShowWindow if the 
        ///program that launched the application specifies startup information 
        ///in the structure. In this case, ShowWindow uses the information 
        ///specified in the STARTUPINFO structure to show the window. On 
        ///subsequent calls, the application must call ShowWindow with nCmdShow 
        ///set to SW_SHOWDEFAULT to use the startup information provided by the 
        ///program that launched the application. This behavior is designed for 
        ///the following situations: </para>
        ///<list type="">
        ///    <item>Applications create their main window by calling CreateWindow 
        ///    with the WS_VISIBLE flag set. </item>
        ///    <item>Applications create their main window by calling CreateWindow 
        ///    with the WS_VISIBLE flag cleared, and later call ShowWindow with the 
        ///    SW_SHOW flag set to make it visible.</item>
        ///</list></remarks>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="nCmdShow">Specifies how the window is to be shown. 
        /// This parameter is ignored the first time an application calls 
        /// ShowWindow, if the program that launched the application provides a 
        /// STARTUPINFO structure. Otherwise, the first time ShowWindow is called, 
        /// the value should be the value obtained by the WinMain function in its 
        /// nCmdShow parameter. In subsequent calls, this parameter can be one of 
        /// the WindowShowStyle members.</param>
        /// <returns>
        /// If the window was previously visible, the return value is nonzero. 
        /// If the window was previously hidden, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        /// <summary>Enumeration of the different ways of showing a window using 
        /// ShowWindow</summary>
        public enum WindowShowStyle : uint
        {
            /// <summary>Hides the window and activates another window.</summary>
            /// <remarks>See SW_HIDE</remarks>
            Hide = 0,
            /// <summary>Activates and displays a window. If the window is minimized 
            /// or maximized, the system restores it to its original size and 
            /// position. An application should specify this flag when displaying 
            /// the window for the first time.</summary>
            /// <remarks>See SW_SHOWNORMAL</remarks>
            ShowNormal = 1,
            /// <summary>Activates the window and displays it as a minimized window.</summary>
            /// <remarks>See SW_SHOWMINIMIZED</remarks>
            ShowMinimized = 2,
            /// <summary>Activates the window and displays it as a maximized window.</summary>
            /// <remarks>See SW_SHOWMAXIMIZED</remarks>
            ShowMaximized = 3,
            /// <summary>Maximizes the specified window.</summary>
            /// <remarks>See SW_MAXIMIZE</remarks>
            Maximize = 3,
            /// <summary>Displays a window in its most recent size and position. 
            /// This value is similar to "ShowNormal", except the window is not 
            /// actived.</summary>
            /// <remarks>See SW_SHOWNOACTIVATE</remarks>
            ShowNormalNoActivate = 4,
            /// <summary>Activates the window and displays it in its current size 
            /// and position.</summary>
            /// <remarks>See SW_SHOW</remarks>
            Show = 5,
            /// <summary>Minimizes the specified window and activates the next 
            /// top-level window in the Z order.</summary>
            /// <remarks>See SW_MINIMIZE</remarks>
            Minimize = 6,
            /// <summary>Displays the window as a minimized window. This value is 
            /// similar to "ShowMinimized", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
            ShowMinNoActivate = 7,
            /// <summary>Displays the window in its current size and position. This 
            /// value is similar to "Show", except the window is not activated.</summary>
            /// <remarks>See SW_SHOWNA</remarks>
            ShowNoActivate = 8,
            /// <summary>Activates and displays the window. If the window is 
            /// minimized or maximized, the system restores it to its original size 
            /// and position. An application should specify this flag when restoring 
            /// a minimized window.</summary>
            /// <remarks>See SW_RESTORE</remarks>
            Restore = 9,
            /// <summary>Sets the show state based on the SW_ value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.</summary>
            /// <remarks>See SW_SHOWDEFAULT</remarks>
            ShowDefault = 10,
            /// <summary>Windows 2000/XP: Minimizes a window, even if the thread 
            /// that owns the window is hung. This flag should only be used when 
            /// minimizing windows from a different thread.</summary>
            /// <remarks>See SW_FORCEMINIMIZE</remarks>
            ForceMinimized = 11
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WNDCLASSEX
        {
            public int cbSize; // Size in bytes of the WNDCLASSEX structure
            public int style;	// Class style
            public WndProcDelegate lpfnWndProc;// Pointer to the classes Window Procedure
            public int cbClsExtra;// Number of extra bytes to allocate for class
            public int cbWndExtra;// Number of extra bytes to allocate for window
            public IntPtr hInstance;// Applications instance handle Class
            public IntPtr hIcon;// Handle to the classes icon
            public IntPtr hCursor;// Handle to the classes cursor
            public IntPtr hbrBackground;// Handle to the classes background brush
            public string lpszMenuName;// Resource name of class menu
            public string lpszClassName;// Name of the Window Class
            public IntPtr hIconSm;// Handle to the classes small icon
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);
            if (MessageReceived != null) MessageReceived(m);
        }



        public void RegisterClass(ClassCreateParams ccp)
        {
            WNDCLASSEX wc = new WNDCLASSEX()
            {
                cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
                lpszClassName = ccp.Name,
                lpfnWndProc = ccp.WndProc,
                style = (int)ccp.Style,
                cbClsExtra = ccp.ExtraClassBytes,
                cbWndExtra = ccp.ExtraWindowBytes,
                hbrBackground = ccp.BackgroundBrush,
                hInstance = Marshal.GetHINSTANCE(this.GetType().Module)
            };

            if (RegisterClassEx(ref wc) != 0)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
        }
                
        public void UnregisterClassAPI(string name)
        {
            if (!UnregisterClass(name, IntPtr.Zero))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public override void CreateHandle(System.Windows.Forms.CreateParams cp)
        {
            base.CreateHandle(cp);
        }

        public void CreateHandle(CreateParamsEx cp)
        {
            this.AssignHandle(CreateWindowEx(cp.ExStyle, cp.ClassName, cp.Caption, cp.Style, cp.X, cp.Y, cp.Width, cp.Height, cp.Parent, IntPtr.Zero, cp.hInstance, (IntPtr)cp.Param));
        }
    }

    public class CreateParamsEx : System.Windows.Forms.CreateParams
    {
        IntPtr _hInstance = Marshal.GetHINSTANCE(typeof(NativeWindowEx).Module);

        public IntPtr hInstance
        {
            get
            {
                return _hInstance;
            }
            set
            {
                _hInstance = value;
            }
        }
    }

    public class ClassCreateParams
    {
        public ClassStyles Style { get; set; }
        public string Name { get; set; }
        public NativeWindowEx.WndProcDelegate WndProc { get; set; }
        public IntPtr BackgroundBrush { get; set; }
        public Icon Icon { get; set; }
        public Icon SmallIcon { get; set; }
        public Cursor Cursor { get; set; }
        public int ExtraClassBytes { get; set; }
        public int ExtraWindowBytes { get; set; }

        [Flags]
        public enum ClassStyles : int
        {
            VRedraw = 0x1,
            HRedraw = 0x2,
            DoubleClicks = 0x8,
            OwnDC = 0x20,
            ClassDC = 0x40,
            ParentDC = 0x80,
            NoClose = 0x200,
            SaveBits = 0x800,
            ByteAlignClient = 0x1000,
            ByteAlignWindow = 0x2000,
            GlobalClass = 0x4000,
            IME = 0x10000,
            DropShadow = 0x20000
        }
    }

    public enum ExtendedWindowStyles : uint
    {
        WS_EX_DLGMODALFRAME = 0x1,
        WS_EX_NOPARENTNOTIFY = 0x4,
        WS_EX_TOPMOST = unchecked(0x8),
        WS_EX_ACCEPTFILES = 0x10,
        WS_EX_TRANSPARENT = 0x20,
        WS_EX_MDICHILD = 0x40,
        WS_EX_TOOLWINDOW = 0x80,
        WS_EX_WINDOWEDGE = 0x100,
        WS_EX_CLIENTEDGE = 0x200,
        WS_EX_CONTEXTHELP = 0x400,
        WS_EX_RIGHT = 0x1000,
        WS_EX_LEFT = 0x0,
        WS_EX_RTLREADING = 0x2000,
        WS_EX_LTRREADING = 0x0,
        WS_EX_LEFTSCROLLBAR = 0x4000,
        WS_EX_RIGHTSCROLLBAR = 0x0,
        WS_EX_CONTROLPARENT = 0x10000,
        WS_EX_STATICEDGE = 0x20000,
        WS_EX_APPWINDOW = 0x40000,
        WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
        WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
        WS_EX_LAYERED = 0x80000,
        WS_EX_NOINHERITLAYOUT = 0x100000,
        WS_EX_LAYOUTRTL = 0x400000,
        WS_EX_COMPOSITED = 0x2000000,
        WS_EX_NOACTIVATE = 0x8000000
    }

    [Flags]
    public enum WindowStyles : uint
    {
        WS_OVERLAPPED = 0x0,
        WS_POPUP = 0x80000000,
        WS_CHILD = 0x40000000,
        WS_MINIMIZE = 0x20000000,
        WS_VISIBLE = 0x10000000,
        WS_DISABLED = 0x8000000,
        WS_CLIPSIBLINGS = 0x4000000,
        WS_CLIPCHILDREN = 0x2000000,
        WS_MAXIMIZE = 0x1000000,
        WS_CAPTION = (WS_BORDER | WS_DLGFRAME),
        WS_BORDER = 0x800000,
        WS_DLGFRAME = 0x400000,
        WS_VSCROLL = 0x200000,
        WS_HSCROLL = 0x100000,
        WS_SYSMENU = 0x80000,
        WS_THICKFRAME = 0x40000,
        WS_GROUP = 0x20000,
        WS_TABSTOP = 0x10000,
        WS_MINIMIZEBOX = 0x20000,
        WS_MAXIMIZEBOX = 0x10000,
        WS_TILED = WS_OVERLAPPED,
        WS_ICONIC = WS_MINIMIZE,
        WS_SIZEBOX = WS_THICKFRAME,
        WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,
        WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX),
        WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU),
        WS_CHILDWINDOW = (WS_CHILD)
    }

    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}