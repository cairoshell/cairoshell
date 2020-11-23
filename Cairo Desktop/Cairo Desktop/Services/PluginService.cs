using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using CairoDesktop.ObjectModel;

namespace CairoDesktop.Services
{
    public sealed class PluginService : ShellService
    {
        private readonly _CairoShell _cairoShell;

        [ImportMany(typeof(ShellExtension))]
        private IEnumerable<ShellExtension> _shellExtensions;

        private string userExtensionsPath;
        private string systemExtensionsPath;

        private AggregateCatalog catalog;
        private CompositionContainer container;

        public PluginService(_CairoShell cairoShell)
        {
            systemExtensionsPath = Path.Combine(App.StartupPath, "Extensions");
            userExtensionsPath = Path.Combine(App.CairoApplicationDataFolder, "Extensions");

            cairoShell.ShellServices.Add(GetType(), this);
            _cairoShell = cairoShell;
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
                _cairoShell.ShellExtensions.Add(shellExtension);
            }

        }

        public override void Stop()
        {
            foreach (ShellExtension shellExtension in ShellExtensions)
            {
                _cairoShell.ShellExtensions.Remove(shellExtension);
                shellExtension.Stop();
            }
        }
    }
}