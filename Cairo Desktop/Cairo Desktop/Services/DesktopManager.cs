using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.SupportingClasses;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.Services;
using ManagedShell;
using ManagedShell.AppBar;
using ManagedShell.Common.Helpers;
using ManagedShell.Common.SupportingClasses;
using ManagedShell.ShellFolders;
using Microsoft.Extensions.Logging;
using NativeMethods = ManagedShell.Interop.NativeMethods;

namespace CairoDesktop.Services
{
    public class DesktopManager : INotifyPropertyChanged, IDisposable
    {
        public Desktop DesktopWindow { get; private set; }

        public DesktopOverlay DesktopOverlayWindow { get; private set; }

        public DesktopNavigationToolbar DesktopToolbar { get; private set; }

        public ShellWindow ShellWindow { get; private set; }

        public static bool IsEnabled => Settings.Instance.EnableDesktop && !GroupPolicyHelper.NoDesktop;

        private bool _isOverlayOpen;
        private bool _isOverlayClosing;
        private int _renderOverlayFrames;
        private HotKey _overlayHotKey;
        private WindowManager _windowManager;
        private readonly ShellManager _shellManager;
        private readonly ICairoApplication _cairoApplication;
        private readonly ILogger<DesktopManager> _logger;
        private readonly ISettingsUIService _settingsUiService;

        public DesktopIcons DesktopIconsControl { get; private set; }

        public NavigationManager NavigationManager { get; private set; }

        public bool IsOverlayOpen
        {
            get => _isOverlayOpen;
            set
            {
                if (_isOverlayOpen == value || _isOverlayClosing)
                    return;

                _isOverlayOpen = value;

                if (value)
                {
                    OpenOverlay();
                }
                else
                {
                    CloseOverlay();
                }

                OnPropertyChanged();
            }
        }

        public bool AllowProgmanChild => ShellWindow == null && EnvironmentHelper.IsWindows8OrBetter; // doesn't work in win7 due to layered child window restrictions

        public ShellFolder DesktopLocation => DesktopIconsControl?.Location;

