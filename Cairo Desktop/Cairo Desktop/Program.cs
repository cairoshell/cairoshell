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
using CairoDesktop.Common.ExtensionMethods;

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
                // Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(s => Settings.Instance);
                    services.AddSingleton<CairoApplication>();
                    services.AddSingleton<ICairoApplication>(x => x.GetRequiredService<CairoApplication>());

                    //services.AddScoped<ILog, CairoShellLoggerObserver>();
                    //services.AddScoped<ManagedShell.Common.Logging.ILog, ManagedShellLoggerObserver>();
                    //services.AddSingleton<LoggerLogAttacher>();

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
                    logging.AddInfrastructureLogging(options =>
                    {
                        var severity = Settings.Instance.GetLogSeverity(LogSeverity.Info);
                        options.LogLevel = severity.ToLogLevel();

                        var cairoApplicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cairo_Development_Team");
                        options.LogsFolderPath = Path.Combine(cairoApplicationDataFolder, "Logs");
                    });
                })
                .Build();

            var app = _host.Services.GetRequiredService<CairoApplication>(); ;
            app.InitializeComponent();
            var result = app.Run();

            return result;
        }

        private static Settings ImplementationFactory(IServiceProvider arg)
        {
            throw new NotImplementedException();
        }
    }
}