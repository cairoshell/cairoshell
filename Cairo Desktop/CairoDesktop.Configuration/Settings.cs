using System.Collections.Generic;
using System.Globalization;

namespace CairoDesktop.Configuration
{
    public class Settings
    {
        private static Settings instance;

        public Settings() { }

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                }

                return instance;
            }
        }

        #region Private properties for caching

        // General
        private static bool? _IsFirstRun;
        private static string _TimeFormat;
        private static string _DateFormat;
        private static string _FileManager;
        private static string _CairoTheme;
        private static string _Language;
        private static bool? _EnableSubDirs;
        private static bool? _ForceSoftwareRendering;

        // Desktop
        private static bool? _EnableDesktop;
        private static bool? _EnableDynamicDesktop;
        private static string _DesktopDirectory;
        private static int? _DesktopLabelPosition;
        private static int? _DesktopIconSize;
        private static bool? _EnableDesktopOverlay;
        private static string _DesktopOverlayHotKey;

        // Taskbar
        private static bool? _EnableTaskbar;
        private static int? _TaskbarMode;
        private static bool? _EnableTaskbarPolling;
        private static int? _TaskbarPosition;
        private static int? _TaskbarIconSize;

        // Menu Bar
        private static string _DefaultProgramsCategory;
        private static bool? _EnableMenuBarShadow;
        private static bool? _EnableSysTray;
        private static bool? _SysTrayAlwaysExpanded;
        private static bool? _EnableSysTrayRehook;
        private static bool? _EnableCairoMenuHotKey;
        private static string _CairoMenuHotKey;

        #endregion

        #region Public properties

        #region General
        public static bool IsFirstRun
        {
            get
            {
                if (_IsFirstRun == null)
                    _IsFirstRun = Properties.Settings.Default.IsFirstRun;

                return (bool)_IsFirstRun;
            }
            set
            {
                _IsFirstRun = value;
                Properties.Settings.Default.IsFirstRun = (bool)_IsFirstRun;
                Save();
            }
        }

        public static string TimeFormat
        {
            get
            {
                if (_TimeFormat == null)
                    _TimeFormat = Properties.Settings.Default.TimeFormat;

                // if still null, means we are using the default or user removed from configuration
                if (string.IsNullOrEmpty(_TimeFormat))
                {
                    DateTimeFormatInfo dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
                    _TimeFormat = "ddd " + dtfi.ShortTimePattern;
                }

                return _TimeFormat;
            }
            set
            {
                _TimeFormat = value;
                Properties.Settings.Default.TimeFormat = _TimeFormat;
                Save();
            }
        }

        public static string DateFormat
        {
            get
            {
                if (_DateFormat == null)
                    _DateFormat = Properties.Settings.Default.DateFormat;

                // if still null, means user removed from configuration
                if (string.IsNullOrEmpty(_DateFormat))
                {
                    _DateFormat = "D";
                }

                return _DateFormat;
            }
            set
            {
                _DateFormat = value;
                Properties.Settings.Default.DateFormat = _DateFormat;
                Save();
            }
        }
        
        public static string FileManager
        {
            get
            {
                if (_FileManager == null)
                    _FileManager = Properties.Settings.Default.FileManager;

                return _FileManager;
            }
            set
            {
                _FileManager = value;
                Properties.Settings.Default.FileManager = _FileManager;
                Save();
            }
        }

        public static string CairoTheme
        {
            get
            {
                if (_CairoTheme == null)
                    _CairoTheme = Properties.Settings.Default.CairoTheme;

                return _CairoTheme;
            }
            set
            {
                _CairoTheme = value;
                Properties.Settings.Default.CairoTheme = _CairoTheme;
                Save();
            }
        }

        public static string Language
        {
            get
            {
                if (_Language == null)
                    _Language = Properties.Settings.Default.Language;

                // if still null, means we are using the default or user removed from configuration
                if (string.IsNullOrEmpty(_Language))
                {
                    _Language = CultureInfo.CurrentUICulture.Name.Replace('-', '_');
                }

                return _Language;
            }
            set
            {
                _Language = value;
                Properties.Settings.Default.Language = _Language;
                Save();
            }
        }

        public static bool EnableSubDirs
        {
            get
            {
                if (_EnableSubDirs == null)
                    _EnableSubDirs = Properties.Settings.Default.EnableSubDirs;

                return (bool)_EnableSubDirs;
            }
            set
            {
                _EnableSubDirs = value;
                Properties.Settings.Default.EnableSubDirs = (bool)_EnableSubDirs;
                Save();
            }
        }

        public static bool ForceSoftwareRendering
        {
            get
            {
                if (_ForceSoftwareRendering == null)
                    _ForceSoftwareRendering = Properties.Settings.Default.ForceSoftwareRendering;

                return (bool)_ForceSoftwareRendering;
            }
            set
            {
                _ForceSoftwareRendering = value;
                Properties.Settings.Default.ForceSoftwareRendering = (bool)_ForceSoftwareRendering;
                Save();
            }
        }
        #endregion

        #region Desktop
        public static bool EnableDesktop
        {
            get
            {
                if (_EnableDesktop == null)
                    _EnableDesktop = Properties.Settings.Default.EnableDesktop;

                return (bool)_EnableDesktop;
            }
            set
            {
                _EnableDesktop = value;
                Properties.Settings.Default.EnableDesktop = (bool)_EnableDesktop;
                Save();
            }
        }

        public static bool EnableDynamicDesktop
        {
            get
            {
                if (_EnableDynamicDesktop == null)
                    _EnableDynamicDesktop = Properties.Settings.Default.EnableDynamicDesktop;

                return (bool)_EnableDynamicDesktop;
            }
            set
            {
                _EnableDynamicDesktop = value;
                Properties.Settings.Default.EnableDynamicDesktop = (bool)_EnableDynamicDesktop;
                Save();
            }
        }

        public static string DesktopDirectory
        {
            get
            {
                if (_DesktopDirectory == null)
                    _DesktopDirectory = Properties.Settings.Default.DesktopDirectory;

                return _DesktopDirectory;
            }
            set
            {
                _DesktopDirectory = value;
                Properties.Settings.Default.DesktopDirectory = _DesktopDirectory;
                Save();
            }
        }

        public static int DesktopLabelPosition
        {
            get
            {
                if (_DesktopLabelPosition == null)
                    _DesktopLabelPosition = Properties.Settings.Default.DesktopLabelPosition;

                return (int)_DesktopLabelPosition;
            }
            set
            {
                _DesktopLabelPosition = value;
                Properties.Settings.Default.DesktopLabelPosition = (int)_DesktopLabelPosition;
                Save();
            }
        }

        public static int DesktopIconSize
        {
            get
            {
                if (_DesktopIconSize == null)
                    _DesktopIconSize = Properties.Settings.Default.DesktopIconSize;

                return (int)_DesktopIconSize;
            }
            set
            {
                _DesktopIconSize = value;
                Properties.Settings.Default.DesktopIconSize = (int)_DesktopIconSize;
                Save();
            }
        }

        public static bool EnableDesktopOverlay
        {
            get
            {
                if (_EnableDesktopOverlay == null)
                    _EnableDesktopOverlay = Properties.Settings.Default.EnableDesktopOverlay;

                return (bool)_EnableDesktopOverlay;
            }
            set
            {
                _EnableDesktopOverlay = value;
                Properties.Settings.Default.EnableDesktopOverlay = (bool)_EnableDesktopOverlay;
                Save();
            }
        }

        public static List<string> DesktopOverlayHotKey
        {
            get
            {
                if (string.IsNullOrEmpty(_DesktopOverlayHotKey))
                    _DesktopOverlayHotKey = Properties.Settings.Default.DesktopOverlayHotKey;

                List<string> parsed = new List<string>();

                foreach (string key in _DesktopOverlayHotKey.Split('|'))
                {
                    if (!string.IsNullOrEmpty(key))
                        parsed.Add(key);
                }

                return parsed;
            }
            set
            {
                string concatenated = "";

                foreach (string key in value)
                {
                    if (!string.IsNullOrEmpty(concatenated))
                        concatenated += "|";

                    concatenated += key;
                }

                _DesktopOverlayHotKey = concatenated;
                Properties.Settings.Default.DesktopOverlayHotKey = _DesktopOverlayHotKey;
                Save();
            }
        }
        #endregion

        #region Taskbar
        public static bool EnableTaskbar
        {
            get
            {
                if (_EnableTaskbar == null)
                    _EnableTaskbar = Properties.Settings.Default.EnableTaskbar;

                return (bool)_EnableTaskbar;
            }
            set
            {
                _EnableTaskbar = value;
                Properties.Settings.Default.EnableTaskbar = (bool)_EnableTaskbar;
                Save();
            }
        }

        public static int TaskbarMode
        {
            get
            {
                if (_TaskbarMode == null)
                    _TaskbarMode = Properties.Settings.Default.TaskbarMode;

                return (int)_TaskbarMode;
            }
            set
            {
                _TaskbarMode = value;
                Properties.Settings.Default.TaskbarMode = (int)_TaskbarMode;
                Save();
            }
        }

        public static bool EnableTaskbarPolling
        {
            get
            {
                if (_EnableTaskbarPolling == null)
                    _EnableTaskbarPolling = Properties.Settings.Default.EnableTaskbarPolling;

                return (bool)_EnableTaskbarPolling;
            }
            set
            {
                _EnableTaskbarPolling = value;
                Properties.Settings.Default.EnableTaskbarPolling = (bool)_EnableTaskbarPolling;
                Save();
            }
        }

        public static int TaskbarPosition
        {
            get
            {
                if (_TaskbarPosition == null)
                    _TaskbarPosition = Properties.Settings.Default.TaskbarPosition;

                return (int)_TaskbarPosition;
            }
            set
            {
                _TaskbarPosition = value;
                Properties.Settings.Default.TaskbarPosition = (int)_TaskbarPosition;
                Save();
            }
        }

        public static int TaskbarIconSize
        {
            get
            {
                if (_TaskbarIconSize == null)
                    _TaskbarIconSize = Properties.Settings.Default.TaskbarIconSize;

                return (int)_TaskbarIconSize;
            }
            set
            {
                _TaskbarIconSize = value;
                Properties.Settings.Default.TaskbarIconSize = (int)_TaskbarIconSize;
                Save();
            }
        }
        #endregion

        #region Menu Bar
        public static string DefaultProgramsCategory
        {
            get
            {
                if (_DefaultProgramsCategory == null)
                    _DefaultProgramsCategory = Properties.Settings.Default.DefaultProgramsCategory;

                return _DefaultProgramsCategory;
            }
            set
            {
                _DefaultProgramsCategory = value;
                Properties.Settings.Default.DefaultProgramsCategory = _DefaultProgramsCategory;
                Save();
            }
        }

        public static bool EnableMenuBarShadow
        {
            get
            {
                if (_EnableMenuBarShadow == null)
                    _EnableMenuBarShadow = Properties.Settings.Default.EnableMenuBarShadow;

                return (bool)_EnableMenuBarShadow;
            }
            set
            {
                _EnableMenuBarShadow = value;
                Properties.Settings.Default.EnableMenuBarShadow = (bool)_EnableMenuBarShadow;
                Save();
            }
        }

        public static bool EnableSysTray
        {
            get
            {
                if (_EnableSysTray == null)
                    _EnableSysTray = Properties.Settings.Default.EnableSysTray;

                return (bool)_EnableSysTray;
            }
            set
            {
                _EnableSysTray = value;
                Properties.Settings.Default.EnableSysTray = (bool)_EnableSysTray;
                Save();
            }
        }

        public static bool SysTrayAlwaysExpanded
        {
            get
            {
                if (_SysTrayAlwaysExpanded == null)
                    _SysTrayAlwaysExpanded = Properties.Settings.Default.SysTrayAlwaysExpanded;

                return (bool)_SysTrayAlwaysExpanded;
            }
            set
            {
                _SysTrayAlwaysExpanded = value;
                Properties.Settings.Default.SysTrayAlwaysExpanded = (bool)_SysTrayAlwaysExpanded;
                Save();
            }
        }

        public static bool EnableSysTrayRehook
        {
            get
            {
                if (_EnableSysTrayRehook == null)
                    _EnableSysTrayRehook = Properties.Settings.Default.EnableSysTrayRehook;

                return (bool)_EnableSysTrayRehook;
            }
            set
            {
                _EnableSysTrayRehook = value;
                Properties.Settings.Default.EnableSysTrayRehook = (bool)_EnableSysTrayRehook;
                Save();
            }
        }

        public static bool EnableCairoMenuHotKey
        {
            get
            {
                if (_EnableCairoMenuHotKey == null)
                    _EnableCairoMenuHotKey = Properties.Settings.Default.EnableCairoMenuHotKey;

                return (bool)_EnableCairoMenuHotKey;
            }
            set
            {
                _EnableCairoMenuHotKey = value;
                Properties.Settings.Default.EnableCairoMenuHotKey = (bool)_EnableCairoMenuHotKey;
                Save();
            }
        }

        public static List<string> CairoMenuHotKey
        {
            get
            {
                if (string.IsNullOrEmpty(_CairoMenuHotKey))
                    _CairoMenuHotKey = Properties.Settings.Default.CairoMenuHotKey;

                List<string> parsed = new List<string>();

                foreach (string key in _CairoMenuHotKey.Split('|'))
                {
                    if (!string.IsNullOrEmpty(key))
                        parsed.Add(key);
                }

                return parsed;
            }
            set
            {
                string concatenated = "";

                foreach (string key in value)
                {
                    if (!string.IsNullOrEmpty(concatenated))
                        concatenated += "|";

                    concatenated += key;
                }

                _CairoMenuHotKey = concatenated;
                Properties.Settings.Default.CairoMenuHotKey = _CairoMenuHotKey;
                Save();
            }
        }
        #endregion

        #endregion

        private static void Save()
        {
            Properties.Settings.Default.Save();
        }

        public static void Upgrade()
        {
            Properties.Settings.Default.Upgrade();

            // clear cached value since it may be wrong after upgrade
            _IsFirstRun = null;
        }
    }
}
