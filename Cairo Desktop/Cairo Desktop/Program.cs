using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common.ExtensionMethods;
using CairoDesktop.Common.Logging;
using CairoDesktop.Common.Logging.Legacy;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.DependencyInjection;
using CairoDesktop.SupportingClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace CairoDesktop
{
    internal sealed class Program
    {
        private static IHost _host;

        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            _host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(s => Settings.Instance);
                    services.AddSingleton<ICairoApplication, CairoApplication>();

                    services.AddSingleton<ShellManagerService>();

                    services.AddSingleton<DesktopManager>();
                    services.AddSingleton<WindowManager>();

                    services.AddSingleton<IWindowService, MenuBarWindowService>();
                    services.AddSingleton<IWindowService, TaskbarWindowService>();

                    services.AddInfrastructureServices(context.Configuration);

                    var extensionPaths = new[]
                    {
                        Path.Combine(CairoApplication.StartupPath, "Extensions"),
                        Path.Combine(CairoApplication.CairoApplicationDataFolder, "Extensions")
                    };

                    services.AddDependencyLoadingServices(context.Configuration, extensionPaths); // TODO: this should not be a property of CairoApplication... Possible solution, use Configuration?
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                    
                    logging.AddManagedShellLogging(options =>
                    {
                        var severity = Settings.Instance.GetLogSeverity(LogSeverity.Info);
                        options.LogLevel = severity.ToLogLevel();

                        var cairoApplicationDataFolder =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "Cairo_Development_Team");
                        options.LogsFolderPath = Path.Combine(cairoApplicationDataFolder, "Logs");
                    });
                })
                .Build();
            
            var app = _host.Services.GetRequiredService<ICairoApplication>();
            return app.Run();
        }
    }
}