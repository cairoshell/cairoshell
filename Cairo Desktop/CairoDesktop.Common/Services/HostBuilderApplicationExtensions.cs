using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CairoDesktop.Common.Services
{
    public static class HostBuilderApplicationExtensions
    {
        private const string MutexBuilderKey = "MutexBuilder";

        /// <summary>
        /// Helper method to retrieve the mutex builder
        /// </summary>
        /// <param name="properties">IDictionary</param>
        /// <param name="mutexBuilder">IMutexBuilder out value</param>
        /// <returns>bool if there was a matcher</returns>
        private static bool TryRetrieveMutexBuilder(this IDictionary<object, object> properties, out IMutexBuilder mutexBuilder)
        {
            if (properties.TryGetValue(MutexBuilderKey, out var mutexBuilderObject))
            {
                mutexBuilder = (IMutexBuilder)mutexBuilderObject;
                return true;

            }
            mutexBuilder = new MutexBuilder();
            properties[MutexBuilderKey] = mutexBuilder;
            return false;
        }

        /// <summary>
        /// Prevent that an application runs multiple times
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="configureAction">Action to configure IMutexBuilder</param>
        /// <returns>IHostBuilder for fluently calling</returns>
        public static IHostBuilder ConfigureSingleInstance(this IHostBuilder hostBuilder, Action<IMutexBuilder> configureAction)
        {
            hostBuilder.ConfigureServices((hostContext, serviceCollection) =>
            {
                if (!TryRetrieveMutexBuilder(hostBuilder.Properties, out var mutexBuilder))
                {
                    serviceCollection
                        .AddSingleton(mutexBuilder)
                        .AddHostedService<MutexLifetimeService>();
                }
                configureAction?.Invoke(mutexBuilder);
            });
            return hostBuilder;
        }

        /// <summary>
        /// Prevent that an application runs multiple times
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="mutexId">string</param>
        /// <returns>IHostBuilder for fluently calling</returns>
        public static IHostBuilder ConfigureSingleInstance(this IHostBuilder hostBuilder, string mutexId)
        {
            return hostBuilder.ConfigureSingleInstance(builder => builder.MutexId = mutexId);
        }

    }
}