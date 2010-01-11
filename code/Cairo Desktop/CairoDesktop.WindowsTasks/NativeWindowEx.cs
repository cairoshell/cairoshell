using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
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
        private static extern int RegisterClassEx(ref WNDCLASSEX pcWndClassEx);

        [DllImport("user32.dll")]
        private static extern bool UnregisterClass(string lpClassname, IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr GetModuleHandle(string filename);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hwndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WNDCLASSEX
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
}