using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using CairoDesktop.Common.DesignPatterns;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Interop;

namespace CairoDesktop.SupportingClasses
{
    public class WindowManager : SingletonObject<WindowManager>
    {
        #region Properties

        private bool hasCompletedInitialDisplaySetup = false;
        private int pendingDisplayEvents = 0;
        private readonly static object displaySetupLock = new object();

        public bool IsSettingDisplays { get; set; }
        public Screen[] ScreenState = { };
        public List<MenuBar> MenuBarWindows = new List<MenuBar>();
        public List<Taskbar> TaskbarWindows = new List<Taskbar>();

        public Desktop DesktopWindow { get; set; }

        public static System.Drawing.Size PrimaryMonitorSize
        {
            get
            {
                return new System.Drawing.Size(Convert.ToInt32(SystemParameters.PrimaryScreenWidth / Shell.DpiScaleAdjustment), Convert.ToInt32(SystemParameters.PrimaryScreenHeight / Shell.DpiScaleAdjustment));
            }
        }

        public static System.Drawing.Size PrimaryMonitorDeviceSize
        {
            get
            {
                return new System.Drawing.Size(NativeMethods.GetSystemMetrics(0), NativeMethods.GetSystemMetrics(1));
            }
        }

        public static System.Drawing.Size PrimaryMonitorWorkArea
        {
            get
            {
                return new System.Drawing.Size(SystemInformation.WorkingArea.Right - SystemInformation.WorkingArea.Left, SystemInformation.WorkingArea.Bottom - SystemInformation.WorkingArea.Top);
            }
        }

        #endregion

        private WindowManager()
        {
            // start a timer to handle orphaned display events
            DispatcherTimer notificationCheckTimer = new DispatcherTimer();

            notificationCheckTimer = new DispatcherTimer(DispatcherPriority.Background, System.Windows.Application.Current.Dispatcher);
            notificationCheckTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            notificationCheckTimer.Tick += NotificationCheck_Tick;
            notificationCheckTimer.Start();
        }

        private void NotificationCheck_Tick(object sender, EventArgs e)
        {
            if (!IsSettingDisplays && pendingDisplayEvents > 0)
            {
                CairoLogger.Instance.Debug("WindowManager: Processing additional display events");
                processDisplayChanges();
            }
        }

        public void InitialSetup()
        {
            IsSettingDisplays = true;

            displaySetup(true);

            hasCompletedInitialDisplaySetup = true;
            IsSettingDisplays = false;
        }

        public void NotifyDisplayChange()
        {
            pendingDisplayEvents++;

            if (!IsSettingDisplays) processDisplayChanges();
        }

        private bool haveDisplaysChanged()
        {
            resetScreenCache();

            if (ScreenState.Length == Screen.AllScreens.Length)
            {
                bool same = true;
                for (int i = 0; i < ScreenState.Length; i++)
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
                    CairoLogger.Instance.Debug("WindowManager: No display changes");
                    return false;
                }
            }

            return true;
        }

        private void processDisplayChanges()
        {
            lock (displaySetupLock)
            {
                IsSettingDisplays = true;

                while (pendingDisplayEvents > 0)
                {
                    if (haveDisplaysChanged())
                    {
                        displaySetup();
                    }
                    else
                    {
                        // if this is only a DPI change, screens will be the same but we still need to reposition
                        refreshWindows();
                        setDisplayWorkAreas();
                    }

                    pendingDisplayEvents--;
                }

                IsSettingDisplays = false;
            }
        }

        private void resetScreenCache()
        {
            // use reflection to empty screens cache
            typeof(Screen).GetField("screens", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, null);
        }

        /// <summary>
        /// Compares the system screen list to the screens associated with Cairo windows, then creates or destroys windows as necessary.
        /// Runs at startup and when a WM_DEVICECHANGE, WM_DISPLAYCHANGE, or WM_DPICHANGED message is received by the MenuBar window on the primary display.
        /// </summary>
        private void displaySetup(bool firstRun = false)
        {
            if (!firstRun && !hasCompletedInitialDisplaySetup)
            {
                CairoLogger.Instance.Debug("WindowManager: Display setup ran before startup completed, aborting");
                return;
            }

            CairoLogger.Instance.Debug("WindowManager: Beginning display setup");

            List<string> sysScreens = new List<string>();
            List<string> openScreens = new List<string>();
            List<string> addedScreens = new List<string>();
            List<string> removedScreens = new List<string>();

            // update our knowledge of the displays present
            ScreenState = Screen.AllScreens;

            if (!firstRun)
            {
                // enumerate screens based on currently open windows
                openScreens = getOpenScreens();

                sysScreens = getScreenDeviceNames();

                // figure out which screens have been added
                foreach (string name in sysScreens)
                {
                    if (!openScreens.Contains(name))
                        addedScreens.Add(name);
                }

                // figure out which screens have been removed
                foreach (string name in openScreens)
                {
                    if (!sysScreens.Contains(name))
                        removedScreens.Add(name);
                }

                // abort if we have no screens or if we are removing all screens without adding new ones
                if (sysScreens.Count == 0 || (removedScreens.Count >= openScreens.Count && addedScreens.Count == 0))
                {
                    CairoLogger.Instance.Debug("WindowManager: Aborted due to no displays present");
                    return;
                }

                // close windows associated with removed screens
                processRemovedScreens(removedScreens);

                // refresh existing window screen properties with updated screen information
                refreshWindows();
            }

            // open windows on newly added screens
            processAddedScreens(addedScreens, firstRun);

            // update each display's work area if we are shell
            setDisplayWorkAreas();

            CairoLogger.Instance.Debug("WindowManager: Completed display setup");
        }

