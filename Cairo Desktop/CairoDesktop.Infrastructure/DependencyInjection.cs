using CairoDesktop.Application.Interfaces;
using CairoDesktop.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace CairoDesktop.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IApplicationUpdateService, WinSparkleApplicationUpdateService>();


            return services;
        }

        public static ILoggingBuilder AddInfrastructureLogging(this ILoggingBuilder builder, Action<Common.Logging.Loggers.CairoLogger> configure = null)
        {
            builder.Services.AddSingleton<ILoggerProvider, Common.Logging.Providers.CairoLoggerProvider>();

            if (configure != null)
            {
                builder.Services.Configure(configure);
            }

            return builder;
        }
    }
}