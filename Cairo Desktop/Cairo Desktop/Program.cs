﻿using CairoDesktop.AppGrabber;
using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common.Logging;
using CairoDesktop.Common;
using CairoDesktop.Infrastructure.DependencyInjection;
using CairoDesktop.Infrastructure.Options;
using CairoDesktop.MenuBarExtensions;
using CairoDesktop.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using CairoDesktop.Interfaces;

namespace CairoDesktop;

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
            .ConfigureAppConfiguration((context, builder) => { builder.AddCommandLine(args); })
            .ConfigureServices((context, services) =>
            {
                services.Configure<CommandLineOptions>(context.Configuration);

                services.AddSingleton<Settings>(s => Settings.Instance);

                services.AddSingleton<IInitializationService, CairoApplicationInitializationService>();

                services.AddSingleton<CairoApplication>();
                services.AddSingleton<ICairoApplication>(provider => provider.GetRequiredService<CairoApplication>());

                services.AddSingleton<IAppGrabber, AppGrabberService>();

                services.AddSingleton<ISettingsUIService, SettingsUIService>();

                services.AddHostedService<ShellHotKeyService>();

                services.AddSingleton<IThemeService, CairoApplicationThemeService>();

                services.AddSingleton<IDesktopManager, DesktopManager>();
                services.AddSingleton<IWindowManager, WindowManager>();
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

                services.AddDependencyLoadingServices(context.Configuration,
                    extensionPaths); // TODO: this should not be a property of CairoApplication... Possible solution, use Configuration?

                // Inbox extensions below
                services.AddSingleton<IShellExtension, MenuBarExtensionsShellExtension>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);

                logging.AddManagedShellLogging(options =>
                {
                    var severity = Settings.Instance.LogSeverity;
                    options.LogLevel = severity.ToLogLevel();
                    options.LogsFolderPath = CairoApplication.LogsFolder;
                });
            })
            .ConfigureHostOptions(options => { options.ShutdownTimeout = TimeSpan.FromSeconds(10); })
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