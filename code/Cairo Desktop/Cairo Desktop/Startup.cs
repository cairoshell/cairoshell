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

    /// <summary>
    /// Handles the startup of the application, including ensuring that only a single instance is running.
    /// </summary>
    public class Startup
    {
        private static System.Threading.Mutex cairoMutex;

        private static Window _parentWindow;

        private static Window _desktopWindow;

        public static MenuBar MenuBarWindow { get; set; }

        public static MenuBarShadow MenuBarShadowWindow { get; set; }

        public static Taskbar TaskbarWindow { get; set; }

        public static Desktop DesktopWindow { get; set; }

        public static Window DeskParent { get; set; }

        public static bool IsCairoUserShell;

        private static bool isRestart;

        private static string procName;

        // properties for startup
        private static string[] hklmStartupKeys = new string[] { "Software\\Microsoft\\Windows\\CurrentVersion\\Run", "Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run" };
        
        private static string[] hkcuStartupKeys = new string[] { "Software\\Microsoft\\Windows\\CurrentVersion\\Run", "Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce" };

        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "/restart")
                isRestart = true;
            else
                isRestart = false;

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

            // Show a splash screen while WPF inits
            // not needed any more
            //SplashScreen splash = new SplashScreen("Resources/loadSplash.png");
            //splash.Show(false, true);

            #region some real shell code
            int hShellReadyEvent;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 5)
                hShellReadyEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, true, @"Global\msgina: ShellReadyEvent");
            else
                hShellReadyEvent = NativeMethods.OpenEvent(NativeMethods.EVENT_MODIFY_STATE, false, "msgina: ShellReadyEvent");

            if (hShellReadyEvent != 0)
            {
                NativeMethods.SetEvent(hShellReadyEvent);
                NativeMethods.CloseHandle(hShellReadyEvent);
            }
            #endregion

            // check if we are the current user's shell
            object userShell = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\WinLogon", false).GetValue("Shell");
            procName = Process.GetCurrentProcess().ProcessName;
            if (userShell != null)
                IsCairoUserShell = userShell.ToString().ToLower().Contains("cairodesktop");
            else
                IsCairoUserShell = false;

            // Before we do anything, check if settings need to be upgraded
            if (Settings.IsFirstRun == true)
                Settings.Upgrade();

            if (Settings.EnableTaskbar)
            {
                // hide the windows taskbar according to user prefs
                switch (Settings.WindowsTaskbarMode)
                {
                    case 0:
                        AppBarHelper.SetWinTaskbarPos(NativeMethods.SWP_HIDEWINDOW);
                        break;
                    case 1:
                        AppBarHelper.SetWinTaskbarState(AppBarHelper.WinTaskbarState.AutoHide);
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }
            }

            if (Settings.EnableDesktop)
            {
                // hide the windows desktop
                Shell.ToggleDesktopIcons(false);
            }

            _parentWindow = new Window();
            InitializeParentWindow(_parentWindow);

            _desktopWindow = new Window();
            InitializeParentWindow(_desktopWindow);
            DeskParent = _desktopWindow;

            App app = new App();
            app.InitializeComponent();

            // Set custom theme if selected
            string theme = Settings.CairoTheme;
            if (theme != "Default")
                if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + theme)) app.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(System.Xml.XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + theme)));
            
            // Set desktop work area for when Explorer isn't running
            if(IsCairoUserShell)
                AppBarHelper.SetWorkArea();

            MenuBarWindow = new MenuBar() { Owner = _parentWindow };
            app.MainWindow = MenuBarWindow;
            MenuBarWindow.Show();

            if (Settings.EnableDesktop)
            {
                DesktopWindow = new Desktop() { Owner = _desktopWindow };
                DesktopWindow.Show();
            }

            if (Settings.EnableMenuBarShadow)
            {
                MenuBarShadowWindow = new MenuBarShadow() { Owner = _desktopWindow };
                MenuBarShadowWindow.Show();
            }

            if (Settings.EnableTaskbar)
            {
                TaskbarWindow = new Taskbar() { Owner = _parentWindow };
                TaskbarWindow.Show();
            }