        #region Display setup helpers
        private List<string> getOpenScreens()
        {
            List<string> openScreens = new List<string>();

            foreach (MenuBar bar in MenuBarWindows)
            {
                if (bar.Screen != null && !openScreens.Contains(bar.Screen.DeviceName))
                    openScreens.Add(bar.Screen.DeviceName);
            }

            foreach (Taskbar bar in TaskbarWindows)
            {
                if (bar.Screen != null && !openScreens.Contains(bar.Screen.DeviceName))
                    openScreens.Add(bar.Screen.DeviceName);
            }

            return openScreens;
        }

        private List<string> getScreenDeviceNames()
        {
            List<string> sysScreens = new List<string>();

            foreach (var screen in ScreenState)
            {
                CairoLogger.Instance.Debug(string.Format("WindowManager: {0} found at {1} with area {2}; primary? {3}", screen.DeviceName, screen.Bounds.ToString(), screen.WorkingArea.ToString(), screen.Primary.ToString()));
                sysScreens.Add(screen.DeviceName);
            }

            return sysScreens;
        }

        private void processRemovedScreens(List<string> removedScreens)
        {
            foreach (string name in removedScreens)
            {
                CairoLogger.Instance.DebugIf(Settings.Instance.EnableMenuBarMultiMon || Settings.Instance.EnableTaskbarMultiMon, "WindowManager: Removing windows associated with screen " + name);

                if (Settings.Instance.EnableTaskbarMultiMon && Settings.Instance.EnableTaskbar)
                {
                    // close taskbars
                    Taskbar taskbarToClose = null;
                    foreach (Taskbar bar in TaskbarWindows)
                    {
                        if (bar.Screen != null && bar.Screen.DeviceName == name)
                        {
                            CairoLogger.Instance.DebugIf(bar.Screen.Primary, "WindowManager: Closing taskbar on primary display");

                            taskbarToClose = bar;
                            break;
                        }
                    }

                    if (taskbarToClose != null)
                    {
                        if (!taskbarToClose.IsClosing) taskbarToClose.Close();
                        TaskbarWindows.Remove(taskbarToClose);
                    }
                }

                if (Settings.Instance.EnableMenuBarMultiMon)
                {
                    // close menu bars
                    MenuBar barToClose = null;
                    foreach (MenuBar bar in MenuBarWindows)
                    {
                        if (bar.Screen != null && bar.Screen.DeviceName == name)
                        {
                            CairoLogger.Instance.DebugIf(bar.Screen.Primary, "WindowManager: Closing menu bar on primary display");

                            barToClose = bar;
                            break;
                        }
                    }

                    if (barToClose != null)
                    {
                        if (!barToClose.IsClosing) barToClose.Close();
                        MenuBarWindows.Remove(barToClose);
                    }
                }
            }
        }

        private void refreshWindows()
        {
            CairoLogger.Instance.Debug("WindowManager: Refreshing screen information for existing windows");

            // update screens of stale windows
            if (Settings.Instance.EnableMenuBarMultiMon)
            {
                foreach (Screen screen in ScreenState)
                {
                    MenuBar bar = GetScreenWindow(MenuBarWindows, screen);

                    if (bar != null)
                    {
                        bar.Screen = screen;
                        bar.setScreenPosition();
                    }
                }
            }
            else if (MenuBarWindows.Count > 0)
            {
                MenuBarWindows[0].Screen = Screen.PrimaryScreen;
                MenuBarWindows[0].setScreenPosition();
            }

            if (Settings.Instance.EnableTaskbarMultiMon)
            {
                foreach (Screen screen in ScreenState)
                {
                    Taskbar bar = GetScreenWindow(TaskbarWindows, screen);

                    if (bar != null)
                    {
                        bar.Screen = screen;
                        bar.setScreenPosition();
                    }
                }
            }
            else if (TaskbarWindows.Count > 0)
            {
                TaskbarWindows[0].Screen = Screen.PrimaryScreen;
                TaskbarWindows[0].setScreenPosition();
            }
        }

