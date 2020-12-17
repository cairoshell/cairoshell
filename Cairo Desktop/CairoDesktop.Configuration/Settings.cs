using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CairoDesktop.Configuration
{
    public class Settings : INotifyPropertyChanged
    {
        public delegate void SettingsEventHandler(object sender, EventArgs args);
        public static event SettingsEventHandler Initializing;
        public static event SettingsEventHandler Initialized;
        private static bool _initialized = false;

        // For Reference
        // https://docs.microsoft.com/en-us/dotnet/framework/wpf/data/how-to-implement-property-change-notification
        public event PropertyChangedEventHandler PropertyChanged;

        private static Settings instance;
        private bool _upgrading;
        private readonly Properties.Settings cairoSettings;

        private Settings()
        {
            cairoSettings = Properties.Settings.Default;
            cairoSettings.PropertyChanged += CairoSettings_PropertyChanged;
        }

        private void CairoSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_upgrading)
                return;

            // Save the planet, one property at a time.
            Save();

            // Tell the rest of Cairo.
            OnNotifyPropertyChanged(e.PropertyName);
        }


        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void OnNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    // add SettingsInitializing event handler
                    // this should be where plugins can register PropertySettings with our core
                    Initializing?.Invoke(null, new EventArgs());

                    instance = new Settings();
                    _initialized = true;

                    // add SettingsInitialized event handler
                    // This should inform the system that all PropertySettings should be added and can now be accessed safely
                    Initialized?.Invoke(instance, new EventArgs());
                }

                return instance;
            }
        }

        #region Public properties

        #region General
        public bool IsFirstRun
        {
            get
            {
                return cairoSettings.IsFirstRun;
            }
            set
            {
                if (cairoSettings.IsFirstRun != value)
                {
                    cairoSettings.IsFirstRun = value;
                }
            }
        }

        public string CairoTheme
        {
            get
            {
                return cairoSettings.CairoTheme;
            }
            set
            {
                if (cairoSettings.CairoTheme != value)
                {
                    cairoSettings.CairoTheme = value;
                }
            }
        }

        public string Language
        {
            get
            {
                // if we are Null or Empty, use the process's culture
                if (string.IsNullOrEmpty(cairoSettings.Language))
                {
                    cairoSettings.Language = CultureInfo.CurrentUICulture.Name.Replace('-', '_');
                }

                return cairoSettings.Language;
            }
            set
            {
                // if we are Null or Empty, use the process's culture
                if (string.IsNullOrEmpty(value))
                {
                    cairoSettings.Language = CultureInfo.CurrentUICulture.Name.Replace('-', '_');
                }
                else if (cairoSettings.Language != value)
                {
                    cairoSettings.Language = value;
                }
            }
        }

        public bool FoldersOpenDesktopOverlay
        {
            get
            {
                return cairoSettings.FoldersOpenDesktopOverlay;
            }
            set
            {
                if (cairoSettings.FoldersOpenDesktopOverlay != value)
                {
                    cairoSettings.FoldersOpenDesktopOverlay = value;
                }
            }
        }
        #endregion

        #region Desktop
        public bool EnableDesktop
        {
            get
            {
                return cairoSettings.EnableDesktop;
            }
            set
            {
                if (cairoSettings.EnableDesktop != value)
                {
                    cairoSettings.EnableDesktop = value;
                }
            }
        }

        public bool EnableDynamicDesktop
        {
            get
            {
                return cairoSettings.EnableDynamicDesktop;
            }
            set
            {
                if (cairoSettings.EnableDynamicDesktop != value)
                {
                    cairoSettings.EnableDynamicDesktop = value;
                }
            }
        }

        public System.Windows.Point DesktopNavigationToolbarLocation
        {
            get
            {
                return cairoSettings.DesktopNavigationToolbarLocation;
            }
            set
            {
                if (cairoSettings.DesktopNavigationToolbarLocation != value)
                {
                    cairoSettings.DesktopNavigationToolbarLocation = value;
                }
            }
        }

        public string DesktopDirectory
        {
            get
            {
                return cairoSettings.DesktopDirectory;
            }
            set
            {
                if (cairoSettings.DesktopDirectory != value)
                {
                    cairoSettings.DesktopDirectory = value;
                }
            }
        }

        public int DesktopLabelPosition
        {
            get
            {
                return cairoSettings.DesktopLabelPosition;
            }
            set
            {
                if (cairoSettings.DesktopLabelPosition != value)
                {
                    cairoSettings.DesktopLabelPosition = value;
                }
            }
        }

        public int DesktopIconSize
        {
            get
            {
                return cairoSettings.DesktopIconSize;
            }
            set
            {
                if (cairoSettings.DesktopIconSize != value)
                {
                    cairoSettings.DesktopIconSize = value;
                }
            }
        }

        public bool EnableDesktopOverlayHotKey
        {
            get
            {
                return cairoSettings.EnableDesktopOverlayHotKey;
            }
            set
            {
                if (cairoSettings.EnableDesktopOverlayHotKey != value)
                {
                    cairoSettings.EnableDesktopOverlayHotKey = value;
                }
            }
        }

        public string[] DesktopOverlayHotKey
        {
            get
            {
                return parseConcatString(cairoSettings.DesktopOverlayHotKey, '|').ToArray();
            }
            set
            {
                string val = concatStringList(value, '|');
                if (cairoSettings.DesktopOverlayHotKey != val)
                {
                    cairoSettings.DesktopOverlayHotKey = val;
                }
            }
        }

        public string DesktopBackgroundType
        {
            get
            {
                return cairoSettings.DesktopBackgroundType;
            }
            set
            {
                if (cairoSettings.DesktopBackgroundType != value)
                {
                    cairoSettings.DesktopBackgroundType = value;
                }
            }
        }

        public int BingWallpaperStyle
        {
            get
            {
                return cairoSettings.BingWallpaperStyle;
            }
            set
            {
                if (cairoSettings.BingWallpaperStyle != value)
                {
                    cairoSettings.BingWallpaperStyle = value;
                }
            }
        }

        public string CairoBackgroundImagePath
        {
            get
            {
                return cairoSettings.CairoBackgroundImagePath;
            }
            set
            {
                if (cairoSettings.CairoBackgroundImagePath != value)
                {
                    cairoSettings.CairoBackgroundImagePath = value;
                }
            }
        }

        public int CairoBackgroundImageStyle
        {
            get
            {
                return cairoSettings.CairoBackgroundImageStyle;
            }
            set
            {
                if (cairoSettings.CairoBackgroundImageStyle != value)
                {
                    cairoSettings.CairoBackgroundImageStyle = value;
                }
            }
        }

        public string CairoBackgroundVideoPath
        {
            get
            {
                return cairoSettings.CairoBackgroundVideoPath;
            }
            set
            {
                if (cairoSettings.CairoBackgroundVideoPath != value)
                {
                    cairoSettings.CairoBackgroundVideoPath = value;
                }
            }
        }
        #endregion

        #region Taskbar
        public bool EnableTaskbar
        {
            get
            {
                return cairoSettings.EnableTaskbar;
            }
            set
            {
                if (cairoSettings.EnableTaskbar != value)
                {
                    cairoSettings.EnableTaskbar = value;
                }
            }
        }

        public int TaskbarMode
        {
            get
            {
                return cairoSettings.TaskbarMode;
            }
            set
            {
                if (cairoSettings.TaskbarMode != value)
                {
                    cairoSettings.TaskbarMode = value;
                }
            }
        }

        public int TaskbarPosition
        {
            get
            {
                return cairoSettings.TaskbarPosition;
            }
            set
            {
                if (cairoSettings.TaskbarPosition != value)
                {
                    cairoSettings.TaskbarPosition = value;
                }
            }
        }

        public int TaskbarIconSize
        {
            get
            {
                return cairoSettings.TaskbarIconSize;
            }
            set
            {
                if (cairoSettings.TaskbarIconSize != value)
                {
                    cairoSettings.TaskbarIconSize = value;
                }
            }
        }

        public bool EnableTaskbarMultiMon
        {
            get
            {
                return cairoSettings.EnableTaskbarMultiMon;
            }
            set
            {
                if (cairoSettings.EnableTaskbarMultiMon != value)
                {
                    cairoSettings.EnableTaskbarMultiMon = value;
                }
            }
        }

        public bool FullWidthTaskBar
        {
            get
            {
                return cairoSettings.FullWidthTaskBar;
            }
            set
            {
                if (cairoSettings.FullWidthTaskBar != value)
                {
                    cairoSettings.FullWidthTaskBar = value;
                }
            }
        }

        public int TaskbarMiddleClick
        {
            get
            {
                return cairoSettings.TaskbarMiddleClick;
            }
            set
            {
                if (cairoSettings.TaskbarMiddleClick != value)
                {
                    cairoSettings.TaskbarMiddleClick = value;
                }
            }
        }

        public bool EnableTaskbarThumbnails
        {
            get
            {
                return cairoSettings.EnableTaskbarThumbnails;
            }
            set
            {
                if (cairoSettings.EnableTaskbarThumbnails != value)
                {
                    cairoSettings.EnableTaskbarThumbnails = value;
                }
            }
        }
        #endregion

        #region Menu Bar
        public string DefaultProgramsCategory
        {
            get
            {
                return cairoSettings.DefaultProgramsCategory;
            }
            set
            {
                if (cairoSettings.DefaultProgramsCategory != value)
                {
                    cairoSettings.DefaultProgramsCategory = value;
                }
            }
        }

        public bool EnableMenuBar
        {
            get
            {
                return cairoSettings.EnableMenuBar;
            }
            set
            {
                if (cairoSettings.EnableMenuBar != value)
                {
                    cairoSettings.EnableMenuBar = value;
                }
            }
        }

        public bool EnableMenuBarShadow
        {
            get
            {
                return cairoSettings.EnableMenuBarShadow;
            }
            set
            {
                if (cairoSettings.EnableMenuBarShadow != value)
                {
                    cairoSettings.EnableMenuBarShadow = value;
                }
            }
        }

        public bool EnableSysTray
        {
            get
            {
                return cairoSettings.EnableSysTray;
            }
            set
            {
                if (cairoSettings.EnableSysTray != value)
                {
                    cairoSettings.EnableSysTray = value;
                }
            }
        }

        public bool SysTrayAlwaysExpanded
        {
            get
            {
                return cairoSettings.SysTrayAlwaysExpanded;
            }
            set
            {
                if (cairoSettings.SysTrayAlwaysExpanded != value)
                {
                    cairoSettings.SysTrayAlwaysExpanded = value;
                }
            }
        }

        public bool EnableCairoMenuHotKey
        {
            get
            {
                return cairoSettings.EnableCairoMenuHotKey;
            }
            set
            {
                if (cairoSettings.EnableCairoMenuHotKey != value)
                {
                    cairoSettings.EnableCairoMenuHotKey = value;
                }
            }
        }

        public string[] CairoMenuHotKey
        {
            get
            {
                return parseConcatString(cairoSettings.CairoMenuHotKey, '|').ToArray();
            }
            set
            {
                string val = concatStringList(value, '|');
                if (cairoSettings.CairoMenuHotKey != val)
                {
                    cairoSettings.CairoMenuHotKey = val;
                }
            }
        }

        public bool EnableMenuBarBlur
        {
            get
            {
                return cairoSettings.EnableMenuBarBlur;
            }
            set
            {
                if (cairoSettings.EnableMenuBarBlur != value)
                {
                    cairoSettings.EnableMenuBarBlur = value;
                }
            }
        }

        public bool EnableMenuBarMultiMon
        {
            get
            {
                return cairoSettings.EnableMenuBarMultiMon;
            }
            set
            {
                if (cairoSettings.EnableMenuBarMultiMon != value)
                {
                    cairoSettings.EnableMenuBarMultiMon = value;
                }
            }
        }

        public bool ShowHibernate
        {
            get
            {
                return cairoSettings.ShowHibernate;
            }
            set
            {
                if (cairoSettings.ShowHibernate != value)
                {
                    cairoSettings.ShowHibernate = value;
                }
            }
        }

        public int ProgramsMenuLayout
        {
            get
            {
                return cairoSettings.ProgramsMenuLayout;
            }
            set
            {
                if (cairoSettings.ProgramsMenuLayout != value)
                {
                    cairoSettings.ProgramsMenuLayout = value;
                }
            }
        }

        public string[] PinnedNotifyIcons
        {
            get
            {
                return parseConcatString(cairoSettings.PinnedNotifyIcons, '|');
            }
            set
            {
                string val = concatStringList(value, '|');
                if (cairoSettings.PinnedNotifyIcons != val)
                {
                    cairoSettings.PinnedNotifyIcons = val;
                }
            }
        }

        public bool EnableMenuExtraVolume
        {
            get
            {
                return cairoSettings.EnableMenuExtraVolume;
            }
            set
            {
                if (cairoSettings.EnableMenuExtraVolume != value)
                {
                    cairoSettings.EnableMenuExtraVolume = value;
                }
            }
        }

        public bool EnableMenuExtraActionCenter
        {
            get
            {
                return cairoSettings.EnableMenuExtraActionCenter;
            }
            set
            {
                if (cairoSettings.EnableMenuExtraActionCenter != value)
                {
                    cairoSettings.EnableMenuExtraActionCenter = value;
                }
            }
        }

        public bool EnableMenuExtraClock
        {
            get
            {
                return cairoSettings.EnableMenuExtraClock;
            }
            set
            {
                if (cairoSettings.EnableMenuExtraClock != value)
                {
                    cairoSettings.EnableMenuExtraClock = value;
                }
            }
        }

        public bool EnableMenuExtraSearch
        {
            get
            {
                return cairoSettings.EnableMenuExtraSearch;
            }
            set
            {
                if (cairoSettings.EnableMenuExtraSearch != value)
                {
                    cairoSettings.EnableMenuExtraSearch = value;
                }
            }
        }
        #endregion

        #region Advanced
        public string TimeFormat
        {
            get
            {
                // if Null or Empty null, get CurrentCulture
                if (string.IsNullOrEmpty(cairoSettings.TimeFormat))
                {
                    cairoSettings.TimeFormat = $"ddd {CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern}";
                }

                return cairoSettings.TimeFormat;
            }
            set
            {
                if (string.IsNullOrEmpty(cairoSettings.TimeFormat))
                {
                    cairoSettings.TimeFormat = $"ddd {CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern}";
                }
                else if (cairoSettings.TimeFormat != value)
                {
                    cairoSettings.TimeFormat = value;
                }
            }
        }

        public string DateFormat
        {
            get
            {
                if (string.IsNullOrEmpty(cairoSettings.DateFormat))
                {
                    cairoSettings.DateFormat = "D";
                }

                return cairoSettings.DateFormat;
            }
            set
            {
                if (string.IsNullOrEmpty(cairoSettings.DateFormat))
                {
                    cairoSettings.DateFormat = "D";
                }
                else if (cairoSettings.DateFormat != value)
                {
                    cairoSettings.DateFormat = value;
                }
            }
        }

        public string FileManager
        {
            get
            {
                return cairoSettings.FileManager;
            }
            set
            {
                if (cairoSettings.FileManager != value)
                {
                    cairoSettings.FileManager = value;
                }
            }
        }

        public bool ForceSoftwareRendering
        {
            get
            {
                return cairoSettings.ForceSoftwareRendering;
            }
            set
            {
                if (cairoSettings.ForceSoftwareRendering != value)
                {
                    cairoSettings.ForceSoftwareRendering = value;
                }
            }
        }
        #endregion

        #region Logging
        public string LogSeverity
        {
            get
            {
                return cairoSettings.LogSeverity;
            }
            set
            {
                if (cairoSettings.LogSeverity != value)
                {
                    cairoSettings.LogSeverity = value;
                }
            }
        }
        #endregion

        #endregion

        #region Helpers
        private string[] parseConcatString(string concat, char separator)
        {
            List<string> parsed = new List<string>();

            foreach (string key in concat.Split(separator))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    parsed.Add(key);
                }
            }

            return parsed.ToArray();
        }

        private string concatStringList(IEnumerable<string> list, char separator)
        {
            string concatenated = "";

            foreach (string key in list)
            {
                if (!string.IsNullOrEmpty(concatenated))
                {
                    concatenated += separator.ToString();
                }

                concatenated += key;
            }

            return concatenated;
        }
        #endregion

        public void Save()
        {
            cairoSettings.Save();
        }

        public void Upgrade()
        {
            _upgrading = true;
            cairoSettings.Upgrade();
            _upgrading = false;
        }

        public object this[string propertyName]
        {
            get
            {
                return cairoSettings[propertyName];
            }
            set
            {
                cairoSettings[propertyName] = value;
            }
        }

        public bool Exists(string name)
        {
            return cairoSettings.Properties[name] != null;
        }

        public void AddPropertySetting(string name, Type type, object defaultValue)
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
                    , cairoSettings.Providers[providerName]
                    , false
                    , defaultValue
                    , SettingsSerializeAs.String
                    , attributes
                    , false
                    , false));

                cairoSettings.Properties.Add(prop);
                cairoSettings.Save();
                cairoSettings.Reload();
            }
        }

    }
}