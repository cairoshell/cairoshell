using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure;
using CairoDesktop.Interop;
using CairoDesktop.SupportingClasses;
using CairoDesktop.WindowsTray;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace CairoDesktop
{
    /// <summary>
    /// Handles the startup of the application, including ensuring that only a single instance is running.
    /// </summary>
    public partial class Startup
    {
        private static System.Threading.Mutex cairoMutex;
        private static CommandLineParser commandLineParser;
        private static bool isRestart;
        private static bool isTour;
        private static bool forceEnableShellMode;
        private static bool forceDisableShellMode;
        internal static IHost _host;

        public static bool IsShuttingDown { get; set; }

        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            _host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddInfrastructureServices(context.Configuration);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddInfrastructureLogging();
                })
                .Build();

            #region Initialization Routines

            ProcessCommandLineArgs(args);

            if (!SingleInstanceCheck())
                return;

            SetShellReadyEvent();

            SetupSettings(); // run this before logging setup so that preferences are always used

            // Initialize current shell information here, since it won't be accurate if we wait until after we create our own windows
            SetIsCairoRunningAsShell();

            SetupLoggingSystem();

            WriteApplicationDebugInfoToConsole();

            SetSystemKeyboardShortcuts();

            SetupUpdateManager();

            // Move to App??? app.SetupPluginSystem();
            SetupPluginSystem(); // This will Load the Core Plugin and all other, will either reference it as a dependency or dont need it to be started first

            #endregion

            App app = new App();
            app.InitializeComponent();  // This sets up the Unhandled Exception stuff... 

            setTheme(app);

            // Future: This should be moved to whatever plugin is responsible for MenuBar stuff
            MenuBar initialMenuBar = new MenuBar((IApplicationUpdateService)_host.Services.GetService(typeof(IApplicationUpdateService)), System.Windows.Forms.Screen.PrimaryScreen);
            app.MainWindow = initialMenuBar;
            WindowManager.Instance.MenuBarWindows.Add(initialMenuBar);
            initialMenuBar.Show();

            // Future: This should be moved to whatever plugin is responsible for Taskbar stuff
            if (Settings.Instance.EnableTaskbar)
            {
                AppBarHelper.HideWindowsTaskbar();
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
                StartupRunner runner = new StartupRunner();
                runner.Run();
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
                CairoMessage.Show(string.Format("Whoops! Something bad happened in the startup process.\nCairo will probably run, but please report the following details (preferably as a screen shot...)\n\n{0}", ex),
                    "Unexpected error!",
                    CairoMessageImage.Error);
            }
        }

        public static void RestartCairo()
        {
            try
            {
                // run the program again
                Process current = new Process();
                current.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "CairoDesktop.exe";
                current.StartInfo.Arguments = "/restart";
                current.Start();

                // close this instance
                ExitCairo();
            }
            catch
            { }
        }

        public static void ExitCairo()
        {
            IsShuttingDown = true;

            if (Shell.IsCairoRunningAsShell)
            {
                indicateGracefulShutdown();
            }

            App.Current?.Dispatcher.Invoke(() => App.Current?.Shutdown(), DispatcherPriority.Normal);
        }

        private static void indicateGracefulShutdown()
        {
            // WinLogon will automatically launch the local machine shell if AutoRestartShell is enabled and the shell window process exits
            // setting the exit status to 1 indicates that we are shutting down gracefully and do not want the local machine shell to restart
            Environment.ExitCode = 1;
        }

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
