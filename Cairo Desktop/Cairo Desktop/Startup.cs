namespace CairoDesktop
{
    using CairoDesktop.Common.Helpers;
    using CairoDesktop.Common.Logging;
    using CairoDesktop.Configuration;
    using CairoDesktop.WindowsTray;
    using Common;
    using Interop;
    using Microsoft.Win32;
    using SupportingClasses;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// Handles the startup of the application, including ensuring that only a single instance is running.
    /// </summary>
    public partial class Startup
    {
        private static System.Threading.Mutex cairoMutex;
        private static CommandLineParser commandLineParser;
        private static bool isRestart;
        private static bool isTour;

        public static bool IsShuttingDown { get; set; }

        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            #region Initialization Routines

            ProcessCommandLineArgs(args);
            if(!SingleInstanceCheck()) return;
            SetShellReadyEvent();

            SetupSettings(); // run this before logging setup so that preferences are always used

            // Initialize current shell information here, since it won't be accurate if we wait until after we create our own Shell_TrayWnd
            Shell.SetIsCairoRunningAsShell();

            SetupLoggingSystem();
            WriteApplicationDebugInfoToConsole();

            SetSystemKeyboardShortcuts();

            // Move to App??? app.SetupPluginSystem();
            SetupPluginSystem(); // This will Load the Core Plugin and all other, will either reference it as a dependency or dont need it to be started first

            #endregion

            if (Settings.Instance.EnableDesktop && !GroupPolicyManager.Instance.NoDesktop) // Future: This should be moved to whatever plugin is responsible for desktop stuff
            {
                // hide the windows desktop
                Shell.ToggleDesktopIcons(false);
            }

            App app = new App();
            app.InitializeComponent();  // This sets up the Unhandled Exception stuff... 

            setTheme(app);


            // Future: This should be moved to whatever plugin is responsible for Taskbar stuff
            if (Settings.Instance.EnableTaskbar)
            {
                AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.AutoHide);
                AppBarHelper.SetWinTaskbarVisibility((int)NativeMethods.SetWindowPosFlags.SWP_HIDEWINDOW);
            }

            // Future: This should be moved to whatever plugin is responsible for MenuBar stuff
            MenuBar initialMenuBar = new MenuBar(System.Windows.Forms.Screen.PrimaryScreen);
            app.MainWindow = initialMenuBar;
            WindowManager.Instance.MenuBarWindows.Add(initialMenuBar);
            initialMenuBar.Show();

            // Future: This should be moved to whatever plugin is responsible for Desktop stuff
            if (Settings.Instance.EnableDesktop && !GroupPolicyManager.Instance.NoDesktop)
            {
                WindowManager.Instance.DesktopWindow = new Desktop();
                WindowManager.Instance.DesktopWindow.Show();
            }

            // Future: This should be moved to whatever plugin is responsible for Taskbar stuff
            if (Settings.Instance.EnableTaskbar)
            {
                Taskbar initialTaskbar = new Taskbar(System.Windows.Forms.Screen.PrimaryScreen);
                WindowManager.Instance.TaskbarWindows.Add(initialTaskbar);
                initialTaskbar.Show();
            }

            // Open windows on secondary displays and set work area
            WindowManager.Instance.InitialSetup();

            // Future: This should be moved to whatever plugin is responsible for SystemTray stuff. Possibly Core with no UI, then have a plugin that gives the UI?
            // Don't allow showing both the Windows taskbar and the Cairo tray
            if (Settings.Instance.EnableSysTray && (Settings.Instance.EnableTaskbar || Shell.IsCairoRunningAsShell))
            {
                NotificationArea.Instance.Initialize();
            }

#if ENABLEFIRSTRUN
            FirstRun();
#endif

#if !DEBUG
            // login items only necessary if Explorer didn't start them
            if (Shell.IsCairoRunningAsShell && !isRestart)
            {
                RunStartupApps();
            }
#endif

            app.Run();
        }

        /// <summary>
        /// Executes the first run sequence.
        /// </summary>
        private static void FirstRun()
        {
            try
            {
                if (Settings.Instance.IsFirstRun || isTour)
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
            IsShuttingDown = true;

            // dispose notification area in case we started it earlier
            NotificationArea.Instance.Dispose();

            // reset work area
            if (Shell.IsCairoRunningAsShell) 
                WindowManager.Instance.ResetWorkArea();

            // dispose of long-lived COM objects
            Shell.DisposeIml();

            Application.Current?.Dispatcher.Invoke(() => Application.Current?.Shutdown(), DispatcherPriority.Normal);
        }

#region Shell: Autorun Apps

        private static async void RunStartupApps()
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

                CairoLogger.Instance.Debug("Starting program: " + startInfo.FileName);

                try
                {
                    Process.Start(startInfo);
                }
                catch
                {
                    CairoLogger.Instance.Info("Failed to start program: " + startInfo.FileName);
                }
            }
        }

        private static List<string> FetchStartupApps()
        {
            List<string> startupApps = new List<string>();

            // Registry startup keys
            Dictionary<string, string> startupKeys = new Dictionary<string, string>()
            {
                {
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run"
                },
                {
                    "Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run",
                    "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run32"
                },
                {
                    "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
                    ""
                }
            };

            // TODO: foreach(RegistryKey root in new[] { Registry.LocalMachine ,Registry.CurrentUser}) ... Would this be a more readable solution ???
            // loop twice, once for HKLM once for HKCU
            for (int i = 0; i <= 1; i++)
            {
                foreach (KeyValuePair<string, string> regKey in startupKeys)
                {
                    bool isRunOnce = regKey.Key.Contains("RunOnce");

                    RegistryKey root = null; // HKLM or HKCU
                    RegistryKey key = null; // AppPath
                    RegistryKey approvedKey = null; // the startupapproved key tells us if the item is disabled

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
                    catch
                    {
                        continue; // in case of unable to load registry key
                    }

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
            Dictionary<string, RegistryKey> startupFolderKeys = new Dictionary<string, RegistryKey>()
            {
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup),
                    Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\StartupFolder", false)
                },
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\StartupFolder", false)
                }
              };

            foreach (KeyValuePair<string, RegistryKey> startupFolder in startupFolderKeys)
            {
                try
                {
                    if (Shell.Exists(startupFolder.Key))
                    {
                        SystemDirectory directory = new SystemDirectory(startupFolder.Key, Dispatcher.CurrentDispatcher);
                        foreach (SystemFile startupFile in directory.Files)
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

                        directory.Dispose();
                    }
                }
                catch
                {
                    CairoLogger.Instance.Info("Failed to run startup apps from location:" + startupFolder.Key);
                }
            }

            return startupApps;
        }

        private static string[] expandArgs(string startupPath)
        {
            string[] procInfo = new string[2];

            int exeIndex = startupPath.IndexOf(".exe");

            if (exeIndex > 0)
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



#endregion

        private static void setTheme(App app)
        {
            // Themes are very UI centric. We should devise a way of having Plugins/Extensions contribute to this.
            string theme = Settings.Instance.CairoTheme;
            if (theme != "Default")
            {
                string themeFilePath = AppDomain.CurrentDomain.BaseDirectory + theme;
                if (System.IO.File.Exists(themeFilePath))
                {
                    ResourceDictionary newRes = new ResourceDictionary();
                    newRes.Source = new Uri(themeFilePath, UriKind.RelativeOrAbsolute);
                    app.Resources.MergedDictionaries.Add(newRes);
                }
            }

            Settings.Instance.PropertyChanged += (s, e) =>
            {
                if (e != null && !string.IsNullOrWhiteSpace(e.PropertyName) && e.PropertyName == "CairoTheme")
                {
                    App.Current.Resources.MergedDictionaries.Clear();
                    ResourceDictionary cairoResource = new ResourceDictionary();

                    // Put our base theme back
                    cairoResource.Source = new Uri("Cairo.xaml", UriKind.RelativeOrAbsolute);
                    App.Current.Resources.MergedDictionaries.Add(cairoResource);

                    string newTheme = Settings.Instance.CairoTheme;
                    if (newTheme != "Default")
                    {
                        string newThemeFilePath = AppDomain.CurrentDomain.BaseDirectory + newTheme;
                        if (System.IO.File.Exists(newThemeFilePath))
                        {
                            ResourceDictionary newRes = new ResourceDictionary();
                            newRes.Source = new Uri(newThemeFilePath, UriKind.RelativeOrAbsolute);
                            app.Resources.MergedDictionaries.Add(newRes);
                        }
                    }
                }
            };
        }
    }
}
