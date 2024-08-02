using CairoDesktop.Application.Interfaces;
using CairoDesktop.Common.Logging.Other;
using CairoDesktop.Infrastructure.ObjectModel;
using CairoDesktop.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace CairoDesktop.Infrastructure.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IExtensionService, ExtensionService>();
            services.AddSingleton<IApplicationUpdateService, WinSparkleApplicationUpdateService>();
            services.AddSingleton<ShellManagerService>();
            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<IWindowManager, WindowManager>();

            return services;
        }

        public static IServiceCollection AddDependencyLoadingServices(this IServiceCollection services, IConfiguration configuration, string path, string pattern = null)
        {
            services.LoadDependencies(path, pattern);

            return services;
        }

        public static IServiceCollection AddDependencyLoadingServices(this IServiceCollection services, IConfiguration configuration, string[] paths, string pattern = null)
        {
            int numLoaded = 0;

            foreach (string path in paths)
            {
                // The first path is always the inbox extensions path. If extensions are disabled, we want to skip loading all other paths.
                if (numLoaded > 0 && configuration["NoExtensions"] == "true")
                {
                    continue;
                }

                services.AddDependencyLoadingServices(configuration, path, pattern);
                numLoaded++;
            }

            return services;
        }

        public static ILoggingBuilder AddManagedShellLogging(this ILoggingBuilder builder, Action<ManagedShellFileLoggerOptions> configure = null)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ManagedShellLoggerProvider>());
            builder.Services.Configure(configure);

            return builder;
        }
    }
}