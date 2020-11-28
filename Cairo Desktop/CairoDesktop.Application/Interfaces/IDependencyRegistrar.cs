namespace CairoDesktop.Application.Interfaces
{
    public interface IDependencyRegistrar
    {
        void AddScoped<TService>() where TService : class;

        void AddScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        void AddSingleton<TService>() where TService : class;

        void AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        void AddTransient<TService>() where TService : class;

        void AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;
    }
}