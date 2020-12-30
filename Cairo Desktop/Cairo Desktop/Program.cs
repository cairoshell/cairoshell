using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common.Logging;
using CairoDesktop.Common.Logging.Legacy;
using CairoDesktop.Common.Logging.Observers;
using CairoDesktop.Configuration;
using CairoDesktop.Infrastructure.DependencyInjection;
using CairoDesktop.SupportingClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            _host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<CairoApplication>();
                    services.AddSingleton<ICairoApplication>(x => x.GetRequiredService<CairoApplication>());

                    services.AddScoped<ILog, CairoShellLoggerObserver>();
                    services.AddScoped<ManagedShell.Common.Logging.ILog, ManagedShellLoggerObserver>();
                    services.AddSingleton<LoggerLogAttacher>();

                    services.AddSingleton<ShellManagerService>();

                    services.AddSingleton<DesktopManager>();
                    services.AddSingleton<WindowManager>();
                    services.AddSingleton<IWindowService, MenuBarWindowService>();
                    services.AddSingleton<IWindowService, TaskbarWindowService>();

                    services.AddInfrastructureServices(context.Configuration);

                    services.AddDependencyLoadingServices(context.Configuration, Path.Combine(CairoApplication.StartupPath, "Extensions")); // TODO: this should not be a property of CairoApplication... Possible solution, use Configuration?
                    services.AddDependencyLoadingServices(context.Configuration, Path.Combine(CairoApplication.CairoApplicationDataFolder, "Extensions"));// TODO: this should not be a property of CairoApplication... Possible solution, use Configuration?
                    
                    services.AddLogging();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddInfrastructureLogFile(options =>
                    {
                        var severity = GetLogSeverityFromCairoSettings(LogSeverity.Info);
                        options.LogLevel = severity.ToLogLevel();

                        var cairoApplicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cairo_Development_Team");
                        options.LogsFolderPath = Path.Combine(cairoApplicationDataFolder, "Logs");

                        // TODO: ManagedShell and CairoShell logging could be handled better
                        CairoLogger.Severity = severity;
                        ManagedShell.Common.Logging.ShellLogger.Severity = severity.ToManagedShellLogSeverity();
                    });
                })
                .Build();

            var app = _host.Services.GetRequiredService<CairoApplication>(); ;
            app.InitializeComponent();
            var result = app.Run();

            return result;
        }

        private static LogSeverity GetLogSeverityFromCairoSettings(LogSeverity defaultValue)
        {
            if (Enum.TryParse(Settings.Instance.LogSeverity, out LogSeverity result))
                return result;

            Settings.Instance.LogSeverity = defaultValue.ToString();
            result = defaultValue;

            return result;
        }
    }
}