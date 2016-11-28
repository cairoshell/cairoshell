using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace CairoDesktop.Interop
{
    public partial class Shell
    {
        /* ******************************************
         * DLL Imports for getting special folders 
         * that are not supported by .Net Framework
         * *************************************** */

        // Uses some code from https://gist.github.com/madd0/1433330

        private const int MAX_PATH = 260;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

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
            
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [Flags]
        private enum SHGFI : int
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
        
        private class Win32
        {
            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);

            [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
            public static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

            [DllImport("comctl32.dll", SetLastError = true)]
            public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

        }

        public static Icon GetIconByFilename(string fileName, bool isSmall = false)
        {
            if (isSmall)
                return GetIcon(fileName, SHGFI.SmallIcon);
            else
                return GetIcon(fileName, SHGFI.LargeIcon);
        }

        public static Icon GetIconByExtension(string extension, bool isSmall = false)
        {
            if (isSmall)
                return GetIcon(extension, SHGFI.SmallIcon | SHGFI.UseFileAttributes);
            else
                return GetIcon(extension, SHGFI.LargeIcon | SHGFI.UseFileAttributes);
        }

        private static Icon GetIcon(string filename, SHGFI flags, bool isFolder = false)
        {
            SHFILEINFO shinfo = new SHFILEINFO();

            IntPtr hIconInfo = Win32.SHGetFileInfo(filename, isFolder ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(SHGFI.SysIconIndex | flags));

            Icon icon = (Icon)System.Drawing.Icon.FromHandle(Win32.ImageList_GetIcon(hIconInfo, shinfo.iIcon.ToInt32(), (int)0x00000001)).Clone();

            Win32.DestroyIcon(shinfo.hIcon);

            return icon;
        }

        public static string UsersProgramsPath 
        {
            get {
                return GetSpecialFolderPath((int)CSIDL.CSIDL_PROGRAMS);
            }
        }
        public static string UsersStartMenuPath
        {
            get {
                return GetSpecialFolderPath((int)CSIDL.CSIDL_STARTMENU);
            }
        }
        public static string UsersDesktopPath
        {
            get {
                return GetSpecialFolderPath((int)CSIDL.CSIDL_DESKTOPDIRECTORY);
            }
        }
        public static string AllUsersProgramsPath
        {
            get {
                return GetSpecialFolderPath((int)CSIDL.CSIDL_COMMON_PROGRAMS);
            }
        }
        public static string AllUsersStartMenuPath
        {
            get {
                return GetSpecialFolderPath((int)CSIDL.CSIDL_COMMON_STARTMENU);
            }
        }
        public static string AllUsersDesktopPath
        {
            get {
                return GetSpecialFolderPath((int)CSIDL.CSIDL_COMMON_DESKTOPDIRECTORY);
            }
        }
        public static string GetSpecialFolderPath(int FOLDER)
        {
            // Need to test these locations in Vista
            // CSIDL_PROGRAMS
            // CSIDL_STARTMENU
            // CSIDL_DESKTOPDIRECTORY
            // CSIDL_COMMON_STARTMENU
            // CSIDL_COMMON_PROGRAMS
            // CSIDL_COMMON_DESKTOPDIRECTORY

            StringBuilder sbPath = new StringBuilder(MAX_PATH);
            Win32.SHGetFolderPath(IntPtr.Zero, FOLDER, IntPtr.Zero, 0, sbPath);
            return sbPath.ToString();
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int ExtractIconEx(string path, int i, IntPtr[] big, IntPtr[] small, int op);
        
        public static IntPtr GetHIcon(string fileName, int iconIndex, bool isSmall = false) {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            if (!System.IO.File.Exists(fileName)) {
                System.Diagnostics.Debug.WriteLine("File doesn't exist: " + fileName + " - Recovering");
                return IntPtr.Zero;
            }
            if (iconIndex < 0) iconIndex = 0;
            int numIcons = ExtractIconEx(fileName, -1, null, null, 0);
            if (numIcons == 0) {
                try {
                    System.Drawing.Icon i = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
                    if (i == null) {
                        return IntPtr.Zero;
                    }
                    return i.Handle;
                } catch {
                    return GetHIcon(System.Environment.SystemDirectory + @"\shell32.dll",0);
                }
            }
            IntPtr[] largeIcons = new IntPtr[numIcons];
            IntPtr[] smallIcons = new IntPtr[numIcons];
            ExtractIconEx(fileName, 0, largeIcons, smallIcons, numIcons);

            if (isSmall)
                return smallIcons[iconIndex];
            else
                return largeIcons[iconIndex];
        }

        public static void StartProcess(string filename)
        {
            // TODO enhance this for lnk files
            if (!Environment.Is64BitProcess)
            {
                filename.Replace("system32", "sysnative");
            }

            System.Diagnostics.Process.Start(filename);
        }

        public static void StartProcess(string filename, string args)
        {
            // TODO enhance this for lnk files
            if (!Environment.Is64BitProcess)
            {
                filename.Replace("system32", "sysnative");
            }

            System.Diagnostics.Process.Start(filename, args);
        }

        public static bool Exists(string filename)
        {
            if (filename.StartsWith("\\\\"))
                return false;
            else
                return (System.IO.File.Exists(filename) || System.IO.Directory.Exists(filename));
        }
    }
}