        public DesktopManager(ILogger<DesktopManager> logger, ICairoApplication cairoApplication, ShellManagerService shellManagerService, ISettingsUIService settingsUiService)
        {
            // DesktopManager is always created on startup by WindowManager, regardless of desktop preferences
            // this allows for dynamic creation and destruction of the desktop per user preference
            _cairoApplication = cairoApplication;
            _shellManager = shellManagerService.ShellManager;
            _logger = logger;
            _settingsUiService = settingsUiService;
            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        public void Initialize(WindowManager manager)
        {
            _windowManager = manager;
            _windowManager.DwmChanged += DwmChanged;
            _windowManager.ScreensChanged += ScreensChanged;

            InitDesktop();
        }

        private void InitDesktop()
        {
            if (!IsEnabled && !EnvironmentHelper.IsAppRunningAsShell)
                return;

            // hide the windows desktop
            ShellHelper.ToggleDesktopIcons(false);

            CreateShellWindow();
            CreateDesktopBrowser();
            CreateDesktopWindow();
        }

        private void CreateDesktopBrowser()
        {
            if (!IsEnabled)
                return;

            if (NavigationManager == null)
            {
                NavigationManager = new NavigationManager();
                NavigationManager.PropertyChanged += NavigationManager_PropertyChanged;
            }

            if (DesktopIconsControl == null)
            {
                DesktopIconsControl = new DesktopIcons(this);
            }

            RegisterHotKey();

            ConfigureDesktopBrowser();
        }

        private void CreateDesktopWindow()
        {
            if (DesktopWindow != null)
                return;

            DesktopWindow = new Desktop(this, _shellManager.AppBarManager, _shellManager.FullScreenHelper, _settingsUiService);
            DesktopWindow.WorkAreaChanged += WorkAreaChanged;
            DesktopWindow.Show();
        }

        private void CreateToolbar()
        {
            if (Settings.Instance.EnableDynamicDesktop && DesktopWindow != null && NavigationManager != null)
            {
                DesktopToolbar = new DesktopNavigationToolbar(_cairoApplication, this) { Owner = DesktopWindow, NavigationManager = NavigationManager };
                DesktopToolbar.Show();
            }
            else if (Settings.Instance.EnableDynamicDesktop && DesktopWindow == null) // NavigationManager can be expected null
            {
                _logger.LogWarning("Attempted to create ToolBar with uninitialized properties");
            }
        }

        private void DestroyDesktopBrowser()
        {
            DesktopOverlayWindow?.Close();
            DesktopOverlayWindow = null;
            DestroyToolbar();
            NavigationManager = null;

            // remove desktop icons control
            DesktopWindow?.grid.Children.Clear();

            DesktopIconsControl = null;

            UnregisterHotKey();
        }

        private void DestroyDesktopWindow()
        {
            if (DesktopWindow == null)
                return;

            if (DesktopToolbar != null)
            {
                DesktopToolbar.Owner = null;
            }

            DesktopWindow.grid.Children.Clear();
            DesktopWindow.WorkAreaChanged -= WorkAreaChanged;

            DesktopWindow.AllowClose = true;
            DesktopWindow.Close();

            DesktopWindow = null;
        }

        private void DestroyToolbar()
        {
            if (DesktopToolbar == null)
                return;

            DesktopToolbar.AllowClose = true;
            DesktopToolbar.Close();
            DesktopToolbar = null;
        }

        private void TeardownDesktop()
        {
            DestroyDesktopBrowser();
            DestroyDesktopWindow();
            DestroyShellWindow();

            // show the windows desktop
            ShellHelper.ToggleDesktopIcons(true);
        }

        private void RegisterHotKey()
        {
            if (IsEnabled && Settings.Instance.EnableDesktopOverlayHotKey && _overlayHotKey == null)
            {
                _overlayHotKey = HotKeyManager.RegisterHotKey(Settings.Instance.DesktopOverlayHotKey, OnShowDesktop);
            }
        }

        private void UnregisterHotKey()
        {
            _overlayHotKey?.Unregister();
            _overlayHotKey?.Dispose();
            _overlayHotKey = null;
        }

        public void ConfigureDesktop()
        {
            if (DesktopWindow != null)
            {
                if (ShellWindow != null || AllowProgmanChild)
                {
                    // set the desktop window as a child of the shell window
                    NativeMethods.SetWindowLong(DesktopWindow.Handle, NativeMethods.GWL_STYLE, (NativeMethods.GetWindowLong(DesktopWindow.Handle, NativeMethods.GWL_STYLE) | (int)NativeMethods.WindowStyles.WS_CHILD) & ~unchecked((int)NativeMethods.WindowStyles.WS_OVERLAPPED));
                    NativeMethods.SetParent(DesktopWindow.Handle, ShellWindow?.Handle ?? WindowHelper.GetLowestDesktopChildHwnd());
                }

                ConfigureDesktopBrowser();
            }
            else
            {
                _logger.LogWarning("Attempted to configure desktop with uninitialized values");
            }
        }

        private void ConfigureDesktopBrowser()
        {
            if (DesktopWindow == null)
                return;

            if (DesktopIconsControl != null && DesktopOverlayWindow == null)
            {
                DesktopWindow.grid.Children.Add(DesktopIconsControl);
            }

            if (DesktopToolbar == null)
            {
                CreateToolbar();
            }
            else
            {
                DesktopToolbar.Owner = DesktopWindow;
            }
        }

        public void ResetPosition(bool displayChanged)
        {
            ShellWindow?.SetSize();

            DesktopOverlayWindow?.ResetPosition();

            if (displayChanged && DesktopWindow != null)
            {
                DestroyToolbar();
                DestroyDesktopWindow();
                CreateDesktopWindow();
            }
            else
            {
                DesktopWindow?.ResetPosition();
            }
        }

        #region Shell window
        private void CreateShellWindow()
        {
            if (!EnvironmentHelper.IsAppRunningAsShell || ShellWindow != null)
                return;

            // create native shell window; we must pass a native window's handle to SetShellWindow
            ShellWindow = new ShellWindow();
            ShellWindow.WallpaperChanged += WallpaperChanged;
            ShellWindow.WorkAreaChanged += WorkAreaChanged;

            if (ShellWindow.IsShellWindow)
            {
                // we did it
                _logger.LogDebug("Successfully set as shell window");
            }
        }

        private void DestroyShellWindow()
        {
            if (ShellWindow == null)
                return;

            ShellWindow.WallpaperChanged -= WallpaperChanged;
            ShellWindow.WorkAreaChanged -= WorkAreaChanged;
            ShellWindow?.Dispose();
            ShellWindow = null;
        }
        #endregion

        #region Overlay
        private void OpenOverlay()
        {
            if (DesktopOverlayWindow == null && DesktopWindow != null && DesktopIconsControl != null)
            {
                DesktopOverlayWindow = new DesktopOverlay(this, _shellManager.AppBarManager);

                // create mask image to show while the icons control is rendered on the overlay window
                Image maskImage = new Image
                {
                    Source = DesktopIconsControl?.GenerateBitmap(DesktopWindow.grid)
                };

                // add the mask image to and show the overlay
                DesktopOverlayWindow.grid.Children.Add(maskImage);
                DesktopOverlayWindow.Show();
                DesktopOverlayWindow.BringToFront();

                // migrate the desktop icons control
                DesktopWindow.grid.Children.Clear();
                DesktopOverlayWindow.grid.Children.Add(DesktopIconsControl);

                // remove the mask image
                DesktopOverlayWindow.grid.Children.Remove(maskImage);

                // change ToolBar owner
                if (DesktopToolbar == null)
                    return;

                DesktopToolbar.Owner = DesktopOverlayWindow;
                DesktopToolbar.BringToFront();
            }
            else if (DesktopOverlayWindow != null)
            {
                _logger.LogDebug("Desktop overlay already open, ignoring");
            }
            else
            {
                _logger.LogWarning("Attempted to show desktop overlay using uninitialized properties");
            }
        }

        private void CloseOverlay()
        {
            if (DesktopOverlayWindow != null && DesktopWindow != null && DesktopIconsControl != null)
            {
                _isOverlayClosing = true;

                // create mask image to show while the icons control is rendered on the desktop window
                Image maskImage = new Image();
                maskImage.Source = DesktopIconsControl?.GenerateBitmap(DesktopOverlayWindow.grid);

                // add the mask image to the desktop
                DesktopWindow.grid.Children.Add(maskImage);

                // change ToolBar owner
                if (DesktopToolbar != null && NativeMethods.IsWindow(DesktopWindow.Handle))
                {
                    DesktopToolbar.Owner = DesktopWindow;
                    DesktopToolbar.SendToBottom();
                }

                // setup render callback to hide overlay and continue once image is rendered
                _renderOverlayFrames = 0;
                CompositionTarget.Rendering += CloseOverlay_CompositionTarget_Rendering;
            }
            else if (DesktopOverlayWindow == null)
            {
                _logger.LogDebug("Desktop overlay already closed, ignoring");
            }
            else
            {
                _logger.LogWarning("Attempted to close desktop overlay using uninitialized properties");
            }
        }

        public void ToggleOverlay()
        {
            IsOverlayOpen = !IsOverlayOpen;
        }

        private void CloseOverlay_CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // runs once per frame during overlay close

            // it generally takes 2 frames to render the mask image
            const int waitFrames = 2;

            if (_renderOverlayFrames == waitFrames)
            {
                // close the overlay window
                DesktopOverlayWindow.Close();

                // migrate the icons control to the desktop window
                DesktopOverlayWindow.grid.Children.Clear();
                DesktopWindow.grid.Children.Add(DesktopIconsControl);

                // once the control has been migrated, remove the mask image
                DesktopWindow.grid.Children.RemoveAt(0);

                // remove reference to the overlay window to allow GC
                DesktopOverlayWindow = null;

                // we're done here, stop this callback from executing again
                CompositionTarget.Rendering -= CloseOverlay_CompositionTarget_Rendering;

                _isOverlayClosing = false;
            }

            _renderOverlayFrames++;
        }
        #endregion