#if (ENABLEFIRSTRUN)
            FirstRun();
#endif

            // Close the splash screen
            //splash.Close(new TimeSpan(0, 0, 0, 0, 800));

            // login items only necessary if Explorer didn't start them
            if (IsCairoUserShell && !isRestart)
            {
                RunStartupApps();
            }

            app.Run();
        }

        /// <summary>
        /// Checks that a single instance of the application is running, and if another is found it notifies the user and exits.
        /// </summary>
        /// <returns>Result of instance check.</returns>
        private static bool SingleInstanceCheck()
        {
            // get the list of all processes by that name
            Process[] processes = Process.GetProcessesByName(procName);

            // if there is more than one process...
            if (processes.Length > 1)
            {
                System.Threading.Thread.Sleep(1000);
                Process[] processes2 = Process.GetProcessesByName(procName);
                if (processes2.Length > 1)
                {
                    System.Threading.Thread.Sleep(3000);
                    Process[] processes3 = Process.GetProcessesByName(procName);
                    if (processes3.Length > 1)
                    {
                        CairoMessage.ShowAlert("If it's not responding, end it from Task Manager before trying to run Cairo again.", "Cairo is already running!", MessageBoxImage.Stop);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Executes the first run sequence.
        /// </summary>
        private static void FirstRun()
        {
            try
            {
                if (Settings.IsFirstRun == true)
                {
                    Settings.IsFirstRun = false;
                    AppGrabber.AppGrabber.Instance.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                CairoMessage.ShowAlert(string.Format("Whoops! Something bad happened in the startup process.\nCairo will probably run, but please report the following details (preferably as a screen shot...)\n\n{0}", ex), 
                    "Unexpected error!", 
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Initializes a new hidden toolwindow to be the owner for all other windows.
        /// This hides the applications icons from the task switcher.
        /// </summary>
        private static void InitializeParentWindow(Window _parentWindow)
        {
            
            _parentWindow.Top = -100; // Location of new window is outside of visible part of screen
            _parentWindow.Left = -100;
            _parentWindow.Width = 1; // size of window is enough small to avoid its appearance at the beginning
            _parentWindow.Height = 1;
            _parentWindow.WindowStyle = WindowStyle.ToolWindow; // Set window style as ToolWindow to avoid its icon in AltTab 
            _parentWindow.ShowInTaskbar = false;
            _parentWindow.Show(); // We need to show window before set is as owner to our main window
            _parentWindow.Hide(); // Hide helper window just in case
        }

        #region Autorun

        private static List<string> FetchStartupApps()
        {
            List<string> startupApps = new List<string>();

            // HKLM Run
            foreach(string keyString in hklmStartupKeys)
            {
                RegistryKey key;

                key = Registry.LocalMachine.OpenSubKey(keyString, false);

                if(key != null && key.ValueCount > 0)
                {
                    foreach(string valueName in key.GetValueNames())
                        startupApps.Add(((string)key.GetValue(valueName)).Replace("\"", ""));
                }

                if (key != null)
                    key.Close();
            }

            // HKCU Run
            foreach (string keyString in hkcuStartupKeys)
            {
                RegistryKey key;

                if (keyString.Contains("RunOnce"))
                    key = Registry.CurrentUser.OpenSubKey(keyString, true);
                else
                    key = Registry.CurrentUser.OpenSubKey(keyString, false);

                if (key != null && key.ValueCount > 0)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        startupApps.Add(((string)key.GetValue(valueName)).Replace("\"", ""));

                        // if this is a runonce key, remove the value after we grab it
                        if (keyString.Contains("RunOnce"))
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
            }

            // Common Start Menu - Startup folder
            SystemDirectory comStartupDir = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), Dispatcher.CurrentDispatcher);

            foreach(SystemFile startupFile in comStartupDir.Files)
            {
                startupApps.Add(startupFile.FullName);
            }

            // Start Menu - Startup folder
            SystemDirectory startupDir = new SystemDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Dispatcher.CurrentDispatcher);

            foreach (SystemFile startupFile in startupDir.Files)
            {
                startupApps.Add(startupFile.FullName);
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
