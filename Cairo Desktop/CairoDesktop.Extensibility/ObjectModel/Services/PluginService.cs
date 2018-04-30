using CairoDesktop.Extensibility.Plugins;
using CairoDesktop.Extensibility.Providers;
using CairoDesktop.Extensibility.Providers.Custom;
using System.Collections.Generic;

namespace CairoDesktop.Extensibility.ObjectModel.Services
{
    public sealed class PluginService : ShellService
    {
        PluginContext pluginContext;

        public List<Plugin> Plugins { get; private set; }

        public PluginService()
        {
            Plugins = new List<Plugin>();
        }

        public override void Start()
        {
            pluginContext = new PluginContext();
            var fileSystemPluginProvider = new FileSystemPluginProvider(nameof(FileSystemPluginProvider));
            var pluginProviders = new PluginProviderCollection { fileSystemPluginProvider };

            pluginContext.Initialize(pluginProviders);
            pluginContext.Start();
        }

        public override void Stop()
        {
            pluginContext.Stop();
        }
    }
}