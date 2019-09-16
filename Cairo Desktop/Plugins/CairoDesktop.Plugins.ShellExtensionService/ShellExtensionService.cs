using CairoDesktop.Extensibility.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace CairoDesktop.Plugins.ShellExtensionSupport
{
    public sealed class ShellExtensionService : ShellService
    {
        [ImportMany(typeof(ShellExtension))]
        private IEnumerable<ShellExtension> _shellExtensions;

        private string pluginsPath;
        private AggregateCatalog catalog;
        private CompositionContainer container;

        public ShellExtensionService()
        {
            pluginsPath = Path.Combine(_CairoShell.StartupPath, "Extensions");
            if (!Directory.Exists(pluginsPath))
                Directory.CreateDirectory(pluginsPath);

            _CairoShell.Instance.ShellServices.Add(GetType(), this);
        }

        public IEnumerable<ShellExtension> ShellExtensions { get => _shellExtensions; private set => _shellExtensions = value; }

        public override void Start()
        {
            catalog = new AggregateCatalog(new DirectoryCatalog(pluginsPath));
            container = new CompositionContainer(catalog);
            container.ComposeParts(this);

            foreach (ShellExtension shellExtension in ShellExtensions)
                shellExtension.Start();
        }

        public override void Stop()
        {
            foreach (ShellExtension shellExtension in ShellExtensions)
                shellExtension.Stop();
        }
    }
}