        private void processAddedScreens(List<string> addedScreens, bool firstRun)
        {
            foreach (var screen in ScreenState)
            {
                // if firstRun, that means this is initial startup and primary display windows have already opened, so skip them. addedScreens will
                if ((firstRun && !screen.Primary) || addedScreens.Contains(screen.DeviceName))
                {
                    CairoLogger.Instance.DebugIf(Settings.Instance.EnableMenuBarMultiMon || Settings.Instance.EnableTaskbarMultiMon, "WindowManager: Opening windows on screen " + screen.DeviceName);

                    if (Settings.Instance.EnableMenuBarMultiMon)
                    {
                        CairoLogger.Instance.DebugIf(screen.Primary, "WindowManager: Opening menu bar on new primary display");

                        // menu bars
                        MenuBar newMenuBar = new MenuBar(screen);
                        newMenuBar.Show();
                        MenuBarWindows.Add(newMenuBar);
                    }

                    if (Settings.Instance.EnableTaskbarMultiMon && Settings.Instance.EnableTaskbar)
                    {
                        CairoLogger.Instance.DebugIf(screen.Primary, "WindowManager: Opening taskbar on new primary display");

                        // taskbars
                        Taskbar newTaskbar = new Taskbar(screen);
                        newTaskbar.Show();
                        TaskbarWindows.Add(newTaskbar);
                    }
                }
            }
        }

        public T GetScreenWindow<T>(List<T> windowList, Screen screen) where T : AppBarWindow
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

        #region Work area
        private void setDisplayWorkAreas()
        {
            // Set desktop work area for when Explorer isn't running
            if (Shell.IsCairoRunningAsShell)
            {
                foreach (var screen in ScreenState)
                {
                    SetWorkArea(screen);
                }
            }
        }

        public void SetWorkArea(Screen screen)
        {
            double dpiScale = 1;
            double menuBarHeight = 0;
            double taskbarHeight = 0;
            NativeMethods.Rect rc;
            rc.Left = screen.Bounds.Left;
            rc.Right = screen.Bounds.Right;

            // get appropriate windows for this display
            foreach (MenuBar bar in MenuBarWindows)
            {
                if (bar.Screen.DeviceName == screen.DeviceName)
                {
                    menuBarHeight = bar.ActualHeight;
                    dpiScale = bar.dpiScale;
                    break;
                }
            }

            foreach (Taskbar bar in TaskbarWindows)
            {
                if (bar.Screen.DeviceName == screen.DeviceName)
                {
                    taskbarHeight = bar.ActualHeight;
                    break;
                }
            }

            // only allocate space for taskbar if enabled
            if (Settings.Instance.EnableTaskbar && Settings.Instance.TaskbarMode == 0)
            {
                if (Settings.Instance.TaskbarPosition == 1)
                {
                    rc.Top = screen.Bounds.Top + (int)(menuBarHeight * dpiScale) + (int)(taskbarHeight * dpiScale);
                    rc.Bottom = screen.Bounds.Bottom;
                }
                else
                {
                    rc.Top = screen.Bounds.Top + (int)(menuBarHeight * dpiScale);
                    rc.Bottom = screen.Bounds.Bottom - (int)(taskbarHeight * dpiScale);
                }
            }
            else
            {
                rc.Top = screen.Bounds.Top + (int)(menuBarHeight * dpiScale);
                rc.Bottom = screen.Bounds.Bottom;
            }

            NativeMethods.SystemParametersInfo((int)NativeMethods.SPI.SETWORKAREA, 0, ref rc, (1 | 2));
        }

        public void ResetWorkArea()
        {
            // TODO this is wrong for multi-display
            // set work area back to full screen size. we can't assume what pieces of the old workarea may or may not be still used
            NativeMethods.Rect oldWorkArea;
            oldWorkArea.Left = SystemInformation.VirtualScreen.Left;
            oldWorkArea.Top = SystemInformation.VirtualScreen.Top;
            oldWorkArea.Right = SystemInformation.VirtualScreen.Right;
            oldWorkArea.Bottom = SystemInformation.VirtualScreen.Bottom;

            NativeMethods.SystemParametersInfo((int)NativeMethods.SPI.SETWORKAREA, 0, ref oldWorkArea, (1 | 2));
        }
        #endregion
    }
}
