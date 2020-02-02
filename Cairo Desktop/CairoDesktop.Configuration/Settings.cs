using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;

namespace CairoDesktop.Configuration
{
    public class Settings
    {
        public delegate void SettingsEvent(object sender, EventArgs args);
        public static event SettingsEvent Initializing = delegate { }; // add empty delegate!
        public static event SettingsEvent Initialized = delegate { }; // add empty delegate!

        private static bool _initialized = false;

        private static Settings instance;

        public Settings() { }

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    // add SettingsInitializing event handler
                    // this should be where plugins can register PropertySettings with our core
                    Initializing(null, null);

                    instance = new Settings();
                    _initialized = true;

                    // add SettingsInitialized event handler
                    // This should inform the system that all PropertySettings should be added and can now be accessed safely
                    Initialized(instance, null);
                }

                return instance;
            }
        }

        #region Private properties for caching

        // General
        private static bool? _IsFirstRun;
        private static string _CairoTheme;
        private static string _Language;
        private static bool? _EnableSubDirs;
        private static bool? _FoldersOpenDesktopOverlay;

        // Desktop
        private static bool? _EnableDesktop;
        private static bool? _EnableDynamicDesktop;
        private static string _DesktopDirectory;
        private static int? _DesktopLabelPosition;
        private static int? _DesktopIconSize;
        private static bool? _EnableDesktopOverlayHotKey;
        private static string _DesktopOverlayHotKey;

        // Taskbar
        private static bool? _EnableTaskbar;
        private static int? _TaskbarMode;
        private static bool? _EnableTaskbarPolling;
        private static int? _TaskbarPosition;
        private static int? _TaskbarIconSize;
        private static bool? _EnableTaskbarMultiMon;
        private static bool? _FullWidthTaskBar;
        private static int? _TaskbarMiddleClick;


        // Menu Bar
        private static string _DefaultProgramsCategory;
        private static bool? _EnableMenuBarShadow;
        private static bool? _EnableSysTray;
        private static bool? _SysTrayAlwaysExpanded;
        private static bool? _EnableCairoMenuHotKey;
        private static string _CairoMenuHotKey;
        private static bool? _EnableMenuBarBlur;
        private static bool? _EnableMenuBarMultiMon;
        private static bool? _ShowHibernate;

        // Advanced
        private static string _TimeFormat;
        private static string _DateFormat;
        private static string _FileManager;
        private static bool? _ForceSoftwareRendering;

        // Logging
        private static string _LogSeverity;

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

        public static bool FoldersOpenDesktopOverlay
        {
            get
            {
                if (_FoldersOpenDesktopOverlay == null)
                    _FoldersOpenDesktopOverlay = Properties.Settings.Default.FoldersOpenDesktopOverlay;

                return (bool)_FoldersOpenDesktopOverlay;
            }
            set
            {
                _FoldersOpenDesktopOverlay = value;
                Properties.Settings.Default.FoldersOpenDesktopOverlay = (bool)_FoldersOpenDesktopOverlay;
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

        public static bool EnableDesktopOverlayHotKey
        {
            get
            {
                if (_EnableDesktopOverlayHotKey == null)
                    _EnableDesktopOverlayHotKey = Properties.Settings.Default.EnableDesktopOverlayHotKey;

                return (bool)_EnableDesktopOverlayHotKey;
            }
            set
            {
                _EnableDesktopOverlayHotKey = value;
                Properties.Settings.Default.EnableDesktopOverlayHotKey = (bool)_EnableDesktopOverlayHotKey;
                Save();
            }
        }

        public static List<string> DesktopOverlayHotKey
        {
            get
            {
                if (string.IsNullOrEmpty(_DesktopOverlayHotKey))
                    _DesktopOverlayHotKey = Properties.Settings.Default.DesktopOverlayHotKey;

                return parseConcatString(_DesktopOverlayHotKey, '|');
            }
            set
            {
                _DesktopOverlayHotKey = concatStringList(value, '|');
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

        public static bool EnableTaskbarMultiMon
        {
            get
            {
                if (_EnableTaskbarMultiMon == null)
                    _EnableTaskbarMultiMon = Properties.Settings.Default.EnableTaskbarMultiMon;

                return (bool)_EnableTaskbarMultiMon;
            }
            set
            {
                _EnableTaskbarMultiMon = value;
                Properties.Settings.Default.EnableTaskbarMultiMon = (bool)_EnableTaskbarMultiMon;
                Save();
            }
        }

        public static bool FullWidthTaskBar
        {
            get
            {
                if (_FullWidthTaskBar == null)
                    _FullWidthTaskBar = Properties.Settings.Default.FullWidthTaskBar;

                return (bool)_FullWidthTaskBar;
            }
            set
            {
                _FullWidthTaskBar = value;
                Properties.Settings.Default.FullWidthTaskBar = (bool)_FullWidthTaskBar;
                Save();
            }
        }

        public static int TaskbarMiddleClick
        {
            get
            {
                if (_TaskbarMiddleClick == null)
                    _TaskbarMiddleClick = Properties.Settings.Default.TaskbarMiddleClick;

                return (int)_TaskbarMiddleClick;
            }
            set
            {
                _TaskbarMiddleClick = value;
                Properties.Settings.Default.TaskbarMiddleClick = (int)_TaskbarMiddleClick;
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

                return parseConcatString(_CairoMenuHotKey, '|');
            }
            set
            {
                _CairoMenuHotKey = concatStringList(value, '|');
                Properties.Settings.Default.CairoMenuHotKey = _CairoMenuHotKey;
                Save();
            }
        }

        public static bool EnableMenuBarBlur
        {
            get
            {
                if (_EnableMenuBarBlur == null)
                    _EnableMenuBarBlur = Properties.Settings.Default.EnableMenuBarBlur;

                return (bool)_EnableMenuBarBlur;
            }
            set
            {
                _EnableMenuBarBlur = value;
                Properties.Settings.Default.EnableMenuBarBlur = (bool)_EnableMenuBarBlur;
                Save();
            }
        }

        public static bool EnableMenuBarMultiMon
        {
            get
            {
                if (_EnableMenuBarMultiMon == null)
                    _EnableMenuBarMultiMon = Properties.Settings.Default.EnableMenuBarMultiMon;

                return (bool)_EnableMenuBarMultiMon;
            }
            set
            {
                _EnableMenuBarMultiMon = value;
                Properties.Settings.Default.EnableMenuBarMultiMon = (bool)_EnableMenuBarMultiMon;
                Save();
            }
        }

        public static bool ShowHibernate
        {
            get
            {
                if (_ShowHibernate == null)
                    _ShowHibernate = Properties.Settings.Default.ShowHibernate;

                return (bool)_ShowHibernate;
            }
            set
            {
                _ShowHibernate = value;
                Properties.Settings.Default.ShowHibernate = (bool)_ShowHibernate;
                Save();
            }
        }

        public static int ProgramsMenuLayout
        {
            get
            {
                return Properties.Settings.Default.ProgramsMenuLayout;
            }
            set
            {
                Properties.Settings.Default.ProgramsMenuLayout = value;
                Save();
            }
        }
        #endregion

        #region Advanced

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

        #region Logging

        public static string LogSeverity
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_LogSeverity))
                    _LogSeverity = Properties.Settings.Default.LogSeverity;

                return _LogSeverity;
            }
            set
            {
                _LogSeverity = value;
                Properties.Settings.Default.LogSeverity = _LogSeverity;
                Save();
            }
        }

        #endregion

        #endregion

        #region Helpers
        private static List<string> parseConcatString(string concat, char separator)
        {
            List<string> parsed = new List<string>();

            foreach (string key in concat.Split(separator))
            {
                if (!string.IsNullOrEmpty(key))
                    parsed.Add(key);
            }

            return parsed;
        }

        private static string concatStringList(List<string> list, char separator)
        {
            string concatenated = "";

            foreach (string key in list)
            {
                if (!string.IsNullOrEmpty(concatenated))
                    concatenated += separator.ToString();

                concatenated += key;
            }

            return concatenated;
        }
        #endregion


        public static void Save()
        {
            Properties.Settings.Default.Save();
        }

        public static void Upgrade()
        {
            Properties.Settings.Default.Upgrade();

            // clear cached value since it may be wrong after upgrade
            _IsFirstRun = null;
        }

        public object this[string propertyName]
        {
            get
            {
                return Properties.Settings.Default[propertyName];
            }
            set
            {
                Properties.Settings.Default[propertyName] = value;
            }
        }

        public static bool Exists(string name)
        {
            return Properties.Settings.Default.Properties[name] != null;
        }

        public static void AddPropertySetting(string name, Type type, object defaultValue)
        {
            // Only allow settings to be added during initialization
            if (!_initialized)
            {
                string providerName = "LocalFileSettingsProvider";

                SettingsAttributeDictionary attributes = new SettingsAttributeDictionary();
                UserScopedSettingAttribute attr = new UserScopedSettingAttribute();
                attributes.Add(attr.TypeId, attr);

                var prop = new SettingsProperty(
                    new SettingsProperty(name
                    , type
                    , Properties.Settings.Default.Providers[providerName]
                    , false
                    , defaultValue
                    , SettingsSerializeAs.String
                    , attributes
                    , false
                    , false));

                Properties.Settings.Default.Properties.Add(prop);
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
            }
        }

    }
}
