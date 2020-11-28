using CairoDesktop.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CairoDesktop.Infrastructure.DependencyInjection
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private readonly IServiceCollection _serviceCollection;

        public DependencyRegistrar(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        void IDependencyRegistrar.AddScoped<TService>()
        {
            _serviceCollection.AddScoped<TService>();
        }

        void IDependencyRegistrar.AddScoped<TService, TImplementation>()
        {
            _serviceCollection.AddScoped<TService, TImplementation>();
        }

        void IDependencyRegistrar.AddSingleton<TService>()
        {
            _serviceCollection.AddSingleton<TService>();
        }

        void IDependencyRegistrar.AddSingleton<TService, TImplementation>()
        {
            _serviceCollection.AddSingleton<TService, TImplementation>();
        }

        void IDependencyRegistrar.AddTransient<TService>()
        {
            _serviceCollection.AddTransient<TService>();
        }

        void IDependencyRegistrar.AddTransient<TService, TImplementation>()
        {
            _serviceCollection.AddTransient<TService, TImplementation>();
        }
    }
}