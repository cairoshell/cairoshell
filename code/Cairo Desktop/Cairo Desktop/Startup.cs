namespace CairoDesktop
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using Microsoft.Win32;
    using System.Windows.Interop;
    using Interop;

    /// <summary>
    /// Handles the startup of the application, including ensuring that only a single instance is running.
    /// </summary>
    public class Startup
    {
        private static System.Threading.Mutex cairoMutex;

        private static System.Windows.Window _parentWindow;

        private static System.Windows.Window _desktopWindow;

        public static MenuBar MenuBarWindow { get; set; }
        public static MenuBarShadow MenuBarShadowWindow { get; set; }
        public static Taskbar TaskbarWindow { get; set; }
        public static Desktop DesktopWindow { get; set; }

        public static Window DeskParent { get; set; }


        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        public static void Main()
        {
            #region Single Instance Check
            bool ok;
            cairoMutex = new System.Threading.Mutex(true, "CairoShell", out ok);

            if (!ok)
            {
                // Another instance is already running.
                return;
            }
            #endregion

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

            // Before we do anything, check if settings need to be upgraded
            if (Properties.Settings.Default.IsFirstRun == true)
                Properties.Settings.Default.Upgrade();

            _parentWindow = new Window();
            InitializeParentWindow(_parentWindow);

            _desktopWindow = new Window();
            InitializeParentWindow(_desktopWindow);
            DeskParent = _desktopWindow;

            App app = new App();

            MenuBarWindow = new MenuBar() { Owner = _parentWindow };
            MenuBarWindow.Show();
            app.MainWindow = MenuBarWindow;

            if (Properties.Settings.Default.EnableDesktop)
            {
                DesktopWindow = new Desktop() { Owner = _desktopWindow };
                DesktopWindow.Show();
            }

            if (Properties.Settings.Default.EnableMenuBarShadow)
            {
                MenuBarShadowWindow = new MenuBarShadow() { Owner = _desktopWindow };
                MenuBarShadowWindow.Show();
            }

            if (Properties.Settings.Default.EnableTaskbar)
            {
                TaskbarWindow = new Taskbar() { Owner = _parentWindow };
                TaskbarWindow.Show();
            }

#if (ENABLEFIRSTRUN)
            FirstRun(app);
#endif

            app.Run();
        }

        /// <summary>
        /// Checks that a single instance of the application is running, and if another is found it notifies the user and exits.
        /// </summary>
        /// <returns>Result of instance check.</returns>
        private static bool SingleInstanceCheck()
        {
            string proc = Process.GetCurrentProcess().ProcessName;

            // get the list of all processes by that name
            Process[] processes = Process.GetProcessesByName(proc);

            // if there is more than one process...
            if (processes.Length > 1)
            {
                System.Threading.Thread.Sleep(1000);
                Process[] processes2 = Process.GetProcessesByName(proc);
                if (processes2.Length > 1)
                {
                    System.Threading.Thread.Sleep(3000);
                    Process[] processes3 = Process.GetProcessesByName(proc);
                    if (processes3.Length > 1)
                    {
                        CairoMessage.Show("If it's not responding, end it from Task Manager before trying to run Cairo again.", "Cairo is already running!", MessageBoxButton.OK, MessageBoxImage.Stop);
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
        /// <param name="app">References to the app object.</param>
        private static void FirstRun(App app)
        {
            try
            {
                if (Properties.Settings.Default.IsFirstRun == true)
                {
                    Properties.Settings.Default.IsFirstRun = false;
                    Properties.Settings.Default.Save();
                    AppGrabber.AppGrabber.Instance.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                CairoMessage.Show(string.Format("Whoops! Something bad happened in the startup process.\nCairo will probably run, but please report the following details (preferably as a screen shot...)\n\n{0}", ex), 
                    "Unexpected error!", 
                    MessageBoxButton.OK, 
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

    }
}