        #region Event handling
        private void WallpaperChanged(object sender, EventArgs e)
        {
            DesktopWindow?.ReloadBackground();
        }

        private void WorkAreaChanged(object sender, EventArgs e)
        {
            ResetPosition(false);
        }

        private void NavigationManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CurrentPath")
                return;

            if (DesktopIconsControl.Location != null &&
                DesktopIconsControl.Location.Path == NavigationManager.CurrentItem.Path)
                return;

            // dispose of current directory so that we don't keep a lock on it
            ShellFolder previousFolder = null;
            if (DesktopIconsControl.Location != null)
            {
                previousFolder = DesktopIconsControl.Location;
            }

            DesktopIconsControl.SetIconLocation();

            try
            {
                previousFolder?.Dispose();
            }
            catch (Exception exception)
            {
                _logger.LogWarning($"Exception during ShellFolder disposal: {exception.Message}");
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.PropertyName)) return;
            switch (e.PropertyName)
            {
                case "EnableDesktop":
                    if (EnvironmentHelper.IsAppRunningAsShell)
                    {
                        if (Settings.Instance.EnableDesktop)
                        {
                            CreateDesktopBrowser();
                        }
                        else
                        {
                            DestroyDesktopBrowser();
                        }
                    }
                    else
                    {
                        if (Settings.Instance.EnableDesktop)
                        {
                            InitDesktop();
                        }
                        else
                        {
                            TeardownDesktop();
                        }
                    }

                    break;
                case "EnableDynamicDesktop" when IsEnabled:
                    if (Settings.Instance.EnableDynamicDesktop)
                    {
                        CreateToolbar();
                    }
                    else
                    {
                        DestroyToolbar();
                        NavigationManager?.NavigateHome();
                    }

                    break;
                case "EnableDesktopOverlayHotKey" when IsEnabled:
                    if (Settings.Instance.EnableDesktopOverlayHotKey)
                    {
                        RegisterHotKey();
                    }
                    else
                    {
                        UnregisterHotKey();
                    }

                    break;
                case "DesktopOverlayHotKey" when IsEnabled:
                    if (Settings.Instance.EnableDesktopOverlayHotKey)
                    {
                        UnregisterHotKey();
                        RegisterHotKey();
                    }

                    break;
            }
        }

        private void ScreensChanged(object sender, WindowManagerEventArgs e)
        {
            if (e.Reason == ScreenSetupReason.DpiChange)
            {
                // treat dpi change as display change
                ResetPosition(true);
            }
            else
            {
                ResetPosition(e.DisplaysChanged);
            }
        }

        private void DwmChanged(object sender, WindowManagerEventArgs e)
        {
            if (DesktopWindow == null || !AllowProgmanChild)
                return;

            // When we are a child of progman, we need to handle redrawing when DWM restarts
            DestroyDesktopWindow();
            CreateDesktopWindow();
        }

        private void OnShowDesktop(HotKey hotKey)
        {
            ToggleOverlay();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void Dispose()
        {
            if (_windowManager != null)
            {
                _windowManager.DwmChanged -= DwmChanged;
                _windowManager.ScreensChanged -= ScreensChanged;
            }

            TeardownDesktop();
        }
    }
}