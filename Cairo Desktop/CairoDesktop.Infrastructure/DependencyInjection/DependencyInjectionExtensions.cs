using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using CairoDesktop.Common.Logging;
using CairoDesktop.Common.Logging.Legacy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CairoDesktop.Infrastructure.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IExtensionService, ExtensionService>();
            services.AddSingleton<IApplicationUpdateService, WinSparkleApplicationUpdateService>();

            return services;
        }

        public static IServiceCollection AddDependencyLoadingServices(this IServiceCollection services, IConfiguration configuration, string path, string pattern = null)
        {
            services.LoadDependencies(path, pattern);

            return services;
        }

        public static ILoggingBuilder AddInfrastructureLogFile(this ILoggingBuilder builder, Action<CairoFileLoggerOptions> configure = null)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CairoFileLoggerProvider>());
            builder.Services.Configure(configure);

            return builder;
        }
    }
}