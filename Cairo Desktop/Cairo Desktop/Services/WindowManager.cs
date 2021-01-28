using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using CairoDesktop.Infrastructure.Services;
using CairoDesktop.SupportingClasses;
using ManagedShell.AppBar;
using ManagedShell.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace CairoDesktop.Services
{
    public sealed class WindowManager : IDisposable
    {
        private bool _isSettingDisplays;
        private bool hasCompletedInitialDisplaySetup;
        private int pendingDisplayEvents;
        private readonly static object displaySetupLock = new object();
        private readonly List<IWindowService> _windowServices = new List<IWindowService>();
        private readonly AppBarManager _appBarManager;
        private readonly ILogger<WindowManager> _logger;

        public bool IsSettingDisplays
        {
            get => _isSettingDisplays;
            set
            {
                if (value != _isSettingDisplays)
                {
                    _isSettingDisplays = value;

                    // when setting displays, flip AllowClose to true on AppBars so they will close if their screen goes away
                    if (_isSettingDisplays)
                    {
                        _appBarManager.SignalGracefulShutdown();
                    }
                    else
                    {
                        foreach (AppBarWindow window in _appBarManager.AppBars)
                        {
                            window.AllowClose = false;
                        }
                    }
                }
            }
        }

        public List<AppBarScreen> ScreenState = new List<AppBarScreen>();
        
        public EventHandler<WindowManagerEventArgs> DwmChanged;
        public EventHandler<WindowManagerEventArgs> ScreensChanged;

        public static System.Drawing.Size PrimaryMonitorSize
        {
            get
            {
                return new System.Drawing.Size(Convert.ToInt32(SystemParameters.PrimaryScreenWidth / DpiHelper.DpiScaleAdjustment), Convert.ToInt32(SystemParameters.PrimaryScreenHeight / DpiHelper.DpiScaleAdjustment));
            }
        }

        public static System.Drawing.Size PrimaryMonitorWorkArea
        {
            get
            {
                return new System.Drawing.Size(SystemInformation.WorkingArea.Right - SystemInformation.WorkingArea.Left, SystemInformation.WorkingArea.Bottom - SystemInformation.WorkingArea.Top);
            }
        }

        public WindowManager(ILogger<WindowManager> logger, ShellManagerService shellManagerService, DesktopManager desktopManager)
        {
            _appBarManager = shellManagerService.ShellManager.AppBarManager;
            _logger = logger;
            desktopManager.Initialize(this);

            // start a timer to handle orphaned display events
            DispatcherTimer notificationCheckTimer = new DispatcherTimer();

            notificationCheckTimer = new DispatcherTimer(DispatcherPriority.Background, System.Windows.Application.Current.Dispatcher)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };

            notificationCheckTimer.Tick += NotificationCheck_Tick;
            notificationCheckTimer.Start();
        }

        private void NotificationCheck_Tick(object sender, EventArgs e)
        {
            if (!IsSettingDisplays && pendingDisplayEvents > 0)
            {
                _logger.LogDebug("Processing additional display events");
                ProcessDisplayChanges(ScreenSetupReason.Reconciliation);
            }
        }
        
        public void RegisterWindowService(IWindowService service)
        {
            _windowServices.Add(service);
        }

        public void InitialSetup()
        {
            IsSettingDisplays = true;

            DisplaySetup(ScreenSetupReason.FirstRun);

            hasCompletedInitialDisplaySetup = true;
            IsSettingDisplays = false;
        }

        public void NotifyDisplayChange(ScreenSetupReason reason)
        {
            _logger.LogDebug($"Received {reason} notification");

            if (reason == ScreenSetupReason.DwmChange)
            {
                lock (displaySetupLock)
                {
                    DwmChanged?.Invoke(this, new WindowManagerEventArgs());
                }
            }
            else
            {
                pendingDisplayEvents++;

                if (!IsSettingDisplays)
                {
                    ProcessDisplayChanges(reason);
                }
            }
        }

        private bool HaveDisplaysChanged()
        {
            ResetScreenCache();

            if (ScreenState.Count == Screen.AllScreens.Length)
            {
                bool same = true;
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    Screen current = Screen.AllScreens[i];
                    if (!(ScreenState[i].Bounds == current.Bounds && ScreenState[i].DeviceName == current.DeviceName && ScreenState[i].Primary == current.Primary && ScreenState[i].WorkingArea == current.WorkingArea))
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                {
                    _logger.LogDebug("No display changes");
                    return false;
                }
            }

            return true;
        }

        private void ProcessDisplayChanges(ScreenSetupReason reason)
        {
            lock (displaySetupLock)
            {
                IsSettingDisplays = true;

                while (pendingDisplayEvents > 0)
                {
                    if (HaveDisplaysChanged())
                    {
                        DisplaySetup(reason);
                    }
                    else
                    {
                        // if this is only a DPI change, screens will be the same but we still need to reposition
                        RefreshWindows(reason, false);
                    }

                    pendingDisplayEvents--;
                }

                IsSettingDisplays = false;
            }
        }

        private void ResetScreenCache()
        {
            // use reflection to empty screens cache
            typeof(Screen).GetField("screens", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, null);
        }

        /// <summary>
        /// Compares the system screen list to the screens associated with Cairo windows, then creates or destroys windows as necessary.
        /// Runs at startup and when a WM_DEVICECHANGE, WM_DISPLAYCHANGE, or WM_DPICHANGED message is received by the MenuBar window on the primary display.
        /// </summary>
        private void DisplaySetup(ScreenSetupReason reason)
        {
            if (reason != ScreenSetupReason.FirstRun && !hasCompletedInitialDisplaySetup)
            {
                _logger.LogDebug("Display setup ran before startup completed, aborting");
                return;
            }

            _logger.LogDebug("Beginning display setup");

            List<string> sysScreens = new List<string>();
            List<string> openScreens = new List<string>();
            List<string> addedScreens = new List<string>();
            List<string> removedScreens = new List<string>();

            // update our knowledge of the displays present
            ScreenState = AppBarScreen.FromAllScreens();

            // enumerate screens based on currently open windows
            openScreens = GetOpenScreens();

            sysScreens = GetScreenDeviceNames();

            // figure out which screens have been added
            foreach (string name in sysScreens)
            {
                if (!openScreens.Contains(name))
                {
                    addedScreens.Add(name);
                }
            }

            if (reason != ScreenSetupReason.FirstRun)
            {
                // figure out which screens have been removed
                foreach (string name in openScreens)
                {
                    if (!sysScreens.Contains(name))
                    {
                        removedScreens.Add(name);
                    }
                }

                // abort if we have no screens or if we are removing all screens without adding new ones
                if (sysScreens.Count == 0 || (removedScreens.Count >= openScreens.Count && addedScreens.Count == 0))
                {
                    _logger.LogDebug("Aborted due to no displays present");
                    return;
                }

                // close windows associated with removed screens
                ProcessRemovedScreens(removedScreens);

                // refresh existing window screen properties with updated screen information
                RefreshWindows(reason, true);
            }

            // open windows on newly added screens
            ProcessAddedScreens(addedScreens);

            _logger.LogDebug("Completed display setup");
        }

        #region Display setup helpers
        private List<string> GetOpenScreens()
        {
            List<string> openScreens = new List<string>();

            foreach (var windowService in _windowServices)
            {
                foreach (var window in windowService.Windows)
                {
                    if (window.Screen != null && !openScreens.Contains(window.Screen.DeviceName))
                    {
                        openScreens.Add(window.Screen.DeviceName);
                    }
                }
            }

            return openScreens;
        }

        private List<string> GetScreenDeviceNames()
        {
            List<string> sysScreens = new List<string>();

            foreach (var screen in ScreenState)
            {
                _logger.LogDebug($"{screen.DeviceName} found at {screen.Bounds} with area {screen.WorkingArea}; primary? {screen.Primary}");
                sysScreens.Add(screen.DeviceName);
            }

            return sysScreens;
        }

        private void ProcessRemovedScreens(List<string> removedScreens)
        {
            foreach (string name in removedScreens)
            {
                _logger.LogDebug("Removing windows associated with screen " + name);

                foreach (var windowService in _windowServices)
                {
                    windowService.HandleScreenRemoved(name);
                }
            }
        }

        private void RefreshWindows(ScreenSetupReason reason, bool displaysChanged)
        {
            _logger.LogDebug("Refreshing screen information for existing windows");

            WindowManagerEventArgs args = new WindowManagerEventArgs { DisplaysChanged = displaysChanged, Reason = reason };

            foreach (var windowService in _windowServices)
            {
                windowService.RefreshWindows(args);
            }

            ScreensChanged?.Invoke(this, args);
        }

        private void ProcessAddedScreens(List<string> addedScreens)
        {
            foreach (var screen in ScreenState)
            {
                if (addedScreens.Contains(screen.DeviceName))
                {
                    _logger.LogDebug("Opening windows on screen " + screen.DeviceName);

                    foreach (var windowService in _windowServices)
                    {
                        windowService.HandleScreenAdded(screen);
                    }
                }
            }
        }

        public static T GetScreenWindow<T>(List<T> windowList, AppBarScreen screen) where T : AppBarWindow
        {
            foreach (AppBarWindow window in windowList)
            {
                if (window.Screen?.DeviceName == screen.DeviceName)
                {
                    return (T)window;
                }
            }

            return null;
        }
        #endregion

        public void Dispose()
        {
        }
    }
}