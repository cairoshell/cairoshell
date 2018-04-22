namespace CairoDesktop
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using Microsoft.Win32;
    using Interop;
    using System.Collections.Generic;
    using System.Windows.Threading;
    using System.Windows.Markup;
    using System.Threading.Tasks;
    using CairoDesktop.Configuration;
    using SupportingClasses;
    using Common;
    using CairoDesktop.WindowsTray;
    using System.Threading;

    /// <summary>
    /// Handles the startup of the application, including ensuring that only a single instance is running.
    /// </summary>
    public partial class Startup
    {
        private static System.Threading.Mutex cairoMutex;

        public static MenuBar MenuBarWindow { get; set; }
        public static List<MenuBar> MenuBarWindows = new List<MenuBar>();

        public static MenuBarShadow MenuBarShadowWindow { get; set; }
        public static List<MenuBarShadow> MenuBarShadowWindows = new List<MenuBarShadow>();

        public static Taskbar TaskbarWindow { get; set; }
        public static List<Taskbar> TaskbarWindows = new List<Taskbar>();

        public static Desktop DesktopWindow { get; set; }

        public static bool IsCairoUserShell;

        private static bool isRestart;

        private static bool isTour;

        public static bool IsShuttingDown { get; set; }

        public static bool IsSettingScreens { get; set; }

        private static Object screenSetupLock = new Object();

        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            #region Args

            if (args.Length > 0 && args[0] == "/restart")
                isRestart = true;
            else
                isRestart = false;

            if (args.Length > 0 && args[0] == "/tour")
                isTour = true;
            else
                isTour = false;

            #endregion

            #region Single Instance Check
            bool ok;
            cairoMutex = new System.Threading.Mutex(true, "CairoShell", out ok);

            if (!ok && !isRestart)
            {
                // Another instance is already running.
                return;
            }
            else if (!ok && isRestart)
            {
                // this is a restart so let's wait for the old instance to end
                System.Threading.Thread.Sleep(2000);
            }
            #endregion

            #region some real shell code
            int hShellReadyEvent;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Shell.IsWindows2kOrBetter)
                hShellReadyEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, true, @"Global\msgina: ShellReadyEvent");
            else
                hShellReadyEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, "msgina: ShellReadyEvent");

            if (hShellReadyEvent != 0)
            {
                NativeMethods.SetEvent(hShellReadyEvent);
                NativeMethods.CloseHandle(hShellReadyEvent);
            }
            #endregion

           // an attempt to relaunch explorer it the application gets closed. doesnt work if VS terminates execution
           // AppDomain.CurrentDomain.ProcessExit += (o, e) => Shutdown();

            #region InitializationRoutines
            
            SetupLoggingSystem();
            WriteApplicationDebugInfoToConsole();

            SetupPluginSystem();

            #endregion

            // check if we are the current user's shell
            // set here as well so that we don't behave differently once user changes setting
            IsCairoUserShell = Shell.IsCairoUserShell;

            // Before we do anything, check if settings need to be upgraded
            if (Settings.IsFirstRun == true)
                Settings.Upgrade();

            if (Settings.EnableDesktop)
            {
                // hide the windows desktop
                Shell.ToggleDesktopIcons(false);
            }
            
            App app = new App();
            app.InitializeComponent();

            // Set custom theme if selected
            string theme = Settings.CairoTheme;
            if (theme != "Default")
                if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + theme)) app.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + theme)));

            if (Settings.EnableTaskbar)
            {
                // hide Windows taskbar
                AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.AutoHide);
                AppBarHelper.SetWinTaskbarPos((int)NativeMethods.SetWindowPosFlags.SWP_HIDEWINDOW);
            }

            MenuBarWindow = new MenuBar(System.Windows.Forms.Screen.PrimaryScreen);
            app.MainWindow = MenuBarWindow;
            MenuBarWindow.Show();
            MenuBarWindows.Add(MenuBarWindow);

            if (Settings.EnableDesktop)
            {
                DesktopWindow = new Desktop();
                DesktopWindow.Show();
            }

            if (Settings.EnableMenuBarShadow)
            {
                MenuBarShadowWindow = new MenuBarShadow(MenuBarWindow, System.Windows.Forms.Screen.PrimaryScreen);
                MenuBarShadowWindow.Show();
                MenuBarShadowWindows.Add(MenuBarShadowWindow);
            }

            if (Settings.EnableTaskbar)
            {
                TaskbarWindow = new Taskbar(System.Windows.Forms.Screen.PrimaryScreen);
                TaskbarWindow.Show();
                TaskbarWindows.Add(TaskbarWindow);
            }
            
            if (Settings.EnableMenuBarMultiMon || Settings.EnableTaskbarMultiMon)
                ScreenSetup(true);
            else if (IsCairoUserShell) // Set desktop work area for when Explorer isn't running
                AppBarHelper.SetWorkArea(System.Windows.Forms.Screen.PrimaryScreen);

            // initialize system tray if enabled
            if (Settings.EnableSysTray == true)
            {
                NotificationArea.Instance.Initialize();
            }

