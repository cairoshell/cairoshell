using CairoDesktop.ObjectModel;
using System.Collections.Generic;

namespace CairoDesktop.Services
{
    public sealed class ExtensionService : ShellService
    {
        private readonly CairoApplication _app;
        private readonly IEnumerable<ShellExtension> _extensions;

        public ExtensionService(CairoApplication app, IEnumerable<ShellExtension> extensions)
        {
            _app = app;
            _extensions = extensions;
        }

        public override void Start()
        {
            foreach (var shellExtension in _extensions)
            {
                shellExtension.Start();
                _app.Extensions.Add(shellExtension);
            }

        }

        public override void Stop()
        {
            foreach (var shellExtension in _extensions)
            {
                _app.Extensions.Remove(shellExtension);
                shellExtension.Stop();
            }
        }
    }
}