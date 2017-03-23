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
        private static bool? _EnableSubDirs;
        private static bool? _ShowFileExtensions;

        // Desktop
        private static bool? _EnableDesktop;
        private static bool? _EnableDynamicDesktop;
        private static string _DesktopDirectory;

        // Taskbar
        private static bool? _EnableTaskbar;
        private static int? _WindowsTaskbarMode;

        // Menu Bar
        private static string _DefaultProgramsCategory;
        private static bool? _EnableMenuBarShadow;
        private static bool? _EnableSysTray;
        private static bool? _SysTrayAlwaysExpanded;
        private static bool? _EnableSysTrayRehook;

        #endregion

        #region Public properties

        // General
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

        public static bool ShowFileExtensions
        {
            get
            {
                if (_ShowFileExtensions == null)
                    _ShowFileExtensions = Properties.Settings.Default.ShowFileExtensions;

                return (bool)_ShowFileExtensions;
            }
            set
            {
                _ShowFileExtensions = value;
                Properties.Settings.Default.ShowFileExtensions = (bool)_ShowFileExtensions;
                Save();
            }
        }

        // Desktop
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

        // Taskbar
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

        public static int WindowsTaskbarMode
        {
            get
            {
                if (_WindowsTaskbarMode == null)
                    _WindowsTaskbarMode = Properties.Settings.Default.WindowsTaskbarMode;

                return (int)_WindowsTaskbarMode;
            }
            set
            {
                _WindowsTaskbarMode = value;
                Properties.Settings.Default.WindowsTaskbarMode = (int)_WindowsTaskbarMode;
                Save();
            }
        }

        // Menu Bar
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
