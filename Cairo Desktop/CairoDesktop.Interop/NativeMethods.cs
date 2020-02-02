using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CairoDesktop.Interop
{

    /// <summary>
    /// Container class for Win32 Native methods used within the desktop application (e.g. shutdown, sleep, et al).
    /// </summary>
    public class NativeMethods
    {
        public const uint TOKENADJUSTPRIVILEGES = 0x00000020;
        public const uint TOKENQUERY = 0x00000008;

        public enum ExitWindows : uint
        {
            /// <summary>
            /// Log the user off.
            /// </summary>
            Logoff = 0x00,

            /// <summary>
            /// Shutdown the machine.
            /// </summary>
            Shutdown = 0x08,

            /// <summary>
            /// Reboots the machine.
            /// </summary>
            Reboot = 0x02,

            /// <summary>
            /// Forces the machine to perform the operation if the apps are hung.
            /// Use this in conjunction with one of the lower flags.
            /// </summary>
            ForceIfHung = 0x10
        }

        

        public static IntPtr FindWindow(string className)
        {
            return FindWindow(className, string.Empty);
        }

        public const int HWND_TOPMOST = -1; // 0xffff 
        public const int HWND_BOTTOMMOST = 1;

        // Handling the close splash screen event
        [DllImport("kernel32.dll")]
        public static extern Int32 OpenEvent(Int32 DesiredAccess, bool InheritHandle, string Name);

        // OpenEvent DesiredAccess defines
        public const int EVENT_MODIFY_STATE = 0x00000002;

        [DllImport("kernel32.dll")]
        public static extern Int32 SetEvent(Int32 Handle);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(Int32 Handle);

        [DllImport("user32.dll")]
        public static extern Int32 SetShellWindow(IntPtr hWnd);

        [DllImport("USER32.dll")]
        extern public static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        /// <summary>
        /// Structure for the token privileges request.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TOKEN_PRIVILEGES
        {
            /// <summary>
            /// The number of privileges.
            /// </summary>
            public uint PrivilegeCount;

            /// <summary>
            /// The local UID for the request.
            /// </summary>
            public long Luid;

            /// <summary>
            /// Attributes for the request.
            /// </summary>
            public uint Attributes;
        }

        public struct BATTERY_REPORTING_SCALE
        {
            public uint Granularity;
            public uint Capacity;
        }

        public enum SYSTEM_POWER_STATE
        {
            PowerSystemUnspecified = 0,
            PowerSystemWorking = 1,
            PowerSystemSleeping1 = 2,
            PowerSystemSleeping2 = 3,
            PowerSystemSleeping3 = 4,
            PowerSystemHibernate = 5,
            PowerSystemShutdown = 6,
            PowerSystemMaximum = 7
        }

        public struct SYSTEM_POWER_CAPABILITIES
        {
            [MarshalAs(UnmanagedType.U1)]
            public bool PowerButtonPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool SleepButtonPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool LidPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS1;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS2;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS3;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS4;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS5;
            [MarshalAs(UnmanagedType.U1)]
            public bool HiberFilePresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool FullWake;
            [MarshalAs(UnmanagedType.U1)]
            public bool VideoDimPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool ApmPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool UpsPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool ThermalControl;
            [MarshalAs(UnmanagedType.U1)]
            public bool ProcessorThrottle;
            public byte ProcessorMinThrottle;
            public byte ProcessorMaxThrottle;    // Also known as ProcessorThrottleScale before Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool FastSystemS4;   // Ignore if earlier than Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool Hiberboot;  // Ignore if earlier than Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool WakeAlarmPresent;   // Ignore if earlier than Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool AoAc;   // Ignore if earlier than Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool DiskSpinDown;
            public byte HiberFileType;  // Ignore if earlier than Windows 10 (10.0.10240.0)
            [MarshalAs(UnmanagedType.U1)]
            public bool AoAcConnectivitySupported;  // Ignore if earlier than Windows 10 (10.0.10240.0)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            private readonly byte[] spare3;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemBatteriesPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool BatteriesAreShortTerm;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public BATTERY_REPORTING_SCALE[] BatteryScale;
            public SYSTEM_POWER_STATE AcOnLineWake;
            public SYSTEM_POWER_STATE SoftLidWake;
            public SYSTEM_POWER_STATE RtcWake;
            public SYSTEM_POWER_STATE MinDeviceWakeState;
            public SYSTEM_POWER_STATE DefaultLowLatencyWake;
        }

        [DllImport("user32.dll")]
        public static extern bool ExitWindowsEx(uint flags, uint reason);

        // There is a method for this in System.Windows.Forms, however it calls the same p/invoke and I would prefer not to reference that lib
        [DllImport("powrprof.dll")]
        public static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        [DllImport("powrprof.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GetPwrCapabilities(out SYSTEM_POWER_CAPABILITIES systemPowerCapabilites);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, bool disableAllPrivileges, ref TOKEN_PRIVILEGES newState, uint bufferLength, IntPtr previousState, IntPtr returnLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint callback, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        public enum ABMsg : int
        {
            ABM_NEW = 0,
            ABM_REMOVE,
            ABM_QUERYPOS,
            ABM_SETPOS,
            ABM_GETSTATE,
            ABM_GETTASKBARPOS,
            ABM_ACTIVATE,
            ABM_GETAUTOHIDEBAR,
            ABM_SETAUTOHIDEBAR,
            ABM_WINDOWPOSCHANGED,
            ABM_SETSTATE
        }

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string msg);

        [DllImport("User32.dll")]
        public static extern int GetSystemMetrics(int Index);

        public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        public enum CSIDL
        {
            CSIDL_DESKTOP = 0x0000,
            CSIDL_INTERNET = 0x0001,
            CSIDL_PROGRAMS = 0x0002,
            CSIDL_CONTROLS = 0x0003,
            CSIDL_PRINTERS = 0x0004,
            CSIDL_PERSONAL = 0x0005,
            CSIDL_FAVORITES = 0x0006,
            CSIDL_STARTUP = 0x0007,
            CSIDL_RECENT = 0x0008,
            CSIDL_SENDTO = 0x0009,
            CSIDL_BITBUCKET = 0x000A, /*RecycleBin*/
            CSIDL_STARTMENU = 0x000B,
            CSIDL_MYDOCUMENTS = 0x000C,
            CSIDL_MYMUSIC = 0x000D,
            CSIDL_MYVIDEO = 0x000E,
            CSIDL_DESKTOPDIRECTORY = 0x0010,
            CSIDL_DRIVES = 0x0011, /*MyComputer*/
            CSIDL_NETWORK = 0x0012,
            CSIDL_NETHOOD = 0x0013,
            CSIDL_FONTS = 0x0014,
            CSIDL_TEMPLATES = 0x0015,
            CSIDL_COMMON_STARTMENU = 0x0016,
            CSIDL_COMMON_PROGRAMS = 0x0017,
            CSIDL_COMMON_STARTUP = 0x0018,
            CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019,
            CSIDL_APPDATA = 0x001A,
            CSIDL_PRINTHOOD = 0x001B,
            CSIDL_LOCAL_APPDATA = 0x001C,
            CSIDL_ALTSTARTUP = 0x001D,
            CSIDL_COMMON_ALTSTARTUP = 0x001E,
            CSIDL_COMMON_FAVORITES = 0x001F,
            CSIDL_INTERNET_CACHE = 0x0020,
            CSIDL_COOKIES = 0x0021,
            CSIDL_HISTORY = 0x0022,
            CSIDL_COMMON_APPDATA = 0x0023,
            CSIDL_WINDOWS = 0x0024,
            CSIDL_SYSTEM = 0x0025,
            CSIDL_PROGRAM_FILES = 0x0026,
            CSIDL_MYPICTURES = 0x0027,
            CSIDL_PROFILE = 0x0028,
            CSIDL_PROGRAM_FILES_COMMON = 0x002B,
            CSIDL_COMMON_TEMPLATES = 0x002D,
            CSIDL_COMMON_DOCUMENTS = 0x002E,
            CSIDL_COMMON_ADMINTOOLS = 0x002F,
            CSIDL_ADMINTOOLS = 0x0030,
            CSIDL_COMMON_MUSIC = 0x0035,
            CSIDL_COMMON_PICTURES = 0x0036,
            CSIDL_COMMON_VIDEO = 0x0037,
            CSIDL_CDBURN_AREA = 0x003B,
            CSIDL_PROFILES = 0x003E,
            CSIDL_FLAG_CREATE = 0x8000,
        }//CSIDL

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [Flags]
        public enum SHGFI : int
        {
            /// <summary>get icon</summary>
            Icon = 0x000000100,
            /// <summary>get display name</summary>
            DisplayName = 0x000000200,
            /// <summary>get type name</summary>
            TypeName = 0x000000400,
            /// <summary>get attributes</summary>
            Attributes = 0x000000800,
            /// <summary>get icon location</summary>
            IconLocation = 0x000001000,
            /// <summary>return exe type</summary>
            ExeType = 0x000002000,
            /// <summary>get system icon index</summary>
            SysIconIndex = 0x000004000,
            /// <summary>put a link overlay on icon</summary>
            LinkOverlay = 0x000008000,
            /// <summary>show icon in selected state</summary>
            Selected = 0x000010000,
            /// <summary>get only specified attributes</summary>
            Attr_Specified = 0x000020000,
            /// <summary>get large icon</summary>
            LargeIcon = 0x000000000,
            /// <summary>get small icon</summary>
            SmallIcon = 0x000000001,
            /// <summary>get open icon</summary>
            OpenIcon = 0x000000002,
            /// <summary>get shell size icon</summary>
            ShellIconSize = 0x000000004,
            /// <summary>pszPath is a pidl</summary>
            PIDL = 0x000000008,
            /// <summary>use passed dwFileAttribute</summary>
            UseFileAttributes = 0x000000010,
            /// <summary>apply the appropriate overlays</summary>
            AddOverlays = 0x000000020,
            /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
            OverlayIndex = 0x000000040,
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("User32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
        public static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        public const int SW_SHOW = 5;
        public const uint SEE_MASK_INVOKEIDLIST = 12;

        /// <summary>
        /// Possible flags for the SHFileOperation method.
        /// </summary>
        [Flags]
        public enum FileOperationFlags : ushort
        {
            /// <summary>
            /// Do not show a dialog during the process
            /// </summary>
            FOF_SILENT = 0x0004,
            /// <summary>
            /// Do not ask the user to confirm selection
            /// </summary>
            FOF_NOCONFIRMATION = 0x0010,
            /// <summary>
            /// Delete the file to the recycle bin.  (Required flag to send a file to the bin
            /// </summary>
            FOF_ALLOWUNDO = 0x0040,
            /// <summary>
            /// Do not show the names of the files or folders that are being recycled.
            /// </summary>
            FOF_SIMPLEPROGRESS = 0x0100,
            /// <summary>
            /// Surpress errors, if any occur during the process.
            /// </summary>
            FOF_NOERRORUI = 0x0400,
            /// <summary>
            /// Warn if files are too big to fit in the recycle bin and will need
            /// to be deleted completely.
            /// </summary>
            FOF_WANTNUKEWARNING = 0x4000,
        }

        /// <summary>
        /// File Operation Function Type for SHFileOperation
        /// </summary>
        public enum FileOperationType : uint
        {
            /// <summary>
            /// Move the objects
            /// </summary>
            FO_MOVE = 0x0001,
            /// <summary>
            /// Copy the objects
            /// </summary>
            FO_COPY = 0x0002,
            /// <summary>
            /// Delete (or recycle) the objects
            /// </summary>
            FO_DELETE = 0x0003,
            /// <summary>
            /// Rename the object(s)
            /// </summary>
            FO_RENAME = 0x0004,
        }

        /// <summary>
        /// SHFILEOPSTRUCT for SHFileOperation from COM
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEOPSTRUCT
        {

            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll")]
        public static extern uint SendMessageTimeout(IntPtr hWnd, uint messageId, IntPtr wparam, IntPtr lparam, uint timeoutFlags, uint timeout, ref IntPtr retval);

        [DllImport("user32.dll")]
        public static extern uint SendMessageTimeout(IntPtr hWnd, uint messageId, uint wparam, uint lparam, uint timeoutFlags, uint timeout, ref IntPtr retval);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler)
                : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
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

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WNDCLASSEX
        {
            public int cbSize; // Size in bytes of the WNDCLASSEX structure
            public int style;   // Class style
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
        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder sb, int Length);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClassLong(IntPtr handle, int longClass);

        [DllImport("user32.dll")]
        public static extern IntPtr GetClassLongPtr(IntPtr handle, int longClass);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr handle, int wCmd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam,
            ref RECT pvParam, uint fWinIni);

        /// <summary>For SystemParametersInfo</summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-systemparametersinfoa
        public enum SPI : int
        {
            SPI_SETWORKAREA = 0x002F,
            SPI_GETWORKAREA = 0x0030,

            SPI_GETMINIMIZEDMETRICS = 0x002B,
            SPI_SETMINIMIZEDMETRICS = 0x002C,

            SPI_GETDESKWALLPAPER = 0x0073,
            SPI_SETDESKWALLPAPER = 0x0014
        }

        public enum AppBarNotifications
        {
            // Notifies an appbar that the taskbar's autohide or 
            // always-on-top state has changed—that is, the user has selected 
            // or cleared the "Always on top" or "Auto hide" check box on the
            // taskbar's property sheet. 
            StateChange = 0x00000000,
            // Notifies an appbar when an event has occurred that may affect 
            // the appbar's size and position. Events include changes in the
            // taskbar's size, position, and visibility state, as well as the
            // addition, removal, or resizing of another appbar on the same 
            // side of the screen.
            PosChanged = 0x00000001,
            // Notifies an appbar when a full-screen application is opening or
            // closing. This notification is sent in the form of an 
            // application-defined message that is set by the ABM_NEW message. 
            FullScreenApp = 0x00000002,
            // Notifies an appbar that the user has selected the Cascade, 
            // Tile Horizontally, or Tile Vertically command from the 
            // taskbar's shortcut menu.
            WindowArrange = 0x00000003
        }

        /// <summary>
        /// Windows Messages
        /// Defined in winuser.h from Windows SDK v6.1
        /// Documentation pulled from MSDN.
        /// </summary>
        public enum WM : uint
        {
            /// <summary>
            /// The WM_NULL message performs no operation. An application sends the WM_NULL message if it wants to post a message that the recipient window will ignore.
            /// </summary>
            NULL = 0x0000,
            /// <summary>
            /// The WM_CREATE message is sent when an application requests that a window be created by calling the CreateWindowEx or CreateWindow function. (The message is sent before the function returns.) The window procedure of the new window receives this message after the window is created, but before the window becomes visible.
            /// </summary>
            CREATE = 0x0001,
            /// <summary>
            /// The WM_DESTROY message is sent when a window is being destroyed. It is sent to the window procedure of the window being destroyed after the window is removed from the screen. 
            /// This message is sent first to the window being destroyed and then to the child windows (if any) as they are destroyed. During the processing of the message, it can be assumed that all child windows still exist.
            /// /// </summary>
            DESTROY = 0x0002,
            /// <summary>
            /// The WM_MOVE message is sent after a window has been moved. 
            /// </summary>
            MOVE = 0x0003,
            /// <summary>
            /// The WM_SIZE message is sent to a window after its size has changed.
            /// </summary>
            SIZE = 0x0005,
            /// <summary>
            /// The WM_ACTIVATE message is sent to both the window being activated and the window being deactivated. If the windows use the same input queue, the message is sent synchronously, first to the window procedure of the top-level window being deactivated, then to the window procedure of the top-level window being activated. If the windows use different input queues, the message is sent asynchronously, so the window is activated immediately. 
            /// </summary>
            ACTIVATE = 0x0006,
            /// <summary>
            /// The WM_SETFOCUS message is sent to a window after it has gained the keyboard focus. 
            /// </summary>
            SETFOCUS = 0x0007,
            /// <summary>
            /// The WM_KILLFOCUS message is sent to a window immediately before it loses the keyboard focus. 
            /// </summary>
            KILLFOCUS = 0x0008,
            /// <summary>
            /// The WM_ENABLE message is sent when an application changes the enabled state of a window. It is sent to the window whose enabled state is changing. This message is sent before the EnableWindow function returns, but after the enabled state (WS_DISABLED style bit) of the window has changed. 
            /// </summary>
            ENABLE = 0x000A,
            /// <summary>
            /// An application sends the WM_SETREDRAW message to a window to allow changes in that window to be redrawn or to prevent changes in that window from being redrawn. 
            /// </summary>
            SETREDRAW = 0x000B,
            /// <summary>
            /// An application sends a WM_SETTEXT message to set the text of a window. 
            /// </summary>
            SETTEXT = 0x000C,
            /// <summary>
            /// An application sends a WM_GETTEXT message to copy the text that corresponds to a window into a buffer provided by the caller. 
            /// </summary>
            GETTEXT = 0x000D,
            /// <summary>
            /// An application sends a WM_GETTEXTLENGTH message to determine the length, in characters, of the text associated with a window. 
            /// </summary>
            GETTEXTLENGTH = 0x000E,
            /// <summary>
            /// The WM_PAINT message is sent when the system or another application makes a request to paint a portion of an application's window. The message is sent when the UpdateWindow or RedrawWindow function is called, or by the DispatchMessage function when the application obtains a WM_PAINT message by using the GetMessage or PeekMessage function. 
            /// </summary>
            PAINT = 0x000F,
            /// <summary>
            /// The WM_CLOSE message is sent as a signal that a window or an application should terminate.
            /// </summary>
            CLOSE = 0x0010,
            /// <summary>
            /// The WM_QUERYENDSESSION message is sent when the user chooses to end the session or when an application calls one of the system shutdown functions. If any application returns zero, the session is not ended. The system stops sending WM_QUERYENDSESSION messages as soon as one application returns zero.
            /// After processing this message, the system sends the WM_ENDSESSION message with the wParam parameter set to the results of the WM_QUERYENDSESSION message.
            /// </summary>
            QUERYENDSESSION = 0x0011,
            /// <summary>
            /// The WM_QUERYOPEN message is sent to an icon when the user requests that the window be restored to its previous size and position.
            /// </summary>
            QUERYOPEN = 0x0013,
            /// <summary>
            /// The WM_ENDSESSION message is sent to an application after the system processes the results of the WM_QUERYENDSESSION message. The WM_ENDSESSION message informs the application whether the session is ending.
            /// </summary>
            ENDSESSION = 0x0016,
            /// <summary>
            /// The WM_QUIT message indicates a request to terminate an application and is generated when the application calls the PostQuitMessage function. It causes the GetMessage function to return zero.
            /// </summary>
            QUIT = 0x0012,
            /// <summary>
            /// The WM_ERASEBKGND message is sent when the window background must be erased (for example, when a window is resized). The message is sent to prepare an invalidated portion of a window for painting. 
            /// </summary>
            ERASEBKGND = 0x0014,
            /// <summary>
            /// This message is sent to all top-level windows when a change is made to a system color setting. 
            /// </summary>
            SYSCOLORCHANGE = 0x0015,
            /// <summary>
            /// The WM_SHOWWINDOW message is sent to a window when the window is about to be hidden or shown.
            /// </summary>
            SHOWWINDOW = 0x0018,
            /// <summary>
            /// An application sends the WM_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.
            /// Note  The WM_WININICHANGE message is provided only for compatibility with earlier versions of the system. Applications should use the WM_SETTINGCHANGE message.
            /// </summary>
            WININICHANGE = 0x001A,
            /// <summary>
            /// An application sends the WM_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.
            /// Note  The WM_WININICHANGE message is provided only for compatibility with earlier versions of the system. Applications should use the WM_SETTINGCHANGE message.
            /// </summary>
            SETTINGCHANGE = WININICHANGE,
            /// <summary>
            /// The WM_DEVMODECHANGE message is sent to all top-level windows whenever the user changes device-mode settings. 
            /// </summary>
            DEVMODECHANGE = 0x001B,
            /// <summary>
            /// The WM_ACTIVATEAPP message is sent when a window belonging to a different application than the active window is about to be activated. The message is sent to the application whose window is being activated and to the application whose window is being deactivated.
            /// </summary>
            ACTIVATEAPP = 0x001C,
            /// <summary>
            /// An application sends the WM_FONTCHANGE message to all top-level windows in the system after changing the pool of font resources. 
            /// </summary>
            FONTCHANGE = 0x001D,
            /// <summary>
            /// A message that is sent whenever there is a change in the system time.
            /// </summary>
            TIMECHANGE = 0x001E,
            /// <summary>
            /// The WM_CANCELMODE message is sent to cancel certain modes, such as mouse capture. For example, the system sends this message to the active window when a dialog box or message box is displayed. Certain functions also send this message explicitly to the specified window regardless of whether it is the active window. For example, the EnableWindow function sends this message when disabling the specified window.
            /// </summary>
            CANCELMODE = 0x001F,
            /// <summary>
            /// The WM_SETCURSOR message is sent to a window if the mouse causes the cursor to move within a window and mouse input is not captured. 
            /// </summary>
            SETCURSOR = 0x0020,
            /// <summary>
            /// The WM_MOUSEACTIVATE message is sent when the cursor is in an inactive window and the user presses a mouse button. The parent window receives this message only if the child window passes it to the DefWindowProc function.
            /// </summary>
            MOUSEACTIVATE = 0x0021,
            /// <summary>
            /// The WM_CHILDACTIVATE message is sent to a child window when the user clicks the window's title bar or when the window is activated, moved, or sized.
            /// </summary>
            CHILDACTIVATE = 0x0022,
            /// <summary>
            /// The WM_QUEUESYNC message is sent by a computer-based training (CBT) application to separate user-input messages from other messages sent through the WH_JOURNALPLAYBACK Hook procedure. 
            /// </summary>
            QUEUESYNC = 0x0023,
            /// <summary>
            /// The WM_GETMINMAXINFO message is sent to a window when the size or position of the window is about to change. An application can use this message to override the window's default maximized size and position, or its default minimum or maximum tracking size. 
            /// </summary>
            GETMINMAXINFO = 0x0024,
            /// <summary>
            /// Windows NT 3.51 and earlier: The WM_PAINTICON message is sent to a minimized window when the icon is to be painted. This message is not sent by newer versions of Microsoft Windows, except in unusual circumstances explained in the Remarks.
            /// </summary>
            PAINTICON = 0x0026,
            /// <summary>
            /// Windows NT 3.51 and earlier: The WM_ICONERASEBKGND message is sent to a minimized window when the background of the icon must be filled before painting the icon. A window receives this message only if a class icon is defined for the window; otherwise, WM_ERASEBKGND is sent. This message is not sent by newer versions of Windows.
            /// </summary>
            ICONERASEBKGND = 0x0027,
            /// <summary>
            /// The WM_NEXTDLGCTL message is sent to a dialog box procedure to set the keyboard focus to a different control in the dialog box. 
            /// </summary>
            NEXTDLGCTL = 0x0028,
            /// <summary>
            /// The WM_SPOOLERSTATUS message is sent from Print Manager whenever a job is added to or removed from the Print Manager queue. 
            /// </summary>
            SPOOLERSTATUS = 0x002A,
            /// <summary>
            /// The WM_DRAWITEM message is sent to the parent window of an owner-drawn button, combo box, list box, or menu when a visual aspect of the button, combo box, list box, or menu has changed.
            /// </summary>
            DRAWITEM = 0x002B,
            /// <summary>
            /// The WM_MEASUREITEM message is sent to the owner window of a combo box, list box, list view control, or menu item when the control or menu is created.
            /// </summary>
            MEASUREITEM = 0x002C,
            /// <summary>
            /// Sent to the owner of a list box or combo box when the list box or combo box is destroyed or when items are removed by the LB_DELETESTRING, LB_RESETCONTENT, CB_DELETESTRING, or CB_RESETCONTENT message. The system sends a WM_DELETEITEM message for each deleted item. The system sends the WM_DELETEITEM message for any deleted list box or combo box item with nonzero item data.
            /// </summary>
            DELETEITEM = 0x002D,
            /// <summary>
            /// Sent by a list box with the LBS_WANTKEYBOARDINPUT style to its owner in response to a WM_KEYDOWN message. 
            /// </summary>
            VKEYTOITEM = 0x002E,
            /// <summary>
            /// Sent by a list box with the LBS_WANTKEYBOARDINPUT style to its owner in response to a WM_CHAR message. 
            /// </summary>
            CHARTOITEM = 0x002F,
            /// <summary>
            /// An application sends a WM_SETFONT message to specify the font that a control is to use when drawing text. 
            /// </summary>
            SETFONT = 0x0030,
            /// <summary>
            /// An application sends a WM_GETFONT message to a control to retrieve the font with which the control is currently drawing its text. 
            /// </summary>
            GETFONT = 0x0031,
            /// <summary>
            /// An application sends a WM_SETHOTKEY message to a window to associate a hot key with the window. When the user presses the hot key, the system activates the window. 
            /// </summary>
            SETHOTKEY = 0x0032,
            /// <summary>
            /// An application sends a WM_GETHOTKEY message to determine the hot key associated with a window. 
            /// </summary>
            GETHOTKEY = 0x0033,
            /// <summary>
            /// The WM_QUERYDRAGICON message is sent to a minimized (iconic) window. The window is about to be dragged by the user but does not have an icon defined for its class. An application can return a handle to an icon or cursor. The system displays this cursor or icon while the user drags the icon.
            /// </summary>
            QUERYDRAGICON = 0x0037,
            /// <summary>
            /// The system sends the WM_COMPAREITEM message to determine the relative position of a new item in the sorted list of an owner-drawn combo box or list box. Whenever the application adds a new item, the system sends this message to the owner of a combo box or list box created with the CBS_SORT or LBS_SORT style. 
            /// </summary>
            COMPAREITEM = 0x0039,
            /// <summary>
            /// Active Accessibility sends the WM_GETOBJECT message to obtain information about an accessible object contained in a server application. 
            /// Applications never send this message directly. It is sent only by Active Accessibility in response to calls to AccessibleObjectFromPoint, AccessibleObjectFromEvent, or AccessibleObjectFromWindow. However, server applications handle this message. 
            /// </summary>
            GETOBJECT = 0x003D,
            /// <summary>
            /// The WM_COMPACTING message is sent to all top-level windows when the system detects more than 12.5 percent of system time over a 30- to 60-second interval is being spent compacting memory. This indicates that system memory is low.
            /// </summary>
            COMPACTING = 0x0041,
            /// <summary>
            /// WM_COMMNOTIFY is Obsolete for Win32-Based Applications
            /// </summary>
            [Obsolete]
            COMMNOTIFY = 0x0044,
            /// <summary>
            /// The WM_WINDOWPOSCHANGING message is sent to a window whose size, position, or place in the Z order is about to change as a result of a call to the SetWindowPos function or another window-management function.
            /// </summary>
            WINDOWPOSCHANGING = 0x0046,
            /// <summary>
            /// The WM_WINDOWPOSCHANGED message is sent to a window whose size, position, or place in the Z order has changed as a result of a call to the SetWindowPos function or another window-management function.
            /// </summary>
            WINDOWPOSCHANGED = 0x0047,
            /// <summary>
            /// Notifies applications that the system, typically a battery-powered personal computer, is about to enter a suspended mode.
            /// Use: POWERBROADCAST
            /// </summary>
            [Obsolete]
            POWER = 0x0048,
            /// <summary>
            /// An application sends the WM_COPYDATA message to pass data to another application. 
            /// </summary>
            COPYDATA = 0x004A,
            /// <summary>
            /// The WM_CANCELJOURNAL message is posted to an application when a user cancels the application's journaling activities. The message is posted with a NULL window handle. 
            /// </summary>
            CANCELJOURNAL = 0x004B,
            /// <summary>
            /// Sent by a common control to its parent window when an event has occurred or the control requires some information. 
            /// </summary>
            NOTIFY = 0x004E,
            /// <summary>
            /// The WM_INPUTLANGCHANGEREQUEST message is posted to the window with the focus when the user chooses a new input language, either with the hotkey (specified in the Keyboard control panel application) or from the indicator on the system taskbar. An application can accept the change by passing the message to the DefWindowProc function or reject the change (and prevent it from taking place) by returning immediately. 
            /// </summary>
            INPUTLANGCHANGEREQUEST = 0x0050,
            /// <summary>
            /// The WM_INPUTLANGCHANGE message is sent to the topmost affected window after an application's input language has been changed. You should make any application-specific settings and pass the message to the DefWindowProc function, which passes the message to all first-level child windows. These child windows can pass the message to DefWindowProc to have it pass the message to their child windows, and so on. 
            /// </summary>
            INPUTLANGCHANGE = 0x0051,
            /// <summary>
            /// Sent to an application that has initiated a training card with Microsoft Windows Help. The message informs the application when the user clicks an authorable button. An application initiates a training card by specifying the HELP_TCARD command in a call to the WinHelp function.
            /// </summary>
            TCARD = 0x0052,
            /// <summary>
            /// Indicates that the user pressed the F1 key. If a menu is active when F1 is pressed, WM_HELP is sent to the window associated with the menu; otherwise, WM_HELP is sent to the window that has the keyboard focus. If no window has the keyboard focus, WM_HELP is sent to the currently active window. 
            /// </summary>
            HELP = 0x0053,
            /// <summary>
            /// The WM_USERCHANGED message is sent to all windows after the user has logged on or off. When the user logs on or off, the system updates the user-specific settings. The system sends this message immediately after updating the settings.
            /// </summary>
            USERCHANGED = 0x0054,
            /// <summary>
            /// Determines if a window accepts ANSI or Unicode structures in the WM_NOTIFY notification message. WM_NOTIFYFORMAT messages are sent from a common control to its parent window and from the parent window to the common control.
            /// </summary>
            NOTIFYFORMAT = 0x0055,
            /// <summary>
            /// The WM_CONTEXTMENU message notifies a window that the user clicked the right mouse button (right-clicked) in the window.
            /// </summary>
            CONTEXTMENU = 0x007B,
            /// <summary>
            /// The WM_STYLECHANGING message is sent to a window when the SetWindowLong function is about to change one or more of the window's styles.
            /// </summary>
            STYLECHANGING = 0x007C,
            /// <summary>
            /// The WM_STYLECHANGED message is sent to a window after the SetWindowLong function has changed one or more of the window's styles
            /// </summary>
            STYLECHANGED = 0x007D,
            /// <summary>
            /// The WM_DISPLAYCHANGE message is sent to all windows when the display resolution has changed.
            /// </summary>
            DISPLAYCHANGE = 0x007E,
            /// <summary>
            /// The WM_GETICON message is sent to a window to retrieve a handle to the large or small icon associated with a window. The system displays the large icon in the ALT+TAB dialog, and the small icon in the window caption. 
            /// </summary>
            GETICON = 0x007F,
            /// <summary>
            /// An application sends the WM_SETICON message to associate a new large or small icon with a window. The system displays the large icon in the ALT+TAB dialog box, and the small icon in the window caption. 
            /// </summary>
            SETICON = 0x0080,
            /// <summary>
            /// The WM_NCCREATE message is sent prior to the WM_CREATE message when a window is first created.
            /// </summary>
            NCCREATE = 0x0081,
            /// <summary>
            /// The WM_NCDESTROY message informs a window that its nonclient area is being destroyed. The DestroyWindow function sends the WM_NCDESTROY message to the window following the WM_DESTROY message. WM_DESTROY is used to free the allocated memory object associated with the window. 
            /// The WM_NCDESTROY message is sent after the child windows have been destroyed. In contrast, WM_DESTROY is sent before the child windows are destroyed.
            /// </summary>
            NCDESTROY = 0x0082,
            /// <summary>
            /// The WM_NCCALCSIZE message is sent when the size and position of a window's client area must be calculated. By processing this message, an application can control the content of the window's client area when the size or position of the window changes.
            /// </summary>
            NCCALCSIZE = 0x0083,
            /// <summary>
            /// The WM_NCHITTEST message is sent to a window when the cursor moves, or when a mouse button is pressed or released. If the mouse is not captured, the message is sent to the window beneath the cursor. Otherwise, the message is sent to the window that has captured the mouse.
            /// </summary>
            NCHITTEST = 0x0084,
            /// <summary>
            /// The WM_NCPAINT message is sent to a window when its frame must be painted. 
            /// </summary>
            NCPAINT = 0x0085,
            /// <summary>
            /// The WM_NCACTIVATE message is sent to a window when its nonclient area needs to be changed to indicate an active or inactive state.
            /// </summary>
            NCACTIVATE = 0x0086,
            /// <summary>
            /// The WM_GETDLGCODE message is sent to the window procedure associated with a control. By default, the system handles all keyboard input to the control; the system interprets certain types of keyboard input as dialog box navigation keys. To override this default behavior, the control can respond to the WM_GETDLGCODE message to indicate the types of input it wants to process itself.
            /// </summary>
            GETDLGCODE = 0x0087,
            /// <summary>
            /// The WM_SYNCPAINT message is used to synchronize painting while avoiding linking independent GUI threads.
            /// </summary>
            SYNCPAINT = 0x0088,
            /// <summary>
            /// The WM_NCMOUSEMOVE message is posted to a window when the cursor is moved within the nonclient area of the window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCMOUSEMOVE = 0x00A0,
            /// <summary>
            /// The WM_NCLBUTTONDOWN message is posted when the user presses the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCLBUTTONDOWN = 0x00A1,
            /// <summary>
            /// The WM_NCLBUTTONUP message is posted when the user releases the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCLBUTTONUP = 0x00A2,
            /// <summary>
            /// The WM_NCLBUTTONDBLCLK message is posted when the user double-clicks the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCLBUTTONDBLCLK = 0x00A3,
            /// <summary>
            /// The WM_NCRBUTTONDOWN message is posted when the user presses the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCRBUTTONDOWN = 0x00A4,
            /// <summary>
            /// The WM_NCRBUTTONUP message is posted when the user releases the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCRBUTTONUP = 0x00A5,
            /// <summary>
            /// The WM_NCRBUTTONDBLCLK message is posted when the user double-clicks the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCRBUTTONDBLCLK = 0x00A6,
            /// <summary>
            /// The WM_NCMBUTTONDOWN message is posted when the user presses the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCMBUTTONDOWN = 0x00A7,
            /// <summary>
            /// The WM_NCMBUTTONUP message is posted when the user releases the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCMBUTTONUP = 0x00A8,
            /// <summary>
            /// The WM_NCMBUTTONDBLCLK message is posted when the user double-clicks the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCMBUTTONDBLCLK = 0x00A9,
            /// <summary>
            /// The WM_NCXBUTTONDOWN message is posted when the user presses the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCXBUTTONDOWN = 0x00AB,
            /// <summary>
            /// The WM_NCXBUTTONUP message is posted when the user releases the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCXBUTTONUP = 0x00AC,
            /// <summary>
            /// The WM_NCXBUTTONDBLCLK message is posted when the user double-clicks the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            NCXBUTTONDBLCLK = 0x00AD,
            /// <summary>
            /// The WM_INPUT_DEVICE_CHANGE message is sent to the window that registered to receive raw input. A window receives this message through its WindowProc function.
            /// </summary>
            INPUT_DEVICE_CHANGE = 0x00FE,
            /// <summary>
            /// The WM_INPUT message is sent to the window that is getting raw input. 
            /// </summary>
            INPUT = 0x00FF,
            /// <summary>
            /// This message filters for keyboard messages.
            /// </summary>
            KEYFIRST = 0x0100,
            /// <summary>
            /// The WM_KEYDOWN message is posted to the window with the keyboard focus when a nonsystem key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed. 
            /// </summary>
            KEYDOWN = 0x0100,
            /// <summary>
            /// The WM_KEYUP message is posted to the window with the keyboard focus when a nonsystem key is released. A nonsystem key is a key that is pressed when the ALT key is not pressed, or a keyboard key that is pressed when a window has the keyboard focus. 
            /// </summary>
            KEYUP = 0x0101,
            /// <summary>
            /// The WM_CHAR message is posted to the window with the keyboard focus when a WM_KEYDOWN message is translated by the TranslateMessage function. The WM_CHAR message contains the character code of the key that was pressed. 
            /// </summary>
            CHAR = 0x0102,
            /// <summary>
            /// The WM_DEADCHAR message is posted to the window with the keyboard focus when a WM_KEYUP message is translated by the TranslateMessage function. WM_DEADCHAR specifies a character code generated by a dead key. A dead key is a key that generates a character, such as the umlaut (double-dot), that is combined with another character to form a composite character. For example, the umlaut-O character (Ö) is generated by typing the dead key for the umlaut character, and then typing the O key. 
            /// </summary>
            DEADCHAR = 0x0103,
            /// <summary>
            /// The WM_SYSKEYDOWN message is posted to the window with the keyboard focus when the user presses the F10 key (which activates the menu bar) or holds down the ALT key and then presses another key. It also occurs when no window currently has the keyboard focus; in this case, the WM_SYSKEYDOWN message is sent to the active window. The window that receives the message can distinguish between these two contexts by checking the context code in the lParam parameter. 
            /// </summary>
            SYSKEYDOWN = 0x0104,
            /// <summary>
            /// The WM_SYSKEYUP message is posted to the window with the keyboard focus when the user releases a key that was pressed while the ALT key was held down. It also occurs when no window currently has the keyboard focus; in this case, the WM_SYSKEYUP message is sent to the active window. The window that receives the message can distinguish between these two contexts by checking the context code in the lParam parameter. 
            /// </summary>
            SYSKEYUP = 0x0105,
            /// <summary>
            /// The WM_SYSCHAR message is posted to the window with the keyboard focus when a WM_SYSKEYDOWN message is translated by the TranslateMessage function. It specifies the character code of a system character key — that is, a character key that is pressed while the ALT key is down. 
            /// </summary>
            SYSCHAR = 0x0106,
            /// <summary>
            /// The WM_SYSDEADCHAR message is sent to the window with the keyboard focus when a WM_SYSKEYDOWN message is translated by the TranslateMessage function. WM_SYSDEADCHAR specifies the character code of a system dead key — that is, a dead key that is pressed while holding down the ALT key. 
            /// </summary>
            SYSDEADCHAR = 0x0107,
            /// <summary>
            /// The WM_UNICHAR message is posted to the window with the keyboard focus when a WM_KEYDOWN message is translated by the TranslateMessage function. The WM_UNICHAR message contains the character code of the key that was pressed. 
            /// The WM_UNICHAR message is equivalent to WM_CHAR, but it uses Unicode Transformation Format (UTF)-32, whereas WM_CHAR uses UTF-16. It is designed to send or post Unicode characters to ANSI windows and it can can handle Unicode Supplementary Plane characters.
            /// </summary>
            UNICHAR = 0x0109,
            /// <summary>
            /// This message filters for keyboard messages.
            /// </summary>
            KEYLAST = 0x0108,
            /// <summary>
            /// Sent immediately before the IME generates the composition string as a result of a keystroke. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_STARTCOMPOSITION = 0x010D,
            /// <summary>
            /// Sent to an application when the IME ends composition. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_ENDCOMPOSITION = 0x010E,
            /// <summary>
            /// Sent to an application when the IME changes composition status as a result of a keystroke. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_COMPOSITION = 0x010F,
            IME_KEYLAST = 0x010F,
            /// <summary>
            /// The WM_INITDIALOG message is sent to the dialog box procedure immediately before a dialog box is displayed. Dialog box procedures typically use this message to initialize controls and carry out any other initialization tasks that affect the appearance of the dialog box. 
            /// </summary>
            INITDIALOG = 0x0110,
            /// <summary>
            /// The WM_COMMAND message is sent when the user selects a command item from a menu, when a control sends a notification message to its parent window, or when an accelerator keystroke is translated. 
            /// </summary>
            COMMAND = 0x0111,
            /// <summary>
            /// A window receives this message when the user chooses a command from the Window menu, clicks the maximize button, minimize button, restore button, close button, or moves the form. You can stop the form from moving by filtering this out.
            /// </summary>
            SYSCOMMAND = 0x0112,
            /// <summary>
            /// The WM_TIMER message is posted to the installing thread's message queue when a timer expires. The message is posted by the GetMessage or PeekMessage function. 
            /// </summary>
            TIMER = 0x0113,
            /// <summary>
            /// The WM_HSCROLL message is sent to a window when a scroll event occurs in the window's standard horizontal scroll bar. This message is also sent to the owner of a horizontal scroll bar control when a scroll event occurs in the control. 
            /// </summary>
            HSCROLL = 0x0114,
            /// <summary>
            /// The WM_VSCROLL message is sent to a window when a scroll event occurs in the window's standard vertical scroll bar. This message is also sent to the owner of a vertical scroll bar control when a scroll event occurs in the control. 
            /// </summary>
            VSCROLL = 0x0115,
            /// <summary>
            /// The WM_INITMENU message is sent when a menu is about to become active. It occurs when the user clicks an item on the menu bar or presses a menu key. This allows the application to modify the menu before it is displayed. 
            /// </summary>
            INITMENU = 0x0116,
            /// <summary>
            /// The WM_INITMENUPOPUP message is sent when a drop-down menu or submenu is about to become active. This allows an application to modify the menu before it is displayed, without changing the entire menu. 
            /// </summary>
            INITMENUPOPUP = 0x0117,
            /// <summary>
            /// The WM_MENUSELECT message is sent to a menu's owner window when the user selects a menu item. 
            /// </summary>
            MENUSELECT = 0x011F,
            /// <summary>
            /// The WM_MENUCHAR message is sent when a menu is active and the user presses a key that does not correspond to any mnemonic or accelerator key. This message is sent to the window that owns the menu. 
            /// </summary>
            MENUCHAR = 0x0120,
            /// <summary>
            /// The WM_ENTERIDLE message is sent to the owner window of a modal dialog box or menu that is entering an idle state. A modal dialog box or menu enters an idle state when no messages are waiting in its queue after it has processed one or more previous messages. 
            /// </summary>
            ENTERIDLE = 0x0121,
            /// <summary>
            /// The WM_MENURBUTTONUP message is sent when the user releases the right mouse button while the cursor is on a menu item. 
            /// </summary>
            MENURBUTTONUP = 0x0122,
            /// <summary>
            /// The WM_MENUDRAG message is sent to the owner of a drag-and-drop menu when the user drags a menu item. 
            /// </summary>
            MENUDRAG = 0x0123,
            /// <summary>
            /// The WM_MENUGETOBJECT message is sent to the owner of a drag-and-drop menu when the mouse cursor enters a menu item or moves from the center of the item to the top or bottom of the item. 
            /// </summary>
            MENUGETOBJECT = 0x0124,
            /// <summary>
            /// The WM_UNINITMENUPOPUP message is sent when a drop-down menu or submenu has been destroyed. 
            /// </summary>
            UNINITMENUPOPUP = 0x0125,
            /// <summary>
            /// The WM_MENUCOMMAND message is sent when the user makes a selection from a menu. 
            /// </summary>
            MENUCOMMAND = 0x0126,
            /// <summary>
            /// An application sends the WM_CHANGEUISTATE message to indicate that the user interface (UI) state should be changed.
            /// </summary>
            CHANGEUISTATE = 0x0127,
            /// <summary>
            /// An application sends the WM_UPDATEUISTATE message to change the user interface (UI) state for the specified window and all its child windows.
            /// </summary>
            UPDATEUISTATE = 0x0128,
            /// <summary>
            /// An application sends the WM_QUERYUISTATE message to retrieve the user interface (UI) state for a window.
            /// </summary>
            QUERYUISTATE = 0x0129,
            /// <summary>
            /// The WM_CTLCOLORMSGBOX message is sent to the owner window of a message box before Windows draws the message box. By responding to this message, the owner window can set the text and background colors of the message box by using the given display device context handle. 
            /// </summary>
            CTLCOLORMSGBOX = 0x0132,
            /// <summary>
            /// An edit control that is not read-only or disabled sends the WM_CTLCOLOREDIT message to its parent window when the control is about to be drawn. By responding to this message, the parent window can use the specified device context handle to set the text and background colors of the edit control. 
            /// </summary>
            CTLCOLOREDIT = 0x0133,
            /// <summary>
            /// Sent to the parent window of a list box before the system draws the list box. By responding to this message, the parent window can set the text and background colors of the list box by using the specified display device context handle. 
            /// </summary>
            CTLCOLORLISTBOX = 0x0134,
            /// <summary>
            /// The WM_CTLCOLORBTN message is sent to the parent window of a button before drawing the button. The parent window can change the button's text and background colors. However, only owner-drawn buttons respond to the parent window processing this message. 
            /// </summary>
            CTLCOLORBTN = 0x0135,
            /// <summary>
            /// The WM_CTLCOLORDLG message is sent to a dialog box before the system draws the dialog box. By responding to this message, the dialog box can set its text and background colors using the specified display device context handle. 
            /// </summary>
            CTLCOLORDLG = 0x0136,
            /// <summary>
            /// The WM_CTLCOLORSCROLLBAR message is sent to the parent window of a scroll bar control when the control is about to be drawn. By responding to this message, the parent window can use the display context handle to set the background color of the scroll bar control. 
            /// </summary>
            CTLCOLORSCROLLBAR = 0x0137,
            /// <summary>
            /// A static control, or an edit control that is read-only or disabled, sends the WM_CTLCOLORSTATIC message to its parent window when the control is about to be drawn. By responding to this message, the parent window can use the specified device context handle to set the text and background colors of the static control. 
            /// </summary>
            CTLCOLORSTATIC = 0x0138,
            /// <summary>
            /// Use WM_MOUSEFIRST to specify the first mouse message. Use the PeekMessage() Function.
            /// </summary>
            MOUSEFIRST = 0x0200,
            /// <summary>
            /// The WM_MOUSEMOVE message is posted to a window when the cursor moves. If the mouse is not captured, the message is posted to the window that contains the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            MOUSEMOVE = 0x0200,
            /// <summary>
            /// The WM_LBUTTONDOWN message is posted when the user presses the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            LBUTTONDOWN = 0x0201,
            /// <summary>
            /// The WM_LBUTTONUP message is posted when the user releases the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            LBUTTONUP = 0x0202,
            /// <summary>
            /// The WM_LBUTTONDBLCLK message is posted when the user double-clicks the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            LBUTTONDBLCLK = 0x0203,
            /// <summary>
            /// The WM_RBUTTONDOWN message is posted when the user presses the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            RBUTTONDOWN = 0x0204,
            /// <summary>
            /// The WM_RBUTTONUP message is posted when the user releases the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            RBUTTONUP = 0x0205,
            /// <summary>
            /// The WM_RBUTTONDBLCLK message is posted when the user double-clicks the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            RBUTTONDBLCLK = 0x0206,
            /// <summary>
            /// The WM_MBUTTONDOWN message is posted when the user presses the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            MBUTTONDOWN = 0x0207,
            /// <summary>
            /// The WM_MBUTTONUP message is posted when the user releases the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            MBUTTONUP = 0x0208,
            /// <summary>
            /// The WM_MBUTTONDBLCLK message is posted when the user double-clicks the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            MBUTTONDBLCLK = 0x0209,
            /// <summary>
            /// The WM_MOUSEWHEEL message is sent to the focus window when the mouse wheel is rotated. The DefWindowProc function propagates the message to the window's parent. There should be no internal forwarding of the message, since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
            /// </summary>
            MOUSEWHEEL = 0x020A,
            /// <summary>
            /// The WM_XBUTTONDOWN message is posted when the user presses the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse. 
            /// </summary>
            XBUTTONDOWN = 0x020B,
            /// <summary>
            /// The WM_XBUTTONUP message is posted when the user releases the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            XBUTTONUP = 0x020C,
            /// <summary>
            /// The WM_XBUTTONDBLCLK message is posted when the user double-clicks the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            XBUTTONDBLCLK = 0x020D,
            /// <summary>
            /// The WM_MOUSEHWHEEL message is sent to the focus window when the mouse's horizontal scroll wheel is tilted or rotated. The DefWindowProc function propagates the message to the window's parent. There should be no internal forwarding of the message, since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
            /// </summary>
            MOUSEHWHEEL = 0x020E,
            /// <summary>
            /// Use WM_MOUSELAST to specify the last mouse message. Used with PeekMessage() Function.
            /// </summary>
            MOUSELAST = 0x020E,
            /// <summary>
            /// The WM_PARENTNOTIFY message is sent to the parent of a child window when the child window is created or destroyed, or when the user clicks a mouse button while the cursor is over the child window. When the child window is being created, the system sends WM_PARENTNOTIFY just before the CreateWindow or CreateWindowEx function that creates the window returns. When the child window is being destroyed, the system sends the message before any processing to destroy the window takes place.
            /// </summary>
            PARENTNOTIFY = 0x0210,
            /// <summary>
            /// The WM_ENTERMENULOOP message informs an application's main window procedure that a menu modal loop has been entered. 
            /// </summary>
            ENTERMENULOOP = 0x0211,
            /// <summary>
            /// The WM_EXITMENULOOP message informs an application's main window procedure that a menu modal loop has been exited. 
            /// </summary>
            EXITMENULOOP = 0x0212,
            /// <summary>
            /// The WM_NEXTMENU message is sent to an application when the right or left arrow key is used to switch between the menu bar and the system menu. 
            /// </summary>
            NEXTMENU = 0x0213,
            /// <summary>
            /// The WM_SIZING message is sent to a window that the user is resizing. By processing this message, an application can monitor the size and position of the drag rectangle and, if needed, change its size or position. 
            /// </summary>
            SIZING = 0x0214,
            /// <summary>
            /// The WM_CAPTURECHANGED message is sent to the window that is losing the mouse capture.
            /// </summary>
            CAPTURECHANGED = 0x0215,
            /// <summary>
            /// The WM_MOVING message is sent to a window that the user is moving. By processing this message, an application can monitor the position of the drag rectangle and, if needed, change its position.
            /// </summary>
            MOVING = 0x0216,
            /// <summary>
            /// Notifies applications that a power-management event has occurred.
            /// </summary>
            POWERBROADCAST = 0x0218,
            /// <summary>
            /// Notifies an application of a change to the hardware configuration of a device or the computer.
            /// </summary>
            DEVICECHANGE = 0x0219,
            /// <summary>
            /// An application sends the WM_MDICREATE message to a multiple-document interface (MDI) client window to create an MDI child window. 
            /// </summary>
            MDICREATE = 0x0220,
            /// <summary>
            /// An application sends the WM_MDIDESTROY message to a multiple-document interface (MDI) client window to close an MDI child window. 
            /// </summary>
            MDIDESTROY = 0x0221,
            /// <summary>
            /// An application sends the WM_MDIACTIVATE message to a multiple-document interface (MDI) client window to instruct the client window to activate a different MDI child window. 
            /// </summary>
            MDIACTIVATE = 0x0222,
            /// <summary>
            /// An application sends the WM_MDIRESTORE message to a multiple-document interface (MDI) client window to restore an MDI child window from maximized or minimized size. 
            /// </summary>
            MDIRESTORE = 0x0223,
            /// <summary>
            /// An application sends the WM_MDINEXT message to a multiple-document interface (MDI) client window to activate the next or previous child window. 
            /// </summary>
            MDINEXT = 0x0224,
            /// <summary>
            /// An application sends the WM_MDIMAXIMIZE message to a multiple-document interface (MDI) client window to maximize an MDI child window. The system resizes the child window to make its client area fill the client window. The system places the child window's window menu icon in the rightmost position of the frame window's menu bar, and places the child window's restore icon in the leftmost position. The system also appends the title bar text of the child window to that of the frame window. 
            /// </summary>
            MDIMAXIMIZE = 0x0225,
            /// <summary>
            /// An application sends the WM_MDITILE message to a multiple-document interface (MDI) client window to arrange all of its MDI child windows in a tile format. 
            /// </summary>
            MDITILE = 0x0226,
            /// <summary>
            /// An application sends the WM_MDICASCADE message to a multiple-document interface (MDI) client window to arrange all its child windows in a cascade format. 
            /// </summary>
            MDICASCADE = 0x0227,
            /// <summary>
            /// An application sends the WM_MDIICONARRANGE message to a multiple-document interface (MDI) client window to arrange all minimized MDI child windows. It does not affect child windows that are not minimized. 
            /// </summary>
            MDIICONARRANGE = 0x0228,
            /// <summary>
            /// An application sends the WM_MDIGETACTIVE message to a multiple-document interface (MDI) client window to retrieve the handle to the active MDI child window. 
            /// </summary>
            MDIGETACTIVE = 0x0229,
            /// <summary>
            /// An application sends the WM_MDISETMENU message to a multiple-document interface (MDI) client window to replace the entire menu of an MDI frame window, to replace the window menu of the frame window, or both. 
            /// </summary>
            MDISETMENU = 0x0230,
            /// <summary>
            /// The WM_ENTERSIZEMOVE message is sent one time to a window after it enters the moving or sizing modal loop. The window enters the moving or sizing modal loop when the user clicks the window's title bar or sizing border, or when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete when DefWindowProc returns. 
            /// The system sends the WM_ENTERSIZEMOVE message regardless of whether the dragging of full windows is enabled.
            /// </summary>
            ENTERSIZEMOVE = 0x0231,
            /// <summary>
            /// The WM_EXITSIZEMOVE message is sent one time to a window, after it has exited the moving or sizing modal loop. The window enters the moving or sizing modal loop when the user clicks the window's title bar or sizing border, or when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete when DefWindowProc returns. 
            /// </summary>
            EXITSIZEMOVE = 0x0232,
            /// <summary>
            /// Sent when the user drops a file on the window of an application that has registered itself as a recipient of dropped files.
            /// </summary>
            DROPFILES = 0x0233,
            /// <summary>
            /// An application sends the WM_MDIREFRESHMENU message to a multiple-document interface (MDI) client window to refresh the window menu of the MDI frame window. 
            /// </summary>
            MDIREFRESHMENU = 0x0234,
            /// <summary>
            /// Sent to an application when a window is activated. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_SETCONTEXT = 0x0281,
            /// <summary>
            /// Sent to an application to notify it of changes to the IME window. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_NOTIFY = 0x0282,
            /// <summary>
            /// Sent by an application to direct the IME window to carry out the requested command. The application uses this message to control the IME window that it has created. To send this message, the application calls the SendMessage function with the following parameters.
            /// </summary>
            IME_CONTROL = 0x0283,
            /// <summary>
            /// Sent to an application when the IME window finds no space to extend the area for the composition window. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_COMPOSITIONFULL = 0x0284,
            /// <summary>
            /// Sent to an application when the operating system is about to change the current IME. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_SELECT = 0x0285,
            /// <summary>
            /// Sent to an application when the IME gets a character of the conversion result. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_CHAR = 0x0286,
            /// <summary>
            /// Sent to an application to provide commands and request information. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_REQUEST = 0x0288,
            /// <summary>
            /// Sent to an application by the IME to notify the application of a key press and to keep message order. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_KEYDOWN = 0x0290,
            /// <summary>
            /// Sent to an application by the IME to notify the application of a key release and to keep message order. A window receives this message through its WindowProc function. 
            /// </summary>
            IME_KEYUP = 0x0291,
            /// <summary>
            /// The WM_MOUSEHOVER message is posted to a window when the cursor hovers over the client area of the window for the period of time specified in a prior call to TrackMouseEvent.
            /// </summary>
            MOUSEHOVER = 0x02A1,
            /// <summary>
            /// The WM_MOUSELEAVE message is posted to a window when the cursor leaves the client area of the window specified in a prior call to TrackMouseEvent.
            /// </summary>
            MOUSELEAVE = 0x02A3,
            /// <summary>
            /// The WM_NCMOUSEHOVER message is posted to a window when the cursor hovers over the nonclient area of the window for the period of time specified in a prior call to TrackMouseEvent.
            /// </summary>
            NCMOUSEHOVER = 0x02A0,
            /// <summary>
            /// The WM_NCMOUSELEAVE message is posted to a window when the cursor leaves the nonclient area of the window specified in a prior call to TrackMouseEvent.
            /// </summary>
            NCMOUSELEAVE = 0x02A2,
            /// <summary>
            /// The WM_WTSSESSION_CHANGE message notifies applications of changes in session state.
            /// </summary>
            WTSSESSION_CHANGE = 0x02B1,
            TABLET_FIRST = 0x02c0,
            TABLET_LAST = 0x02df,
            /// <summary>
            /// An application sends a WM_CUT message to an edit control or combo box to delete (cut) the current selection, if any, in the edit control and copy the deleted text to the clipboard in CF_TEXT format. 
            /// </summary>
            CUT = 0x0300,
            /// <summary>
            /// An application sends the WM_COPY message to an edit control or combo box to copy the current selection to the clipboard in CF_TEXT format. 
            /// </summary>
            COPY = 0x0301,
            /// <summary>
            /// An application sends a WM_PASTE message to an edit control or combo box to copy the current content of the clipboard to the edit control at the current caret position. Data is inserted only if the clipboard contains data in CF_TEXT format. 
            /// </summary>
            PASTE = 0x0302,
            /// <summary>
            /// An application sends a WM_CLEAR message to an edit control or combo box to delete (clear) the current selection, if any, from the edit control. 
            /// </summary>
            CLEAR = 0x0303,
            /// <summary>
            /// An application sends a WM_UNDO message to an edit control to undo the last operation. When this message is sent to an edit control, the previously deleted text is restored or the previously added text is deleted.
            /// </summary>
            UNDO = 0x0304,
            /// <summary>
            /// The WM_RENDERFORMAT message is sent to the clipboard owner if it has delayed rendering a specific clipboard format and if an application has requested data in that format. The clipboard owner must render data in the specified format and place it on the clipboard by calling the SetClipboardData function. 
            /// </summary>
            RENDERFORMAT = 0x0305,
            /// <summary>
            /// The WM_RENDERALLFORMATS message is sent to the clipboard owner before it is destroyed, if the clipboard owner has delayed rendering one or more clipboard formats. For the content of the clipboard to remain available to other applications, the clipboard owner must render data in all the formats it is capable of generating, and place the data on the clipboard by calling the SetClipboardData function. 
            /// </summary>
            RENDERALLFORMATS = 0x0306,
            /// <summary>
            /// The WM_DESTROYCLIPBOARD message is sent to the clipboard owner when a call to the EmptyClipboard function empties the clipboard. 
            /// </summary>
            DESTROYCLIPBOARD = 0x0307,
            /// <summary>
            /// The WM_DRAWCLIPBOARD message is sent to the first window in the clipboard viewer chain when the content of the clipboard changes. This enables a clipboard viewer window to display the new content of the clipboard. 
            /// </summary>
            DRAWCLIPBOARD = 0x0308,
            /// <summary>
            /// The WM_PAINTCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and the clipboard viewer's client area needs repainting. 
            /// </summary>
            PAINTCLIPBOARD = 0x0309,
            /// <summary>
            /// The WM_VSCROLLCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and an event occurs in the clipboard viewer's vertical scroll bar. The owner should scroll the clipboard image and update the scroll bar values. 
            /// </summary>
            VSCROLLCLIPBOARD = 0x030A,
            /// <summary>
            /// The WM_SIZECLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and the clipboard viewer's client area has changed size. 
            /// </summary>
            SIZECLIPBOARD = 0x030B,
            /// <summary>
            /// The WM_ASKCBFORMATNAME message is sent to the clipboard owner by a clipboard viewer window to request the name of a CF_OWNERDISPLAY clipboard format.
            /// </summary>
            ASKCBFORMATNAME = 0x030C,
            /// <summary>
            /// The WM_CHANGECBCHAIN message is sent to the first window in the clipboard viewer chain when a window is being removed from the chain. 
            /// </summary>
            CHANGECBCHAIN = 0x030D,
            /// <summary>
            /// The WM_HSCROLLCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window. This occurs when the clipboard contains data in the CF_OWNERDISPLAY format and an event occurs in the clipboard viewer's horizontal scroll bar. The owner should scroll the clipboard image and update the scroll bar values. 
            /// </summary>
            HSCROLLCLIPBOARD = 0x030E,
            /// <summary>
            /// This message informs a window that it is about to receive the keyboard focus, giving the window the opportunity to realize its logical palette when it receives the focus. 
            /// </summary>
            QUERYNEWPALETTE = 0x030F,
            /// <summary>
            /// The WM_PALETTEISCHANGING message informs applications that an application is going to realize its logical palette. 
            /// </summary>
            PALETTEISCHANGING = 0x0310,
            /// <summary>
            /// This message is sent by the OS to all top-level and overlapped windows after the window with the keyboard focus realizes its logical palette. 
            /// This message enables windows that do not have the keyboard focus to realize their logical palettes and update their client areas.
            /// </summary>
            PALETTECHANGED = 0x0311,
            /// <summary>
            /// The WM_HOTKEY message is posted when the user presses a hot key registered by the RegisterHotKey function. The message is placed at the top of the message queue associated with the thread that registered the hot key. 
            /// </summary>
            HOTKEY = 0x0312,
            /// <summary>
            /// The WM_PRINT message is sent to a window to request that it draw itself in the specified device context, most commonly in a printer device context.
            /// </summary>
            PRINT = 0x0317,
            /// <summary>
            /// The WM_PRINTCLIENT message is sent to a window to request that it draw its client area in the specified device context, most commonly in a printer device context.
            /// </summary>
            PRINTCLIENT = 0x0318,
            /// <summary>
            /// The WM_APPCOMMAND message notifies a window that the user generated an application command event, for example, by clicking an application command button using the mouse or typing an application command key on the keyboard.
            /// </summary>
            APPCOMMAND = 0x0319,
            /// <summary>
            /// The WM_THEMECHANGED message is broadcast to every window following a theme change event. Examples of theme change events are the activation of a theme, the deactivation of a theme, or a transition from one theme to another.
            /// </summary>
            THEMECHANGED = 0x031A,
            /// <summary>
            /// Sent when the contents of the clipboard have changed.
            /// </summary>
            CLIPBOARDUPDATE = 0x031D,
            /// <summary>
            /// The system will send a window the WM_DWMCOMPOSITIONCHANGED message to indicate that the availability of desktop composition has changed.
            /// </summary>
            DWMCOMPOSITIONCHANGED = 0x031E,
            /// <summary>
            /// WM_DWMNCRENDERINGCHANGED is called when the non-client area rendering status of a window has changed. Only windows that have set the flag DWM_BLURBEHIND.fTransitionOnMaximized to true will get this message. 
            /// </summary>
            DWMNCRENDERINGCHANGED = 0x031F,
            /// <summary>
            /// Sent to all top-level windows when the colorization color has changed. 
            /// </summary>
            DWMCOLORIZATIONCOLORCHANGED = 0x0320,
            /// <summary>
            /// WM_DWMWINDOWMAXIMIZEDCHANGE will let you know when a DWM composed window is maximized. You also have to register for this message as well. You'd have other windowd go opaque when this message is sent.
            /// </summary>
            DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
            /// <summary>
            /// Sent to request extended title bar information. A window receives this message through its WindowProc function.
            /// </summary>
            GETTITLEBARINFOEX = 0x033F,
            HANDHELDFIRST = 0x0358,
            HANDHELDLAST = 0x035F,
            AFXFIRST = 0x0360,
            AFXLAST = 0x037F,
            PENWINFIRST = 0x0380,
            PENWINLAST = 0x038F,

            /// <summary>
            /// The WM_APP constant is used by applications to help define private messages, usually of the form WM_APP+X, where X is an integer value. 
            /// </summary>
            APP = 0x8000,
            /// <summary>
            /// The WM_USER constant is used by applications to help define private messages for use by private window classes, usually of the form WM_USER+X, where X is an integer value. 
            /// </summary>
            USER = 0x0400,

            /// <summary>
            /// An application sends the WM_CPL_LAUNCH message to Windows Control Panel to request that a Control Panel application be started. 
            /// </summary>
            CPL_LAUNCH = USER + 0x1000,
            /// <summary>
            /// The WM_CPL_LAUNCHED message is sent when a Control Panel application, started by the WM_CPL_LAUNCH message, has closed. The WM_CPL_LAUNCHED message is sent to the window identified by the wParam parameter of the WM_CPL_LAUNCH message that started the application. 
            /// </summary>
            CPL_LAUNCHED = USER + 0x1001,
            /// <summary>
            /// WM_SYSTIMER is a well-known yet still undocumented message. Windows uses WM_SYSTIMER for internal actions like scrolling.
            /// </summary>
            SYSTIMER = 0x118,

            /// <summary>
            /// The accessibility state has changed.
            /// </summary>
            HSHELL_ACCESSIBILITYSTATE = 11,
            /// <summary>
            /// The shell should activate its main window.
            /// </summary>
            HSHELL_ACTIVATESHELLWINDOW = 3,
            /// <summary>
            /// The user completed an input event (for example, pressed an application command button on the mouse or an application command key on the keyboard), and the application did not handle the WM_APPCOMMAND message generated by that input.
            /// If the Shell procedure handles the WM_COMMAND message, it should not call CallNextHookEx. See the Return Value section for more information.
            /// </summary>
            HSHELL_APPCOMMAND = 12,
            /// <summary>
            /// A window is being minimized or maximized. The system needs the coordinates of the minimized rectangle for the window.
            /// </summary>
            HSHELL_GETMINRECT = 5,
            /// <summary>
            /// Keyboard language was changed or a new keyboard layout was loaded.
            /// </summary>
            HSHELL_LANGUAGE = 8,
            /// <summary>
            /// The title of a window in the task bar has been redrawn.
            /// </summary>
            HSHELL_REDRAW = 6,
            /// <summary>
            /// The user has selected the task list. A shell application that provides a task list should return TRUE to prevent Windows from starting its task list.
            /// </summary>
            HSHELL_TASKMAN = 7,
            /// <summary>
            /// A top-level, unowned window has been created. The window exists when the system calls this hook.
            /// </summary>
            HSHELL_WINDOWCREATED = 1,
            /// <summary>
            /// A top-level, unowned window is about to be destroyed. The window still exists when the system calls this hook.
            /// </summary>
            HSHELL_WINDOWDESTROYED = 2,
            /// <summary>
            /// The activation has changed to a different top-level, unowned window.
            /// </summary>
            HSHELL_WINDOWACTIVATED = 4,
            /// <summary>
            /// A top-level window is being replaced. The window exists when the system calls this hook.
            /// </summary>
            HSHELL_WINDOWREPLACED = 13
        }

        public const int WM_COPYDATA = 0x004a;
        public const int WM_DISPLAYCHANGE = 0x007e;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_WINDOWPOSCHANGING = 0x0046;
        public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_NCHITTEST = 0x0084;
        public const int WM_DPICHANGED = 0x02E0;
        public const int WM_ACTIVATE = 0x0006;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_COMMAND = 0x111;
        public const uint WM_LBUTTONDOWN = 0x201;
        public const uint WM_LBUTTONUP = 0x202;
        public const uint WM_LBUTTONDBLCLK = 0x203;
        public const uint WM_RBUTTONDOWN = 0x204;
        public const uint WM_RBUTTONUP = 0x205;
        public const uint WM_RBUTTONDBLCLK = 0x206;
        public const uint WM_CONTEXTMENU = 0x7B;
        public const uint WM_MOUSEMOVE = 0x200;
        public const uint WM_MOUSEHOVER = 0x2A1;
        public const uint WM_MOUSELAST = 0x20D;
        public const uint WM_MOUSELEAVE = 0x2A3;
        public const uint WM_HOTKEY = 0x0312;
        public const int WM_DEVICECHANGE = 0x219;

        public const int MA_NOACTIVATE = 0x0003;

        public const uint NIN_SELECT = 0x400;
        public const uint NIN_POPUPOPEN = 0x406;
        public const uint NIN_POPUPCLOSE = 0x407;

        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public WindowShowStyle showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
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
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, WindowShowStyle nCmdShow);

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

        public delegate bool CallBackPtr(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(CallBackPtr callPtr, int lPar);

        [DllImport("user32.dll")]
        public static extern bool RegisterShellHookWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetTaskmanWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool DeregisterShellHookWindow(IntPtr hWnd);

        public static int WINEVENT_OUTOFCONTEXT = 0;
        public static int WINEVENT_SKIPOWNPROCESS = 2;
        public static int EVENT_OBJECT_LOCATIONCHANGE = 32779;

        public delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWinEventHook(int eventMin, int eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, int dwflags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, IntPtr pvParam, SPIF fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int GW_OWNER = 4;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, IntPtr className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [Flags()]
        public enum SetWindowPosFlags
        {
            SWP_NOSIZE = 0x1,
            SWP_NOMOVE = 0x2,
            SWP_NOZORDER = 0x4,
            SWP_NOREDRAW = 0x8,
            SWP_NOACTIVATE = 0x10,
            SWP_FRAMECHANGED = 0x20,
            SWP_DRAWFRAME = SWP_FRAMECHANGED,
            SWP_SHOWWINDOW = 0x40,
            SWP_HIDEWINDOW = 0x80,
            SWP_NOCOPYBITS = 0x100,
            SWP_NOOWNERZORDER = 0x200,
            SWP_NOREPOSITION = SWP_NOOWNERZORDER,
            SWP_NOSENDCHANGING = 0x400,
            SWP_DEFERERASE = 0x2000,
            SWP_ASYNCWINDOWPOS = 0x4000
        }

        public enum WindowZOrder
        {
            HWND_TOP = 0,
            HWND_BOTTOM = 1,
            HWND_TOPMOST = -1,
            HWND_NOTOPMOST = -2,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hWnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public SetWindowPosFlags flags;

            // Returns the WINDOWPOS structure pointed to by the lParam parameter
            // of a WM_WINDOWPOSCHANGING or WM_WINDOWPOSCHANGED message.
            public static WINDOWPOS FromMessage(IntPtr lParam)
            {
                // Marshal the lParam parameter to an WINDOWPOS structure,
                // and return the new structure
                return (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
            }

            // Replaces the original WINDOWPOS structure pointed to by the lParam
            // parameter of a WM_WINDOWPOSCHANGING or WM_WINDOWPSCHANGING message
            // with this one, so that the native window will be able to see any
            // changes that we have made to its values.
            public void UpdateMessage(IntPtr lParam)
            {
                // Marshal this updated structure back to lParam so the native
                // window can respond to our changes.
                // The old structure that it points to should be deleted, too.
                Marshal.StructureToPtr(this, lParam, true);
            }
        }


        [Flags]
        public enum SPIF
        {
            None = 0x00,
            SPIF_UPDATEINIFILE = 0x01,  // Writes the new system-wide parameter setting to the user profile.
            SPIF_SENDCHANGE = 0x02,  // Broadcasts the WM_SETTINGCHANGE message after updating the user profile.
            SPIF_SENDWININICHANGE = 0x02   // Same as SPIF_SENDCHANGE.
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
        public struct SHELLHOOKINFO
        {
            public IntPtr hwnd;
            public RECT rc;
        }

        /// <summary>
        /// Numerical values of the NIM_* messages represented as an enumeration.
        /// </summary>
        public enum NIM : uint
        {
            /// <summary>
            /// Add a new icon.
            /// </summary>
            NIM_ADD = 0,

            /// <summary>
            /// Modify an existing icon.
            /// </summary>
            NIM_MODIFY = 1,

            /// <summary>
            /// Delete an icon.
            /// </summary>
            NIM_DELETE = 2,

            /// <summary>
            /// Shell v5 and above - Return focus to the notification area.
            /// </summary>
            NIM_SETFOCUS = 3,

            /// <summary>
            /// Shell v4 and above - Instructs the taskbar to behave accordingly based on the version (uVersion) set in the notifiyicondata struct.
            /// </summary>
            NIM_SETVERSION = 4
        }

        /// <summary>
        /// Shell_NotifyIcon flags.  NIF_*
        /// </summary>
        [Flags]
        public enum NIF : uint
        {
            MESSAGE = 0x0001,
            ICON = 0x0002,
            TIP = 0x0004,
            STATE = 0x0008,
            INFO = 0x0010,
            GUID = 0x0020,

            /// <summary>
            /// Vista only.
            /// </summary>
            REALTIME = 0x0040,
            /// <summary>
            /// Vista only.
            /// </summary>
            SHOWTIP = 0x0080,

            XP_MASK = STATE | INFO | GUID,
            VISTA_MASK = REALTIME | SHOWTIP,
        }

        /// <summary>
        /// Notify icon data structure type
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NOTIFYICONDATA
        {
            public int cbSize;
            public uint hWnd;
            public uint uID;
            public NIF uFlags;
            public uint uCallbackMessage;
            public uint hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uVersion;  // used with NIM_SETVERSION, values 0, 3 and 4
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public uint dwInfoFlags;
            public Guid guidItem;
            public uint hBalloonIcon;
        }

        /// <summary>
        /// Notify icon data identifier structure type
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CAIROWINNOTIFYICONIDENTIFIER
        {
            public int dwMagic;
            public int dwMessage;
            public int cbSize;
            public int dwPadding;
            public int hWnd;
            public uint uID;
            public Guid guidItem;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TASKINFORMATION
        {
            public string WindowName;
            public IntPtr WindowHandle;
            public IntPtr NewWindowHandle;
            public IntPtr WindowIcon;
            public WindowActions WindowAction; // Enum
            public IntPtr SystemMenu;
        }

        /// <summary>
        /// The window actions enumerations for the action of a task event.
        /// </summary>
        public enum WindowActions
        {
            /// <summary>
            /// The window has been activated.
            /// </summary>
            Activated,

            /// <summary>
            /// The window has been created.
            /// </summary>
            Created,

            /// <summary>
            /// The window has been destroyed.
            /// </summary>
            Destroyed,

            /// <summary>
            /// The window has been replaced.
            /// </summary>
            Replaced
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out uint pvAttribute, int cbAttribute);

        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

        // Taskbar
        public const int WH_SHELL = 10;

        public const int HSHELL_WINDOWCREATED = 1;
        public const int HSHELL_WINDOWDESTROYED = 2;
        public const int HSHELL_ACTIVATESHELLWINDOW = 3;

        //Windows NT
        public const int HSHELL_WINDOWACTIVATED = 4;
        public const int HSHELL_GETMINRECT = 5;
        public const int HSHELL_REDRAW = 6;
        public const int HSHELL_TASKMAN = 7;
        public const int HSHELL_LANGUAGE = 8;
        public const int HSHELL_SYSMENU = 9;
        public const int HSHELL_ENDTASK = 10;

        //Windows 2000
        public const int HSHELL_ACCESSIBILITYSTATE = 11;
        public const int HSHELL_APPCOMMAND = 12;

        //Windows XP
        public const int HSHELL_WINDOWREPLACED = 13;
        public const int HSHELL_WINDOWREPLACING = 14;

        public const int HSHELL_HIGHBIT = 0x8000;
        public const int HSHELL_FLASH = (HSHELL_REDRAW | HSHELL_HIGHBIT);
        public const int HSHELL_RUDEAPPACTIVATED = (HSHELL_WINDOWACTIVATED | HSHELL_HIGHBIT);
        
        public const int WM_CLOSE = 0xF060;
        public const int WM_SYSCOMMAND = 0x0112;

        public const int SC_MINIMIZE = 0xF020;
        public const int SC_RESTORE = 0xF120;

        /*/// <summary>
        /// Unregisters a window class, freeing the memory required for the class.
        /// </summary>
        /// <param name="lpClassName">
        /// Type: LPCTSTR
        /// A null-terminated string or a class atom. If lpClassName is a string, it specifies the window class name. 
        /// This class name must have been registered by a previous call to the RegisterClass or RegisterClassEx function. 
        /// System classes, such as dialog box controls, cannot be unregistered. If this parameter is an atom, 
        ///   it must be a class atom created by a previous call to the RegisterClass or RegisterClassEx function. 
        /// The atom must be in the low-order word of lpClassName; the high-order word must be zero.
        /// 
        /// </param>
        /// <param name="hInstance">
        /// A handle to the instance of the module that created the class.
        /// 
        /// </param>
        /// <returns>
        /// Type: BOOL
        /// If the function succeeds, the return value is nonzero.
        /// If the class could not be found or if a window still exists that was created with the class, the return value is zero. 
        /// To get extended error information, call GetLastError.
        /// 
        /// </returns>
        [DllImport("user32.dll")]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U2)]
        public static extern UInt16 RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        /// <summary>
        /// The CreateWindowEx function creates an overlapped, pop-up, or child window with an extended window style; otherwise, this function is identical to the CreateWindow function. 
        /// </summary>
        /// <param name="dwExStyle">Specifies the extended window style of the window being created.</param>
        /// <param name="lpClassName">Pointer to a null-terminated string or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the high-order word must be zero. If lpClassName is a string, it specifies the window class name. The class name can be any name registered with RegisterClass or RegisterClassEx, provided that the module that registers the class is also the module that creates the window. The class name can also be any of the predefined system class names.</param>
        /// <param name="lpWindowName">Pointer to a null-terminated string that specifies the window name. If the window style specifies a title bar, the window title pointed to by lpWindowName is displayed in the title bar. When using CreateWindow to create controls, such as buttons, check boxes, and static controls, use lpWindowName to specify the text of the control. When creating a static control with the SS_ICON style, use lpWindowName to specify the icon name or identifier. To specify an identifier, use the syntax "#num". </param>
        /// <param name="dwStyle">Specifies the style of the window being created. This parameter can be a combination of window styles, plus the control styles indicated in the Remarks section.</param>
        /// <param name="x">Specifies the initial horizontal position of the window. For an overlapped or pop-up window, the x parameter is the initial x-coordinate of the window's upper-left corner, in screen coordinates. For a child window, x is the x-coordinate of the upper-left corner of the window relative to the upper-left corner of the parent window's client area. If x is set to CW_USEDEFAULT, the system selects the default position for the window's upper-left corner and ignores the y parameter. CW_USEDEFAULT is valid only for overlapped windows; if it is specified for a pop-up or child window, the x and y parameters are set to zero.</param>
        /// <param name="y">Specifies the initial vertical position of the window. For an overlapped or pop-up window, the y parameter is the initial y-coordinate of the window's upper-left corner, in screen coordinates. For a child window, y is the initial y-coordinate of the upper-left corner of the child window relative to the upper-left corner of the parent window's client area. For a list box y is the initial y-coordinate of the upper-left corner of the list box's client area relative to the upper-left corner of the parent window's client area.
        /// <para>If an overlapped window is created with the WS_VISIBLE style bit set and the x parameter is set to CW_USEDEFAULT, then the y parameter determines how the window is shown. If the y parameter is CW_USEDEFAULT, then the window manager calls ShowWindow with the SW_SHOW flag after the window has been created. If the y parameter is some other value, then the window manager calls ShowWindow with that value as the nCmdShow parameter.</para></param>
        /// <param name="nWidth">Specifies the width, in device units, of the window. For overlapped windows, nWidth is the window's width, in screen coordinates, or CW_USEDEFAULT. If nWidth is CW_USEDEFAULT, the system selects a default width and height for the window; the default width extends from the initial x-coordinates to the right edge of the screen; the default height extends from the initial y-coordinate to the top of the icon area. CW_USEDEFAULT is valid only for overlapped windows; if CW_USEDEFAULT is specified for a pop-up or child window, the nWidth and nHeight parameter are set to zero.</param>
        /// <param name="nHeight">Specifies the height, in device units, of the window. For overlapped windows, nHeight is the window's height, in screen coordinates. If the nWidth parameter is set to CW_USEDEFAULT, the system ignores nHeight.</param> <param name="hWndParent">Handle to the parent or owner window of the window being created. To create a child window or an owned window, supply a valid window handle. This parameter is optional for pop-up windows.
        /// <para>Windows 2000/XP: To create a message-only window, supply HWND_MESSAGE or a handle to an existing message-only window.</para></param>
        /// <param name="hMenu">Handle to a menu, or specifies a child-window identifier, depending on the window style. For an overlapped or pop-up window, hMenu identifies the menu to be used with the window; it can be NULL if the class menu is to be used. For a child window, hMenu specifies the child-window identifier, an integer value used by a dialog box control to notify its parent about events. The application determines the child-window identifier; it must be unique for all child windows with the same parent window.</param>
        /// <param name="hInstance">Handle to the instance of the module to be associated with the window.</param> <param name="lpParam">Pointer to a value to be passed to the window through the CREATESTRUCT structure (lpCreateParams member) pointed to by the lParam param of the WM_CREATE message. This message is sent to the created window by this function before it returns.
        /// <para>If an application calls CreateWindow to create a MDI client window, lpParam should point to a CLIENTCREATESTRUCT structure. If an MDI client window calls CreateWindow to create an MDI child window, lpParam should point to a MDICREATESTRUCT structure. lpParam may be NULL if no additional data is needed.</para></param>
        /// <returns>If the function succeeds, the return value is a handle to the new window.
        /// <para>If the function fails, the return value is NULL. To get extended error information, call GetLastError.</para>
        /// <para>This function typically fails for one of the following reasons:</para>
        /// <list type="">
        /// <item>an invalid parameter value</item>
        /// <item>the system class was registered by a different module</item>
        /// <item>The WH_CBT hook is installed and returns a failure code</item>
        /// <item>if one of the controls in the dialog template is not registered, or its window window procedure fails WM_CREATE or WM_NCCREATE</item>
        /// </list></returns>

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowEx(
           WindowStylesEx dwExStyle,
           UInt16 lpClassName,
           [MarshalAs(UnmanagedType.LPStr)] string lpWindowName,
           WindowStyles dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        [Flags]
        public enum WindowStylesEx : uint
        {
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_APPWINDOW = 0x00040000,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_CONTEXTHELP = 0x00000400,
            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_LAYOUTRTL = 0x00400000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_MDICHILD = 0x00000040,
            WS_EX_NOACTIVATE = 0x08000000,
            WS_EX_NOINHERITLAYOUT = 0x00100000,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            WS_EX_RIGHT = 0x00001000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_WINDOWEDGE = 0x00000100
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;    // Any value the sender chooses.  Perhaps its main window handle?
            public int cbData;       // The count of bytes in the message.
            public IntPtr lpData;    // The address of the message.
        }

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);*/

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PROPERTYKEY
        {
            public Guid fmtid;
            public uint pid;
        }

        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyStore
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetCount([Out] out uint cProps);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetAt([In] uint iProp, out PROPERTYKEY pkey);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetValue([In] ref PROPERTYKEY key, out PropVariant pv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetValue([In] ref PROPERTYKEY key, [In] ref PropVariant pv);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Commit();
        }

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int SHGetPropertyStoreForWindow(IntPtr handle, ref Guid riid, out IPropertyStore propertyStore);

        /// <summary>
        /// Represents the OLE struct PROPVARIANT.
        /// </summary>
        /// <remarks>
        /// Must call Clear when finished to avoid memory leaks. If you get the value of
        /// a VT_UNKNOWN prop, an implicit AddRef is called, thus your reference will
        /// be active even after the PropVariant struct is cleared.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct PropVariant
        {
            #region struct fields

            // The layout of these elements needs to be maintained.
            //
            // NOTE: We could use LayoutKind.Explicit, but we want
            //       to maintain that the IntPtr may be 8 bytes on
            //       64-bit architectures, so we’ll let the CLR keep
            //       us aligned.
            //
            // NOTE: In order to allow x64 compat, we need to allow for
            //       expansion of the IntPtr. However, the BLOB struct
            //       uses a 4-byte int, followed by an IntPtr, so
            //       although the p field catches most pointer values,
            //       we need an additional 4-bytes to get the BLOB
            //       pointer. The p2 field provides this, as well as
            //       the last 4-bytes of an 8-byte value on 32-bit
            //       architectures.

            // This is actually a VarEnum value, but the VarEnum type
            // shifts the layout of the struct by 4 bytes instead of the
            // expected 2.
            ushort vt;
            ushort wReserved1;
            ushort wReserved2;
            ushort wReserved3;
            IntPtr p;
            int p2;

            #endregion // struct fields

            #region union members

            sbyte cVal // CHAR cVal;
            {
                get { return (sbyte)GetDataBytes()[0]; }
            }

            byte bVal // UCHAR bVal;
            {
                get { return GetDataBytes()[0]; }
            }

            short iVal // SHORT iVal;
            {
                get { return BitConverter.ToInt16(GetDataBytes(), 0); }
            }

            ushort uiVal // USHORT uiVal;
            {
                get { return BitConverter.ToUInt16(GetDataBytes(), 0); }
            }

            int lVal // LONG lVal;
            {
                get { return BitConverter.ToInt32(GetDataBytes(), 0); }
            }

            uint ulVal // ULONG ulVal;
            {
                get { return BitConverter.ToUInt32(GetDataBytes(), 0); }
            }

            long hVal // LARGE_INTEGER hVal;
            {
                get { return BitConverter.ToInt64(GetDataBytes(), 0); }
            }

            ulong uhVal // ULARGE_INTEGER uhVal;
            {
                get { return BitConverter.ToUInt64(GetDataBytes(), 0); }
            }

            float fltVal // FLOAT fltVal;
            {
                get { return BitConverter.ToSingle(GetDataBytes(), 0); }
            }

            double dblVal // DOUBLE dblVal;
            {
                get { return BitConverter.ToDouble(GetDataBytes(), 0); }
            }

            bool boolVal // VARIANT_BOOL boolVal;
            {
                get { return (iVal == 0 ? false : true); }
            }

            int scode // SCODE scode;
            {
                get { return lVal; }
            }

            decimal cyVal // CY cyVal;
            {
                get { return decimal.FromOACurrency(hVal); }
            }

            DateTime date // DATE date;
            {
                get { return DateTime.FromOADate(dblVal); }
            }

            #endregion // union members

            /// <summary>
            /// Gets a byte array containing the data bits of the struct.
            /// </summary>
            /// <returns>A byte array that is the combined size of the data bits.</returns>
            private byte[] GetDataBytes()
            {
                byte[] ret = new byte[IntPtr.Size + sizeof(int)];
                if (IntPtr.Size == 4)
                    BitConverter.GetBytes(p.ToInt32()).CopyTo(ret, 0);
                else if (IntPtr.Size == 8)
                    BitConverter.GetBytes(p.ToInt64()).CopyTo(ret, 0);
                BitConverter.GetBytes(p2).CopyTo(ret, IntPtr.Size);
                return ret;
            }

            /// <summary>
            /// Called to properly clean up the memory referenced by a PropVariant instance.
            /// </summary>
            [DllImport("ole32.dll")]
            private extern static int PropVariantClear(ref PropVariant pvar);

            /// <summary>
            /// Called to clear the PropVariant’s referenced and local memory.
            /// </summary>
            /// <remarks>
            /// You must call Clear to avoid memory leaks.
            /// </remarks>
            public void Clear()
            {
                // Can’t pass “this” by ref, so make a copy to call PropVariantClear with
                PropVariant var = this;
                PropVariantClear(ref var);

                // Since we couldn’t pass “this” by ref, we need to clear the member fields manually
                // NOTE: PropVariantClear already freed heap data for us, so we are just setting
                //       our references to null.
                vt = (ushort)VarEnum.VT_EMPTY;
                wReserved1 = wReserved2 = wReserved3 = 0;
                p = IntPtr.Zero;
                p2 = 0;
            }

            /// <summary>
            /// Gets the variant type.
            /// </summary>
            public VarEnum Type
            {
                get { return (VarEnum)vt; }
            }

            /// <summary>
            /// Gets the variant value.
            /// </summary>
            public object Value
            {
                get
                {
                    // TODO: Add support for reference types (ie. VT_REF | VT_I1)
                    // TODO: Add support for safe arrays

                    switch ((VarEnum)vt)
                    {
                        case VarEnum.VT_I1:
                            return cVal;
                        case VarEnum.VT_UI1:
                            return bVal;
                        case VarEnum.VT_I2:
                            return iVal;
                        case VarEnum.VT_UI2:
                            return uiVal;
                        case VarEnum.VT_I4:
                        case VarEnum.VT_INT:
                            return lVal;
                        case VarEnum.VT_UI4:
                        case VarEnum.VT_UINT:
                            return ulVal;
                        case VarEnum.VT_I8:
                            return hVal;
                        case VarEnum.VT_UI8:
                            return uhVal;
                        case VarEnum.VT_R4:
                            return fltVal;
                        case VarEnum.VT_R8:
                            return dblVal;
                        case VarEnum.VT_BOOL:
                            return boolVal;
                        case VarEnum.VT_ERROR:
                            return scode;
                        case VarEnum.VT_CY:
                            return cyVal;
                        case VarEnum.VT_DATE:
                            return date;
                        case VarEnum.VT_FILETIME:
                            return DateTime.FromFileTime(hVal);
                        case VarEnum.VT_BSTR:
                            return Marshal.PtrToStringBSTR(p);
                        case VarEnum.VT_BLOB:
                            byte[] blobData = new byte[lVal];
                            IntPtr pBlobData;
                            if (IntPtr.Size == 4)
                            {
                                pBlobData = new IntPtr(p2);
                            }
                            else if (IntPtr.Size == 8)
                            {
                                // In this case, we need to derive a pointer at offset 12,
                                // because the size of the blob is represented as a 4-byte int
                                // but the pointer is immediately after that.
                                pBlobData = new IntPtr(BitConverter.ToInt64(GetDataBytes(), sizeof(int)));
                            }
                            else
                                throw new NotSupportedException();
                            Marshal.Copy(pBlobData, blobData, 0, lVal);
                            return blobData;
                        case VarEnum.VT_LPSTR:
                            return Marshal.PtrToStringAnsi(p);
                        case VarEnum.VT_LPWSTR:
                            return Marshal.PtrToStringUni(p);
                        case VarEnum.VT_UNKNOWN:
                            return Marshal.GetObjectForIUnknown(p);
                        case VarEnum.VT_DISPATCH:
                            return p;
                        default:
                            return "";
                    }
                }
            }
        }

        [Flags()]
        public enum RunFileDialogFlags : uint
        {

            /// <summary>
            /// Don't use any of the flags (only works alone)
            /// </summary>
            None = 0x0000,

            /// <summary>
            /// Removes the browse button
            /// </summary>
            NoBrowse = 0x0001,

            /// <summary>
            /// No default item selected
            /// </summary>
            NoDefault = 0x0002,

            /// <summary>
            /// Calculates the working directory from the file name
            /// </summary>
            CalcDirectory = 0x0004,

            /// <summary>
            /// Removes the edit box label
            /// </summary>
            NoLabel = 0x0008,

            /// <summary>
            /// Removes the separate memory space checkbox (Windows NT only)
            /// </summary>
            NoSeperateMemory = 0x0020
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "#61", SetLastError = true)]
        public static extern bool SHRunFileDialog(IntPtr hwndOwner,
                                   IntPtr hIcon,
                                   string lpszPath,
                                   string lpszDialogTitle,
                                   string lpszDialogTextBody,
                                   RunFileDialogFlags uflags);
        
        // Begin Windows 10 blur
        // https://github.com/riverar/sample-win32-acrylicblur
        // License: MIT

        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        public enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        public enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        //
        // End Windows 10 blur

        [DllImport("User32.dll")]
        public static extern uint SendInput(uint numberOfInputs, INPUT[] input, int structSize);

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int INPUT_KEYBOARD = 1;
        public const uint KEYEVENTF_KEYUP = 0x0002;

        public const int VK_LWIN = 0x5B;
        public const int VK_TAB = 0x09;

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            int dx;
            int dy;
            uint mouseData;
            uint dwFlags;
            uint time;
            IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            uint uMsg;
            ushort wParamL;
            ushort wParamH;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct MouseKeybdHardwareInputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public MouseKeybdHardwareInputUnion mkhi;
        }

        public struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;        // x offest from the upperleft of bitmap
            public int yBitmap;        // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGEINFO
        {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public RECT rcImage;
        }

        public struct POINT
        {
            public long x;
            public long y;
        }
        
        [ComImportAttribute()]
        [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IImageList
        {
            [PreserveSig]
            int Add(
                IntPtr hbmImage,
                IntPtr hbmMask,
                ref int pi);

            [PreserveSig]
            int ReplaceIcon(
                int i,
                IntPtr hicon,
                ref int pi);

            [PreserveSig]
            int SetOverlayImage(
                int iImage,
                int iOverlay);

            [PreserveSig]
            int Replace(
                int i,
                IntPtr hbmImage,
                IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(
                IntPtr hbmImage,
                int crMask,
                ref int pi);

            [PreserveSig]
            int Draw(
                ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(
            int i);

            [PreserveSig]
            int GetIcon(
                int i,
                int flags,
                ref IntPtr picon);

            [PreserveSig]
            int GetImageInfo(
                int i,
                ref IMAGEINFO pImageInfo);

            [PreserveSig]
            int Copy(
                int iDst,
                IImageList punkSrc,
                int iSrc,
                int uFlags);

            [PreserveSig]
            int Merge(
                int i1,
                IImageList punk2,
                int i2,
                int dx,
                int dy,
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int Clone(
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int GetImageRect(
                int i,
                ref RECT prc);

            [PreserveSig]
            int GetIconSize(
                ref int cx,
                ref int cy);

            [PreserveSig]
            int SetIconSize(
                int cx,
                int cy);

            [PreserveSig]
            int GetImageCount(
            ref int pi);

            [PreserveSig]
            int SetImageCount(
                int uNewCount);

            [PreserveSig]
            int SetBkColor(
                int clrBk,
                ref int pclr);

            [PreserveSig]
            int GetBkColor(
                ref int pclr);

            [PreserveSig]
            int BeginDrag(
                int iTrack,
                int dxHotspot,
                int dyHotspot);

            [PreserveSig]
            int EndDrag();

            [PreserveSig]
            int DragEnter(
                IntPtr hwndLock,
                int x,
                int y);

            [PreserveSig]
            int DragLeave(
                IntPtr hwndLock);

            [PreserveSig]
            int DragMove(
                int x,
                int y);

            [PreserveSig]
            int SetDragCursorImage(
                ref IImageList punk,
                int iDrag,
                int dxHotspot,
                int dyHotspot);

            [PreserveSig]
            int DragShowNolock(
                int fShow);

            [PreserveSig]
            int GetDragImage(
                ref POINT ppt,
                ref POINT pptHotspot,
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int GetItemFlags(
                int i,
                ref int dwFlags);

            [PreserveSig]
            int GetOverlayImage(
                int iOverlay,
                ref int piIndex);
        };
        
        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(
            int iImageList,
            ref Guid riid,
            out IImageList ppv
            );

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        public extern static short GetAsyncKeyState(int key);

        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll")]
       public static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <summary>
        /// defines the callback type for the hook
        /// </summary>
        public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);

        public struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
       public static extern IntPtr LoadLibrary(string lpFileName);

    }
}