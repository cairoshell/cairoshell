using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common.ExtensionMethods;
using CairoDesktop.Common.Logging;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using CairoDesktop.AppGrabber;
using CairoDesktop.MenuBarExtensions;
using CairoDesktop.Services;
using Microsoft.Extensions.Configuration;
using CairoDesktop.Infrastructure.Options;

namespace CairoDesktop
{
    internal sealed class Program
    {
        private const string MutexName = "CairoShell";
        private const int MutexAttempts = 10;
        private const int MutexWaitMs = 1000;

        private static IHost _host;
        private static System.Threading.Mutex _cairoMutex;

        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            if (!SingleInstanceCheck())
            {
                return 1;
            }
            
            _host = new HostBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<CommandLineOptions>(context.Configuration);

                    services.AddSingleton(s => Settings.Instance);
                    services.AddSingleton<ICairoApplication, CairoApplication>();

                    services.AddSingleton<AppGrabberService>();
                    services.AddSingleton<ISettingsUIService, SettingsUIService>();
                    services.AddHostedService<ShellHotKeyService>();
                    services.AddSingleton<ThemeService>();

                    services.AddSingleton<DesktopManager>();
                    services.AddSingleton<WindowManager>();
                    services.AddSingleton<IWindowService, MenuBarWindowService>();
                    services.AddSingleton<IWindowService, TaskbarWindowService>();

#if ENABLEFIRSTRUN
                    services.AddHostedService<FirstRunService>();
#endif

                    services.AddInfrastructureServices(context.Configuration);

                    var extensionPaths = new[]
                    {
                        Path.Combine(CairoApplication.StartupPath, "Extensions"),
                        Path.Combine(CairoApplication.CairoApplicationDataFolder, "Extensions")
                    };

                    services.AddDependencyLoadingServices(context.Configuration, extensionPaths); // TODO: this should not be a property of CairoApplication... Possible solution, use Configuration?

                    // Inbox extensions below
                    services.AddSingleton<IShellExtension, MenuBarExtensionsShellExtension>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                    
                    logging.AddManagedShellLogging(options =>
                    {
                        var severity = Settings.Instance.GetLogSeverity(LogSeverity.Info);
                        options.LogLevel = severity.ToLogLevel();
                        options.LogsFolderPath = CairoApplication.LogsFolder;
                    });
                })
                .Build();
            
            var app = _host.Services.GetRequiredService<ICairoApplication>();
            return app.Run();
        }
        
        private static bool GetMutex()
        {
            _cairoMutex = new System.Threading.Mutex(true, MutexName, out bool ok);

            return ok;
        }

        private static bool SingleInstanceCheck()
        {
            for (int i = 0; i < MutexAttempts; i++)
            {
                if (!GetMutex())
                {
                    // Dispose the mutex, otherwise it will never create new
                    _cairoMutex.Dispose();
                    System.Threading.Thread.Sleep(MutexWaitMs);
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}