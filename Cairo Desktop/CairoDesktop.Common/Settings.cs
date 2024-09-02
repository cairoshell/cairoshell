using CairoDesktop.Common.Logging;
using ManagedShell.AppBar;
using ManagedShell.Common.Enums;
using ManagedShell.WindowsTray;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace CairoDesktop.Common
{
    public class Settings : INotifyPropertyChanged
    {
        private static Settings instance;

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = _settingsFile.Settings;
                    _isInitializing = false;
                }

                return instance;
            }
        }

        private static string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cairo Desktop", "settings.json");
        private static bool _isInitializing = true;
        private static SettingsFile<Settings> _settingsFile = new SettingsFile<Settings>(_settingsPath, new Settings());
        
        public event PropertyChangedEventHandler PropertyChanged;

        // This should not be used directly! Unfortunately it must be public for JsonSerializer.
        public Settings()
        {
            PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            _settingsFile.Settings = this;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (field?.Equals(value) != true)
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected void SetEnum<T>(ref T field, T value, [CallerMemberName] string propertyName = "") where T : Enum
        {
            if (!field.Equals(value))
            {
                if (!Enum.IsDefined(typeof(T), value))
                {
                    return;
                }

                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        #region Public properties

        #region General
        private bool _isFirstRun = true;
        public bool IsFirstRun
        {
            get => _isFirstRun;
            set => Set(ref _isFirstRun, value);
        }

        private string _theme = "Default";
        public string Theme
        {
            get => _theme;
            set => Set(ref _theme, value);
        }

        private string _language = "";
        public string Language
        {
            get
            {
                // if we are Null or Empty, use the process's culture
                if (string.IsNullOrEmpty(_language))
                {
                    _language = CultureInfo.CurrentUICulture.Name.Replace('-', '_');
                }

                return _language;
            }
            set
            {
                // if we are Null or Empty, use the process's culture
                if (string.IsNullOrEmpty(value))
                {
                    Set(ref _language, CultureInfo.CurrentUICulture.Name.Replace('-', '_'));
                }
                else if (_language != value)
                {
                    Set(ref _language, value);
                }
            }
        }

        private bool _foldersOpenDesktopOverlay = true;
        public bool FoldersOpenDesktopOverlay
        {
            get => _foldersOpenDesktopOverlay;
            set => Set(ref _foldersOpenDesktopOverlay, value);
        }
        #endregion

        #region Desktop
        private bool _enableDesktop = true;
        public bool EnableDesktop
        {
            get => _enableDesktop;
            set => Set(ref _enableDesktop, value);
        }

        private bool _enableDynamicDesktop = true;
        public bool EnableDynamicDesktop
        {
            get => _enableDynamicDesktop;
            set => Set(ref _enableDynamicDesktop, value);
        }

        private System.Windows.Point _desktopNavigationToolbarLocation = new System.Windows.Point(0.5, 0.8);
        public System.Windows.Point DesktopNavigationToolbarLocation
        {
            get => _desktopNavigationToolbarLocation;
            set => Set(ref _desktopNavigationToolbarLocation, value);
        }

        private string _desktopDirectory;
        public string DesktopDirectory
        {
            get => _desktopDirectory;
            set => Set(ref _desktopDirectory, value);
        }

        private int _desktopLabelPosition;
        public int DesktopLabelPosition
        {
            get => _desktopLabelPosition;
            set => Set(ref _desktopLabelPosition, value);
        }

        private IconSize _desktopIconSize = IconSize.ExtraLarge;
        public IconSize DesktopIconSize
        {
            get => _desktopIconSize;
            set => SetEnum(ref _desktopIconSize, value);
        }

        private bool _enableDesktopOverlayHotKey = true;
        public bool EnableDesktopOverlayHotKey
        {
            get => _enableDesktopOverlayHotKey;
            set => Set(ref _enableDesktopOverlayHotKey, value);
        }

        private string[] _desktopOverlayHotKey = { "Win", "Shift", "D" };
        public string[] DesktopOverlayHotKey
        {
            get => _desktopOverlayHotKey;
            set => Set(ref _desktopOverlayHotKey, value);
        }

        private string _desktopBackgroundType = "windowsDefaultBackground";
        public string DesktopBackgroundType
        {
            get => _desktopBackgroundType;
            set => Set(ref _desktopBackgroundType, value);
        }

        private int _bingWallpaperStyle = 4;
        public int BingWallpaperStyle
        {
            get => _bingWallpaperStyle;
            set => Set(ref _bingWallpaperStyle, value);
        }

        private string _cairoBackgroundImagePath;
        public string CairoBackgroundImagePath
        {
            get => _cairoBackgroundImagePath;
            set => Set(ref _cairoBackgroundImagePath, value);
        }

        private int _cairoBackgroundImageStyle = 4;
        public int CairoBackgroundImageStyle
        {
            get => _cairoBackgroundImageStyle;
            set => Set(ref _cairoBackgroundImageStyle, value);
        }

        private string _cairoBackgroundVideoPath;
        public string CairoBackgroundVideoPath
        {
            get => _cairoBackgroundVideoPath;
            set => Set(ref _cairoBackgroundVideoPath, value);
        }
        #endregion

        #region Taskbar
        private bool _enableTaskbar = true;
        public bool EnableTaskbar
        {
            get => _enableTaskbar;
            set => Set(ref _enableTaskbar, value);
        }

        private int _taskbarMode;
        public int TaskbarMode
        {
            get => _taskbarMode;
            set
            {
                if (_taskbarMode != value)
                {
                    if (value == 2 && EnableMenuBarAutoHide && TaskbarEdge == MenuBarEdge)
                    {
                        // We cannot have multiple auto-hide bars on the same screen edge
                        EnableMenuBarAutoHide = false;
                    }

                    Set(ref _taskbarMode, value);
                }
            }
        }

        private AppBarEdge _TaskbarEdge = AppBarEdge.Bottom;
        public AppBarEdge TaskbarEdge
        {
            get => _TaskbarEdge;
            set
            {
                if (_TaskbarEdge != value)
                {
                    if (value == AppBarEdge.Left || value == AppBarEdge.Right)
                    {
                        // Vertical orientation is not supported
                        return;
                    }

                    if (EnableMenuBarAutoHide && TaskbarMode == 2 && value == MenuBarEdge)
                    {
                        // We cannot have multiple auto-hide bars on the same screen edge
                        EnableMenuBarAutoHide = false;
                    }

                    SetEnum(ref _TaskbarEdge, value);
                }
            }
        }

        private IconSize _taskbarIconSize = IconSize.Medium;
        public IconSize TaskbarIconSize
        {
            get => _taskbarIconSize;
            set => SetEnum(ref _taskbarIconSize, value);
        }

        private int _taskbarGroupingStyle;
        public int TaskbarGroupingStyle
        {
            get => _taskbarGroupingStyle;
            set => Set(ref _taskbarGroupingStyle, value);
        }

        private bool _showTaskbarBadges = true;
        public bool ShowTaskbarBadges
        {
            get => _showTaskbarBadges;
            set => Set(ref _showTaskbarBadges, value);
        }

        private bool _showTaskbarLabels = true;
        public bool ShowTaskbarLabels
        {
            get => _showTaskbarLabels;
            set => Set(ref _showTaskbarLabels, value);
        }

        private int _taskbarButtonWidth = 140;
        public int TaskbarButtonWidth
        {
            get => _taskbarButtonWidth;
            set => Set(ref _taskbarButtonWidth, value);
        }

        private int _taskbarButtonHeight = 29;
        public int TaskbarButtonHeight
        {
            get => _taskbarButtonHeight;
            set => Set(ref _taskbarButtonHeight, value);
        }

        private bool _enableTaskbarMultiMon;
        public bool EnableTaskbarMultiMon
        {
            get => _enableTaskbarMultiMon;
            set => Set(ref _enableTaskbarMultiMon, value);
        }

        private bool _fullWidthTaskBar;
        public bool FullWidthTaskBar
        {
            get => _fullWidthTaskBar;
            set => Set(ref _fullWidthTaskBar, value);
        }

        private int _taskbarMiddleClick;
        public int TaskbarMiddleClick
        {
            get => _taskbarMiddleClick;
            set => Set(ref _taskbarMiddleClick, value);
        }

        private bool _enableTaskbarThumbnails = true;
        public bool EnableTaskbarThumbnails
        {
            get => _enableTaskbarThumbnails;
            set => Set(ref _enableTaskbarThumbnails, value);
        }

        private int _taskbarMultiMonMode = 1;
        public int TaskbarMultiMonMode
        {
            get => _taskbarMultiMonMode;
            set => Set(ref _taskbarMultiMonMode, value);
        }
        #endregion

        #region Menu Bar
        private string _defaultProgramsCategory = "All";
        public string DefaultProgramsCategory
        {
            get => _defaultProgramsCategory;
            set => Set(ref _defaultProgramsCategory, value);
        }

        private bool _enableMenuBar = true;
        public bool EnableMenuBar
        {
            get => _enableMenuBar;
            set => Set(ref _enableMenuBar, value);
        }

        private bool _enablePlacesMenu = true;
        public bool EnablePlacesMenu
        {
            get => _enablePlacesMenu;
            set => Set(ref _enablePlacesMenu, value);
        }

        private bool _enableProgramsMenu = true;
        public bool EnableProgramsMenu
        {
            get => _enableProgramsMenu;
            set => Set(ref _enableProgramsMenu, value);
        }

        private bool _enableMenuBarShadow = true;
        public bool EnableMenuBarShadow
        {
            get => _enableMenuBarShadow;
            set => Set(ref _enableMenuBarShadow, value);
        }

        private bool _enableSysTray = true;
        public bool EnableSysTray
        {
            get => _enableSysTray;
            set => Set(ref _enableSysTray, value);
        }

        private bool _sysTrayAlwaysExpanded;
        public bool SysTrayAlwaysExpanded
        {
            get => _sysTrayAlwaysExpanded;
            set => Set(ref _sysTrayAlwaysExpanded, value);
        }

        private bool _enableCairoMenuHotKey = true;
        public bool EnableCairoMenuHotKey
        {
            get => _enableCairoMenuHotKey;
            set => Set(ref _enableCairoMenuHotKey, value);
        }

        private string[] _cairoMenuHotKey = { "Win", "Shift", "Z" };
        public string[] CairoMenuHotKey
        {
            get => _cairoMenuHotKey;
            set => Set(ref _cairoMenuHotKey, value);
        }

        private bool _enableMenuBarBlur = true;
        public bool EnableMenuBarBlur
        {
            get => _enableMenuBarBlur;
            set => Set(ref _enableMenuBarBlur, value);
        }

        private bool _enableMenuBarMultiMon;
        public bool EnableMenuBarMultiMon
        {
            get => _enableMenuBarMultiMon;
            set => Set(ref _enableMenuBarMultiMon, value);
        }

        private bool _showHibernate;
        public bool ShowHibernate
        {
            get => _showHibernate;
            set => Set(ref _showHibernate, value);
        }

        private int _programsMenuLayout;
        public int ProgramsMenuLayout
        {
            get => _programsMenuLayout;
            set => Set(ref _programsMenuLayout, value);
        }

        private string[] _pinnedNotifyIcons = { NotificationArea.HEALTH_GUID, NotificationArea.POWER_GUID, NotificationArea.NETWORK_GUID, NotificationArea.VOLUME_GUID };
        public string[] PinnedNotifyIcons
        {
            get => _pinnedNotifyIcons;
            set => Set(ref _pinnedNotifyIcons, value);
        }

        private bool _enableMenuExtraVolume = true;
        public bool EnableMenuExtraVolume
        {
            get => _enableMenuExtraVolume;
            set => Set(ref _enableMenuExtraVolume, value);
        }

        private bool _enableMenuExtraActionCenter = true;
        public bool EnableMenuExtraActionCenter
        {
            get => _enableMenuExtraActionCenter;
            set => Set(ref _enableMenuExtraActionCenter, value);
        }

        private bool _enableMenuExtraClock = true;
        public bool EnableMenuExtraClock
        {
            get => _enableMenuExtraClock;
            set => Set(ref _enableMenuExtraClock, value);
        }

        private bool _enableMenuExtraSearch = true;
        public bool EnableMenuExtraSearch
        {
            get => _enableMenuExtraSearch;
            set => Set(ref _enableMenuExtraSearch, value);
        }

        private bool _enableMenuBarAutoHide;
        public bool EnableMenuBarAutoHide
        {
            get => _enableMenuBarAutoHide;
            set
            {
                if (_enableMenuBarAutoHide != value)
                {
                    if (value && TaskbarEdge == MenuBarEdge && TaskbarMode == 2)
                    {
                        // We cannot have multiple auto-hide bars on the same screen edge
                        TaskbarEdge = MenuBarEdge == AppBarEdge.Top ? AppBarEdge.Bottom : AppBarEdge.Top;
                    }

                    Set(ref _enableMenuBarAutoHide, value);
                }
            }
        }

        private AppBarEdge _menuBarEdge = AppBarEdge.Top;
        public AppBarEdge MenuBarEdge
        {
            get => _menuBarEdge;
            set
            {
                if (_menuBarEdge != value)
                {
                    if (value == AppBarEdge.Left || value == AppBarEdge.Right)
                    {
                        // Vertical orientation is not supported
                        return;
                    }

                    if (EnableMenuBarAutoHide && TaskbarEdge == value && TaskbarMode == 2)
                    {
                        // We cannot have multiple auto-hide bars on the same screen edge
                        EnableMenuBarAutoHide = false;
                    }

                    SetEnum(ref _menuBarEdge, value);
                }
            }
        }

        private bool _showStacksRemovableDrives = false;
        public bool ShowStacksRemovableDrives
        {
            get => _showStacksRemovableDrives;
            set => Set(ref _showStacksRemovableDrives, value);
        }
        #endregion

        #region Advanced
        private string _timeFormat;
        public string TimeFormat
        {
            get
            {
                // if Null or Empty, get CurrentCulture
                if (string.IsNullOrEmpty(_timeFormat))
                {
                    _timeFormat = $"ddd {CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern}";
                }

                return _timeFormat;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Set(ref _timeFormat, $"ddd {CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern}");
                }
                else if (_timeFormat != value)
                {
                    Set(ref _timeFormat, value);
                }
            }
        }

        private string _dateFormat = "D";
        public string DateFormat
        {
            get => _dateFormat;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Set(ref _dateFormat, "D");
                }
                else if (_dateFormat != value)
                {
                    Set(ref _dateFormat, value);
                }
            }
        }

        private string _fileManager = "%WINDIR%\\explorer.exe";
        public string FileManager
        {
            get => _fileManager;
            set => Set(ref _fileManager, value);
        }

        private bool _forceSoftwareRendering;
        public bool ForceSoftwareRendering
        {
            get => _forceSoftwareRendering;
            set => Set(ref _forceSoftwareRendering, value);
        }

        private int _autoHideShowDelayMs = 250;
        public int AutoHideShowDelayMs
        {
            get => _autoHideShowDelayMs;
            set => Set(ref _autoHideShowDelayMs, value);
        }
        
        private LogSeverity _logSeverity = LogSeverity.Info;
        public LogSeverity LogSeverity
        {
            get => _logSeverity;
            set => SetEnum(ref _logSeverity, value);
        }
        #endregion

        #endregion
    }
}
