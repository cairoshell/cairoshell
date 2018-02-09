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

    /// <summary>
    /// Handles the startup of the application, including ensuring that only a single instance is running.
    /// </summary>
    public class Startup
    {
        private static System.Threading.Mutex cairoMutex;

        public static MenuBar MenuBarWindow { get; set; }

        public static MenuBarShadow MenuBarShadowWindow { get; set; }

        public static Taskbar TaskbarWindow { get; set; }

        public static Desktop DesktopWindow { get; set; }

        public static bool IsCairoUserShell;

        private static bool isRestart;

        public static bool IsShuttingDown { get; set; }

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

            // check if we are the current user's shell
            object userShell = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\WinLogon", false).GetValue("Shell");
            //procName = Process.GetCurrentProcess().ProcessName;
            if (userShell != null)
                IsCairoUserShell = userShell.ToString().ToLower().Contains("cairodesktop");
            else
                IsCairoUserShell = false;

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

            MenuBarWindow = new MenuBar();
            app.MainWindow = MenuBarWindow;
            MenuBarWindow.Show();

            if (Settings.EnableDesktop)
            {
                DesktopWindow = new Desktop();
                DesktopWindow.Show();
            }

            if (Settings.EnableMenuBarShadow)
            {
                MenuBarShadowWindow = new MenuBarShadow();
                MenuBarShadowWindow.Show();
            }

            if (Settings.EnableTaskbar)
            {
                TaskbarWindow = new Taskbar();
                TaskbarWindow.Show();
            }

            // Set desktop work area for when Explorer isn't running
            if (IsCairoUserShell)
                AppBarHelper.SetWorkArea();

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

            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown(), DispatcherPriority.Normal);
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