#if (ENABLEFIRSTRUN)
            FirstRun();
#endif
            
            // login items only necessary if Explorer didn't start them
            if (IsCairoUserShell && !isRestart)
            {
                RunStartupApps();
            }

            app.Run();
        }

        public static void ResetScreenCache()
        {
            // use reflection to empty screens cache
            typeof(System.Windows.Forms.Screen).GetField("screens", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).SetValue(null, null);
        }

        /// <summary>
        /// Compares the system screen list to the screens associated with Cairo windows, then creates or destroys windows as necessary.
        /// Only affects non-primary screens, as Cairo always opens on at least the primary screen.
        /// Runs at startup and when a WM_DISPLAYCHANGE message is received by the main MenuBar window.
        /// </summary>
        public static void ScreenSetup(bool skipChecks = false)
        {
            lock (screenSetupLock)
            {
                IsSettingScreens = true;

                List<string> sysScreens = new List<string>();
                List<string> openScreens = new List<string>();
                List<string> addedScreens = new List<string>();
                List<string> removedScreens = new List<string>();

                ResetScreenCache();

                if (!skipChecks)
                {
                    // enumerate screens

                    if (Settings.EnableMenuBarMultiMon || !Settings.EnableTaskbar)
                    {
                        foreach (MenuBar bar in MenuBarWindows)
                        {
                            if (bar.Screen != null)
                                openScreens.Add(bar.Screen.DeviceName);
                        }
                    }
                    else if (Settings.EnableTaskbarMultiMon)
                    {
                        foreach (Taskbar bar in TaskbarWindows)
                        {
                            if (bar.Screen != null)
                                openScreens.Add(bar.Screen.DeviceName);
                        }
                    }
                    else
                        return;

                    foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                    {
                        Trace.WriteLine(string.Format("{0} found at {1} with area {2}; primary? {3}", screen.DeviceName, screen.Bounds.ToString(), screen.WorkingArea.ToString(), screen.Primary.ToString()));

                        sysScreens.Add(screen.DeviceName);
                    }

                    // figure out which screens have been added vs removed

                    foreach (string name in sysScreens)
                    {
                        if (!openScreens.Contains(name))
                            addedScreens.Add(name);
                    }

                    foreach (string name in openScreens)
                    {
                        if (!sysScreens.Contains(name))
                            removedScreens.Add(name);
                    }

                    if (removedScreens.Count == sysScreens.Count)
                    {
                        // remove everything?! no way!
                        return;
                    }

                    // close windows associated with removed screens
                    foreach (string name in removedScreens)
                    {
                        // close taskbars
                        Taskbar taskbarToClose = null;
                        foreach (Taskbar bar in TaskbarWindows)
                        {
                            if (bar.Screen != null && bar.Screen.DeviceName == name)
                            {
                                taskbarToClose = bar;
                                break;
                            }
                        }

                        if (taskbarToClose != null)
                        {
                            taskbarToClose.Close();
                            TaskbarWindows.Remove(taskbarToClose);
                        }

                        // close menu bars
                        MenuBar barToClose = null;
                        foreach (MenuBar bar in MenuBarWindows)
                        {
                            if (bar.Screen != null && bar.Screen.DeviceName == name)
                            {
                                barToClose = bar;
                                break;
                            }
                        }

                        if (barToClose != null)
                        {
                            if (!barToClose.IsClosing)
                                barToClose.Close();
                            MenuBarWindows.Remove(barToClose);
                        }

                        // close menu bar shadows
                        MenuBarShadow barShadowToClose = null;
                        foreach (MenuBarShadow bar in MenuBarShadowWindows)
                        {
                            if (bar.Screen != null && bar.Screen.DeviceName == name)
                            {
                                barShadowToClose = bar;
                                break;
                            }
                        }

                        if (barShadowToClose != null)
                        {
                            if (!barShadowToClose.IsClosing)
                                barShadowToClose.Close();
                            MenuBarShadowWindows.Remove(barShadowToClose);
                        }
                    }

                    // update screens of stale windows
                    foreach (MenuBar bar in MenuBarWindows)
                    {
                        if (bar.Screen != null)
                        {
                            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
                            {
                                if (screen.DeviceName == bar.Screen.DeviceName)
                                {
                                    bar.Screen = screen;
                                    break;
                                }
                            }
                        }
                    }

                    foreach (MenuBarShadow bar in MenuBarShadowWindows)
                    {
                        if (bar.Screen != null)
                        {
                            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
                            {
                                if (screen.DeviceName == bar.Screen.DeviceName)
                                {
                                    bar.Screen = screen;
                                    break;
                                }
                            }
                        }
                    }

                    foreach (Taskbar bar in TaskbarWindows)
                    {
                        if (bar.Screen != null)
                        {
                            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
                            {
                                if (screen.DeviceName == bar.Screen.DeviceName)
                                {
                                    bar.Screen = screen;
                                    bar.setPosition();
                                    break;
                                }
                            }
                        }
                    }
                }

                // open windows on newly added screens
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    if ((skipChecks && !screen.Primary) || addedScreens.Contains(screen.DeviceName))
                    {
                        if (Settings.EnableMenuBarMultiMon)
                        {
                            // menu bars
                            MenuBar newMenuBar = new MenuBar(screen);
                            newMenuBar.Show();
                            MenuBarWindows.Add(newMenuBar);

                            if (Settings.EnableMenuBarShadow)
                            {
                                // menu bar shadows
                                MenuBarShadow newMenuBarShadow = new MenuBarShadow(newMenuBar, screen);
                                newMenuBarShadow.Show();
                                MenuBarShadowWindows.Add(newMenuBarShadow);
                            }
                        }

                        if (Settings.EnableTaskbarMultiMon && Settings.EnableTaskbar)
                        {
                            // taskbars
                            Taskbar newTaskbar = new Taskbar(screen);
                            newTaskbar.Show();
                            TaskbarWindows.Add(newTaskbar);
                        }
                    }

                    // Set desktop work area for when Explorer isn't running
                    if (IsCairoUserShell)
                        AppBarHelper.SetWorkArea(screen);
                }

                IsSettingScreens = false;
            }
        }

        /// <summary>
        /// Executes the first run sequence.
        /// </summary>
        private static void FirstRun()
        {
            try
            {
                if (Settings.IsFirstRun == true || isTour)
                {
                    Welcome welcome = new Welcome();
                    welcome.Show();
                }
            }
            catch (Exception ex)
            {
                CairoMessage.ShowAlert(string.Format("Whoops! Something bad happened in the startup process.\nCairo will probably run, but please report the following details (preferably as a screen shot...)\n\n{0}", ex), 
                    "Unexpected error!", 
                    MessageBoxImage.Error);
            }
        }

        public static void Restart()
        {
            try
            {
                // run the program again
                Process current = new Process();
                current.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "CairoDesktop.exe";
                current.StartInfo.Arguments = "/restart";
                current.Start();

                // close this instance
                Shutdown();
            }
            catch
            { }
        }

        public static void Shutdown()
        {
            if (IsCairoUserShell)
            {
                Shell.StartProcess("explorer.exe");
            }

            IsShuttingDown = true;

            Application.Current?.Dispatcher.Invoke(() => Application.Current?.Shutdown(), DispatcherPriority.Normal);
        }

        #region Autorun

        private static List<string> FetchStartupApps()
        {
            List<string> startupApps = new List<string>();

            // Registry startup keys
            Dictionary<string, string> startupKeys = new Dictionary<string, string>()
            {
                { "Software\\Microsoft\\Windows\\CurrentVersion\\Run", "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run" },
                { "Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run32" },
                { "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce", "" }
            };

            // loop twice, once for HKLM once for HKCU
            for (int i = 0; i <= 1; i++)
            {
                foreach (KeyValuePair<string, string> regKey in startupKeys)
                {
                    bool isRunOnce = regKey.Key.Contains("RunOnce");

                    // key is the reg key with the app path, value is the startupapproved key telling us if the item is disabled
                    RegistryKey root = null;
                    RegistryKey key = null;
                    RegistryKey approvedKey = null;

                    try
                    {
                        if (i == 0)
                            root = Registry.LocalMachine;
                        else
                            root = Registry.CurrentUser;

                        if (isRunOnce && i != 0)
                            key = root.OpenSubKey(regKey.Key, true);
                        else if (isRunOnce)
                            continue; // skip processing HKLM RunOnce because we can't remove things from there
                        else
                        {
                            key = root.OpenSubKey(regKey.Key, false);
                            approvedKey = root.OpenSubKey(regKey.Value, false);
                        }
                    }
                    catch { continue; } // in case of unable to load registry key
                    
                    if (key != null && key.ValueCount > 0)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            bool canRun = true;

                            if (approvedKey != null)
                            {
                                foreach (string approvedName in approvedKey.GetValueNames())
                                {
                                    try
                                    {
                                        string s = ((byte[])approvedKey.GetValue(approvedName))[0].ToString();
                                        if (approvedName == valueName && ((byte[])approvedKey.GetValue(approvedName))[0] % 2 != 0) // if value is odd number, item is disabled
                                        {
                                            canRun = false;
                                            break;
                                        }
                                        else if (approvedName == valueName)
                                            break;
                                    }
                                    catch { } // in case of invalid registry key values
                                }
                            }

                            if (canRun)
                                startupApps.Add(((string)key.GetValue(valueName)).Replace("\"", ""));

                            // if this is a runonce key, remove the value after we grab it
                            if (isRunOnce)
                            {
                                try
                                {
                                    key.DeleteValue(valueName);
                                }
                                catch { }
                            }
                        }
                    }

                    if (key != null)
                        key.Close();

                    if (approvedKey != null)
                        approvedKey.Close();
                }
            }

            // startup folders
            Dictionary<SystemDirectory, RegistryKey> startupFolderKeys = new Dictionary<SystemDirectory, RegistryKey>()
            {
                { new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), Dispatcher.CurrentDispatcher), Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\StartupFolder", false) },
                { new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Dispatcher.CurrentDispatcher), Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\StartupFolder", false) }
            };
            
            foreach (KeyValuePair<SystemDirectory, RegistryKey> startupFolder in startupFolderKeys)
            {
                foreach (SystemFile startupFile in startupFolder.Key.Files)
                {
                    bool canRun = true;

                    if (startupFolder.Value != null)
                    {
                        foreach (string approvedName in startupFolder.Value.GetValueNames())
                        {
                            try
                            {
                                string s = ((byte[])startupFolder.Value.GetValue(approvedName))[0].ToString();
                                if (approvedName == startupFile.Name && ((byte[])startupFolder.Value.GetValue(approvedName))[0] % 2 != 0) // if value is odd number, item is disabled
                                {
                                    canRun = false;
                                    break;
                                }
                                else if (approvedName == startupFile.Name)
                                    break;
                            }
                            catch { } // in case of invalid registry key values
                        }
                    }

                    if (canRun)
                        startupApps.Add(startupFile.FullName);
                }
            }

            return startupApps;
        }

        private static string[] expandArgs(string startupPath)
        {
            string[] procInfo = new string[2];

            int exeIndex = startupPath.IndexOf(".exe");

            if(exeIndex > 0)
            {
                // we may have args for an executable
                if (exeIndex + 4 != startupPath.Length)
                {
                    // argh, args!
                    procInfo[0] = startupPath.Substring(0, exeIndex + 4);
                    procInfo[1] = startupPath.Substring(exeIndex + 5, startupPath.Length - exeIndex - 5);
                }
                else
                {
                    procInfo[0] = startupPath;
                }
            }
            else
            {
                // no args to parse out
                procInfo[0] = startupPath;
            }

            return procInfo;
        }

        private async static void RunStartupApps()
        {
            await Task.Run(() => LoopStartupApps());
        }

        private static void LoopStartupApps()
        {
            foreach (string app in FetchStartupApps())
            {
                string[] procInfo = expandArgs(app);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.FileName = procInfo[0];
                startInfo.Arguments = procInfo[1];

                Debug.WriteLine("Starting program: " + startInfo.FileName);

                try
                {
                    Process.Start(startInfo);
                }
                catch { }
            }
        }

        #endregion
    }
}
