using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CairoDesktop.Interop
{

    /// <summary>
    /// Container class for Win32 Native methods used within the desktop application (e.g. shutdown, sleep, et al).
    /// </summary>
    public partial class NativeMethods
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

        // Handling the close splash screen event
        [DllImport("kernel32.dll")]
        public static extern Int32 OpenEvent(Int32 DesiredAccess, bool InheritHandle, string Name);

        // OpenEvent DesiredAccess defines
        public const int EVENT_MODIFY_STATE = 0x00000002;

        [DllImport("kernel32.dll")]
        public static extern Int32 SetEvent(Int32 Handle);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(Int32 Handle);

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

        [DllImport("Advapi32.dll")]
        public static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool watchSubtree, REG_NOTIFY_CHANGE notifyFilter, IntPtr hEvent, bool asynchronous);

        [Flags]
        public enum REG_NOTIFY_CHANGE : uint
        {
            /// <summary>
            /// Notify the caller if a subkey is added or deleted
            /// </summary>
            NAME = 0x1,
            /// <summary>
            /// Notify the caller of changes to the attributes of the key,
            /// such as the security descriptor information
            /// </summary>
            ATTRIBUTES = 0x2,
            /// <summary>
            /// Notify the caller of changes to a value of the key. This can
            /// include adding or deleting a value, or changing an existing value
            /// </summary>
            LAST_SET = 0x4,
            /// <summary>
            /// Notify the caller of changes to the security descriptor of the key
            /// </summary>
            SECURITY = 0x8
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;

            public int Height => Bottom - Top;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public Rect rc;
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
        public enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

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

        [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
        public static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

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

        public const int MA_NOACTIVATE = 0x0003;

        public const uint NIN_SELECT = 0x400;
        public const uint NIN_POPUPOPEN = 0x406;
        public const uint NIN_POPUPCLOSE = 0x407;

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentProcessId();

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
            public Rect rc;
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
        /// AppBar message data structure type
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MenuBarSizeData
        {
            public Rect rc;
            public int edge;
        }

        // Taskbar
        public const int WH_SHELL = 10;
        public const int HSHELL_HIGHBIT = 0x8000;

        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MOVE = 0xF010;
        public const int SC_RESTORE = 0xF120;
        public const int SC_SIZE = 0xF000;
        public const int SC_CLOSE = 0xF060;

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

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;    // Any value the sender chooses.  Perhaps its main window handle?
            public int cbData;       // The count of bytes in the message.
            public IntPtr lpData;    // The address of the message.
        }

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);*/

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
            public Rect rcImage;
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
                ref Rect prc);

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

        /*
         * These interfaces can be used to display a shell desktop with the correct zordering, kept for future reference
        [ComImport()]
        [Guid("213E2DF9-9A14-4328-99B1-6961F9143CE9")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDeskTray
        {
            [PreserveSig]
            uint AppBarGetState();

            [PreserveSig]
            IntPtr GetTrayWindow(ref IntPtr hwnd);

            [PreserveSig]
            IntPtr SetDesktopWindow(IntPtr hwnd);

            [PreserveSig]
            IntPtr SetVar(int i, ulong l);
        }

        public class ShellDesktopTray : IDeskTray {
            public uint AppBarGetState()
            {
                return 0;
            }

            public IntPtr GetTrayWindow(ref IntPtr hwnd)
            {
                return IntPtr.Zero;
            }

            public IntPtr SetDesktopWindow(IntPtr hwnd)
            {
                return IntPtr.Zero;
            }

            public IntPtr SetVar(int i, ulong l)
            {
                return IntPtr.Zero;
            }
        }

        [DllImport("shell32.dll", EntryPoint = "#200")]
        public extern static IntPtr SHCreateDesktop(IDeskTray iTray);

        [DllImport("shell32.dll", EntryPoint = "#201")]
        public extern static IntPtr SHDesktopMessageLoop(IntPtr hDesktop);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hwnd);*/

        [ComImport, Guid("b722bccb-4e68-101b-a2bc-00aa00404770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleCommandTarget
        {
            [PreserveSig()]
            int QueryStatus(ref Guid pguidCmdGroup, int cCmds, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IntPtr[] prgCmds, ref IntPtr pCmdText);

            [PreserveSig()]
            int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdExecOpt, ref object pvaIn, ref object pvaOut);
        }

        public const int OLECMDID_NEW = 2;
        public const int OLECMDID_SAVE = 3;
        public const int OLECMDEXECOPT_DODEFAULT = 0;
        public const string CGID_SHELLSERVICEOBJECT = "000214D2-0000-0000-C000-000000000046";

    }
}