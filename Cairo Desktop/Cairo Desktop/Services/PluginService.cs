using CairoDesktop.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace CairoDesktop.Services
{
    public sealed class PluginService : ShellService
    {
        [ImportMany(typeof(ShellExtension))]
        private IEnumerable<ShellExtension> _shellExtensions;

        private string userExtensionsPath;
        private string systemExtensionsPath;

        private AggregateCatalog catalog;
        private CompositionContainer container;

        public PluginService()
        {
            systemExtensionsPath = Path.Combine(CairoApplication.StartupPath, "Extensions");
            userExtensionsPath = Path.Combine(CairoApplication.CairoApplicationDataFolder, "Extensions");

            CairoApplication.Current.ShellServices.Add(GetType(), this);
        }

        public IEnumerable<ShellExtension> ShellExtensions { get => _shellExtensions; private set => _shellExtensions = value; }

        public override void Start()
        {
            catalog = new AggregateCatalog(new AssemblyCatalog(System.Reflection.Assembly.GetEntryAssembly()));

            if (Directory.Exists(systemExtensionsPath))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(systemExtensionsPath));
            }

            if (Directory.Exists(userExtensionsPath))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(userExtensionsPath));
            }


            container = new CompositionContainer(catalog);
            container.ComposeParts(this);

            foreach (ShellExtension shellExtension in ShellExtensions)
            {
                shellExtension.Start();
                CairoApplication.Current.ShellExtensions.Add(shellExtension);
            }

        }

        public override void Stop()
        {
            foreach (ShellExtension shellExtension in ShellExtensions)
            {
                CairoApplication.Current.ShellExtensions.Remove(shellExtension);
                shellExtension.Stop();
            }
        }
    }
}